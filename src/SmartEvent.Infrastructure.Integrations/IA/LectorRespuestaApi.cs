using System.Text.Json;

namespace SmartEvent.Infrastructure.Integrations.IA;

/// <summary>Contenido util extraido de la respuesta del proveedor.</summary>
internal sealed class ContenidoModelo
{
    public string? Texto { get; init; }

    /// <summary>Negativa explicita del modelo a responder. Es un caso previsto por el contrato.</summary>
    public string? Rechazo { get; init; }

    public int? TokensEntrada { get; init; }
    public int? TokensSalida { get; init; }

    public bool TieneTexto => !string.IsNullOrWhiteSpace(Texto);
}

/// <summary>
/// Interpretacion de la respuesta HTTP del proveedor.
///
/// Se navega el JSON con JsonDocument en lugar de deserializar a clases fijas. El motivo es
/// practico: la aplicacion puede apuntar a distintos proveedores compatibles y todos anaden
/// campos propios. Con una clase rigida, un campo extra o una forma ligeramente distinta
/// rompe la deserializacion entera; navegando de forma defensiva se extrae lo que interesa y
/// se ignora el resto.
///
/// Se cubren las dos formas del contrato:
///   - Responses API      : output[].content[] con type "output_text" (o "refusal").
///   - Chat Completions   : choices[0].message.content (o message.refusal).
/// </summary>
internal static class LectorRespuestaApi
{
    public static ContenidoModelo Leer(string cuerpoJson)
    {
        using var documento = JsonDocument.Parse(cuerpoJson);
        var raiz = documento.RootElement;

        var (tokensEntrada, tokensSalida) = LeerUso(raiz);

        // ---------------------------------------------------------------- Responses API
        if (raiz.TryGetProperty("output", out var salida) && salida.ValueKind == JsonValueKind.Array)
        {
            foreach (var elemento in salida.EnumerateArray())
            {
                if (!elemento.TryGetProperty("content", out var contenido) ||
                    contenido.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var parte in contenido.EnumerateArray())
                {
                    var tipo = parte.TryGetProperty("type", out var t) ? t.GetString() : null;

                    if (tipo == "refusal" && parte.TryGetProperty("refusal", out var rechazo))
                    {
                        return new ContenidoModelo
                        {
                            Rechazo = rechazo.GetString(),
                            TokensEntrada = tokensEntrada,
                            TokensSalida = tokensSalida
                        };
                    }

                    if (tipo == "output_text" && parte.TryGetProperty("text", out var texto))
                    {
                        return new ContenidoModelo
                        {
                            Texto = texto.GetString(),
                            TokensEntrada = tokensEntrada,
                            TokensSalida = tokensSalida
                        };
                    }
                }
            }
        }

        // Algunos proveedores exponen ademas un atajo con el texto ya concatenado.
        if (raiz.TryGetProperty("output_text", out var atajo))
        {
            var texto = atajo.ValueKind switch
            {
                JsonValueKind.String => atajo.GetString(),
                JsonValueKind.Array => string.Concat(atajo.EnumerateArray().Select(e => e.GetString())),
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(texto))
            {
                return new ContenidoModelo
                {
                    Texto = texto,
                    TokensEntrada = tokensEntrada,
                    TokensSalida = tokensSalida
                };
            }
        }

        // ------------------------------------------------------------- Chat Completions
        if (raiz.TryGetProperty("choices", out var opciones) &&
            opciones.ValueKind == JsonValueKind.Array &&
            opciones.GetArrayLength() > 0)
        {
            var primera = opciones[0];

            if (primera.TryGetProperty("message", out var mensaje))
            {
                if (mensaje.TryGetProperty("refusal", out var rechazo) &&
                    rechazo.ValueKind == JsonValueKind.String)
                {
                    return new ContenidoModelo
                    {
                        Rechazo = rechazo.GetString(),
                        TokensEntrada = tokensEntrada,
                        TokensSalida = tokensSalida
                    };
                }

                if (mensaje.TryGetProperty("content", out var contenido) &&
                    contenido.ValueKind == JsonValueKind.String)
                {
                    return new ContenidoModelo
                    {
                        Texto = contenido.GetString(),
                        TokensEntrada = tokensEntrada,
                        TokensSalida = tokensSalida
                    };
                }
            }
        }

        return new ContenidoModelo
        {
            TokensEntrada = tokensEntrada,
            TokensSalida = tokensSalida
        };
    }

    /// <summary>
    /// Consumo de tokens. La Responses API usa input_tokens/output_tokens y Chat Completions
    /// prompt_tokens/completion_tokens; se aceptan ambos porque la auditoria los guarda cuando
    /// el proveedor los informa y los deja nulos cuando no.
    /// </summary>
    private static (int? entrada, int? salida) LeerUso(JsonElement raiz)
    {
        if (!raiz.TryGetProperty("usage", out var uso) || uso.ValueKind != JsonValueKind.Object)
        {
            return (null, null);
        }

        var entrada = LeerEntero(uso, "input_tokens") ?? LeerEntero(uso, "prompt_tokens");
        var salida = LeerEntero(uso, "output_tokens") ?? LeerEntero(uso, "completion_tokens");

        return (entrada, salida);
    }

    private static int? LeerEntero(JsonElement objeto, string propiedad) =>
        objeto.TryGetProperty(propiedad, out var valor) && valor.TryGetInt32(out var numero)
            ? numero
            : null;

    /// <summary>
    /// Mensaje de error legible que devuelve el proveedor en el cuerpo cuando falla.
    /// Se extrae para poder auditarlo, pero nunca contiene la clave: es la descripcion del
    /// error, no la peticion.
    /// </summary>
    public static string? LeerMensajeDeError(string? cuerpoJson)
    {
        if (string.IsNullOrWhiteSpace(cuerpoJson))
        {
            return null;
        }

        try
        {
            using var documento = JsonDocument.Parse(cuerpoJson);

            if (documento.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.String)
                {
                    return error.GetString();
                }

                if (error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out var mensaje))
                {
                    return mensaje.GetString();
                }
            }

            if (documento.RootElement.TryGetProperty("message", out var directo))
            {
                return directo.GetString();
            }
        }
        catch (JsonException)
        {
            // El proveedor devolvio algo que no es JSON (por ejemplo, una pagina HTML de error
            // de un proxy). No hay nada util que extraer.
        }

        return null;
    }

    /// <summary>
    /// Aisla el objeto JSON dentro del texto devuelto.
    ///
    /// Con salidas estructuradas estrictas el texto YA es el JSON puro. Esta funcion es una
    /// red de seguridad para proveedores compatibles que anaden un bloque de codigo con
    /// comillas invertidas o una frase antes del objeto: en lugar de fallar, se recupera el
    /// primer objeto con llaves balanceadas. Si no hay ninguno, se devuelve null y el analisis
    /// se audita como respuesta invalida.
    /// </summary>
    public static string? AislarObjetoJson(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        var contenido = texto.Trim();

        // Caso habitual: el texto ya es JSON.
        if (contenido.StartsWith('{') && contenido.EndsWith('}'))
        {
            return contenido;
        }

        var inicio = contenido.IndexOf('{');

        if (inicio < 0)
        {
            return null;
        }

        var profundidad = 0;
        var dentroDeCadena = false;
        var escapado = false;

        for (var i = inicio; i < contenido.Length; i++)
        {
            var caracter = contenido[i];

            if (dentroDeCadena)
            {
                if (escapado) escapado = false;
                else if (caracter == '\\') escapado = true;
                else if (caracter == '"') dentroDeCadena = false;

                continue;
            }

            switch (caracter)
            {
                case '"':
                    dentroDeCadena = true;
                    break;
                case '{':
                    profundidad++;
                    break;
                case '}':
                    profundidad--;
                    if (profundidad == 0)
                    {
                        return contenido[inicio..(i + 1)];
                    }
                    break;
            }
        }

        return null;
    }
}
