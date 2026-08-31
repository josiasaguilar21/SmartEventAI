using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Infrastructure.Integrations.Configuracion;

namespace SmartEvent.Infrastructure.Integrations.IA;

/// <summary>
/// Cliente del servicio de analisis con IA.
///
/// Habla el contrato de la Responses API con SALIDAS ESTRUCTURADAS: la peticion lleva
/// text.format con type json_schema y strict en true, de modo que el proveedor garantiza la
/// forma de la respuesta en lugar de dejarla al azar del texto libre.
///
/// GARANTIA PRINCIPAL: este metodo NUNCA lanza por un fallo del proveedor. Falta de clave,
/// tiempo de espera agotado, sin conexion, error HTTP, limite de uso, respuesta vacia, JSON
/// invalido o negativa del modelo se traducen a un AnalisisIAResultado con Exitoso en false y
/// un mensaje tecnico controlado que se audita (CA-09). La unica excepcion que sale de aqui es
/// OperationCanceledException cuando el USUARIO cancela, porque eso no es un fallo.
///
/// LA IA SOLO ASESORA: esta clase no tiene acceso a la base de datos, no cambia estados y no
/// toca importes. Devuelve texto estructurado; que hacer con el lo decide una persona.
/// </summary>
public sealed class ServicioAnalisisIAHttp : IServicioAnalisisIA, IDisposable
{
    private readonly OpcionesAnalisisIA _opciones;
    private readonly IRegistroEventos _registro;
    private readonly HttpClient _http;
    private readonly bool _httpPropio;

    /// <summary>
    /// Recuerda si el proveedor expone /responses. Se resuelve en la primera llamada y evita
    /// repetir el tanteo en cada analisis.
    /// </summary>
    private bool _usarChatCompletions;

    private bool _endpointResuelto;
    private bool _liberado;

    public ServicioAnalisisIAHttp(OpcionesAnalisisIA opciones, IRegistroEventos registro, HttpClient? http = null)
    {
        _opciones = opciones;
        _registro = registro;

        _httpPropio = http is null;
        _http = http ?? new HttpClient();

        // El tiempo de espera se fija en el cliente: si el proveedor no responde, la llamada se
        // corta sola y la interfaz no queda esperando indefinidamente.
        _http.Timeout = TimeSpan.FromSeconds(Math.Clamp(_opciones.TimeoutSegundos, 5, 300));

        _usarChatCompletions = _opciones.UsarChatCompletions;
        _endpointResuelto = _opciones.UsarChatCompletions;
    }

    public bool EstaConfigurado => _opciones.EstaCompleta;

    public string Modelo => _opciones.Modelo;

    public async Task<AnalisisIAResultado> AnalizarAsync(AnalisisIASolicitud solicitud, CancellationToken cancelacion)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        if (!EstaConfigurado)
        {
            return Fallo("No hay una clave de API configurada. Defina la variable de entorno " +
                         "OPENAI_API_KEY y reinicie la aplicacion.");
        }

        try
        {
            var respuesta = await EjecutarConReintentosAsync(solicitud, cancelacion).ConfigureAwait(false);

            if (respuesta.Error is not null)
            {
                return Fallo(respuesta.Error);
            }

            return InterpretarContenido(respuesta.Contenido!);
        }
        catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
        {
            // Cancelacion pedida por el usuario con el boton Cancelar. Sube tal cual.
            throw;
        }
        catch (OperationCanceledException ex)
        {
            // Cancelacion NO pedida por el usuario: es el tiempo de espera del HttpClient.
            _registro.Advertencia("Tiempo de espera agotado en el analisis con IA.", ex);

            return Fallo($"El servicio de IA no respondio dentro de los {_opciones.TimeoutSegundos} " +
                         "segundos configurados. Puede reintentar o confirmar con una justificacion de contingencia.");
        }
        catch (HttpRequestException ex)
        {
            _registro.Advertencia("No hay conexion con el proveedor del modelo de IA.", ex);

            return Fallo("No fue posible conectar con el servicio de IA. Verifique su conexion a internet " +
                         "y vuelva a intentarlo.");
        }
        catch (Exception ex)
        {
            _registro.Error("Fallo inesperado durante el analisis con IA.", ex);

            return Fallo("Ocurrio un problema al ejecutar el analisis. El detalle tecnico quedo registrado " +
                         "en el log local de la aplicacion.");
        }
    }

    // ------------------------------------------------------------------------- envio HTTP

    private sealed class RespuestaHttp
    {
        public ContenidoModelo? Contenido { get; init; }
        public string? Error { get; init; }
    }

    /// <summary>
    /// Envia la peticion y reintenta ante errores TEMPORALES (429 por limite de uso y 5xx del
    /// servidor) con espera creciente. Los errores permanentes (401, 403, 404) no se reintentan:
    /// insistir no los va a arreglar y solo retrasa el mensaje al usuario.
    /// </summary>
    private async Task<RespuestaHttp> EjecutarConReintentosAsync(AnalisisIASolicitud solicitud,
                                                                 CancellationToken cancelacion)
    {
        var intentos = Math.Max(0, _opciones.MaximoReintentos) + 1;
        string? ultimoError = null;

        for (var intento = 1; intento <= intentos; intento++)
        {
            using var peticion = ConstruirPeticion(solicitud);
            using var respuesta = await _http.SendAsync(peticion, HttpCompletionOption.ResponseContentRead, cancelacion)
                                             .ConfigureAwait(false);

            var cuerpo = await respuesta.Content.ReadAsStringAsync(cancelacion).ConfigureAwait(false);

            if (respuesta.IsSuccessStatusCode)
            {
                return new RespuestaHttp { Contenido = LectorRespuestaApi.Leer(cuerpo) };
            }

            // El proveedor no expone /responses: se cambia a /chat/completions y se repite el
            // intento con el mismo contenido. Solo ocurre una vez por ejecucion.
            if (!_endpointResuelto && !_usarChatCompletions && EsEndpointNoDisponible(respuesta.StatusCode))
            {
                _usarChatCompletions = true;
                _endpointResuelto = true;

                _registro.Informacion(
                    "El proveedor de IA no expone /responses; se usara /chat/completions con el mismo esquema JSON.");

                intento--;   // este intento no cuenta: fue un tanteo del endpoint
                continue;
            }

            _endpointResuelto = true;
            ultimoError = TraducirErrorHttp(respuesta.StatusCode, cuerpo);

            if (!EsTemporal(respuesta.StatusCode) || intento == intentos)
            {
                return new RespuestaHttp { Error = ultimoError };
            }

            var espera = CalcularEspera(respuesta, intento);

            _registro.Advertencia(
                $"El proveedor de IA respondio {(int)respuesta.StatusCode}. " +
                $"Reintento {intento} de {intentos - 1} en {espera.TotalSeconds:0.#} s.");

            // Espera asincronica: nunca Thread.Sleep, que bloquearia el hilo de la interfaz.
            await Task.Delay(espera, cancelacion).ConfigureAwait(false);
        }

        return new RespuestaHttp { Error = ultimoError ?? "No fue posible completar la solicitud al servicio de IA." };
    }

    private HttpRequestMessage ConstruirPeticion(AnalisisIASolicitud solicitud)
    {
        var ruta = _usarChatCompletions ? "/chat/completions" : "/responses";
        var url = _opciones.BaseUrl.TrimEnd('/') + ruta;

        var cuerpo = _usarChatCompletions
            ? ConstruirCuerpoChatCompletions(solicitud)
            : ConstruirCuerpoResponses(solicitud);

        var peticion = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(cuerpo.ToJsonString(), Encoding.UTF8, "application/json")
        };

        // La clave viaja en la cabecera Authorization y en ningun otro sitio: nunca en la URL
        // (quedaria en registros de servidores y proxies) ni en el cuerpo.
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opciones.ApiKey);

        return peticion;
    }

    /// <summary>Cuerpo de la Responses API: el contrato que pide el enunciado.</summary>
    private JsonObject ConstruirCuerpoResponses(AnalisisIASolicitud solicitud) => new()
    {
        ["model"] = _opciones.Modelo,
        ["input"] = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "system",
                ["content"] = ContratoAnalisisIA.InstruccionesSistema
            },
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = ContratoAnalisisIA.ConstruirEntrada(solicitud)
            }
        },
        ["text"] = new JsonObject
        {
            ["format"] = new JsonObject
            {
                ["type"] = "json_schema",
                ["name"] = ContratoAnalisisIA.NombreEsquema,
                ["strict"] = true,
                ["schema"] = ContratoAnalisisIA.ConstruirEsquema()
            }
        }
    };

    /// <summary>
    /// Cuerpo equivalente para /chat/completions. El esquema es EL MISMO; solo cambia donde se
    /// declara: response_format.json_schema en lugar de text.format.
    /// </summary>
    private JsonObject ConstruirCuerpoChatCompletions(AnalisisIASolicitud solicitud) => new()
    {
        ["model"] = _opciones.Modelo,
        ["messages"] = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "system",
                ["content"] = ContratoAnalisisIA.InstruccionesSistema
            },
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = ContratoAnalisisIA.ConstruirEntrada(solicitud)
            }
        },
        ["response_format"] = new JsonObject
        {
            ["type"] = "json_schema",
            ["json_schema"] = new JsonObject
            {
                ["name"] = ContratoAnalisisIA.NombreEsquema,
                ["strict"] = true,
                ["schema"] = ContratoAnalisisIA.ConstruirEsquema()
            }
        }
    };

    // ---------------------------------------------------------------- lectura del resultado

    /// <summary>
    /// Valida y deserializa la respuesta ANTES de darla por buena. Aunque el proveedor
    /// garantice el esquema, la aplicacion comprueba el contrato por su cuenta: es la
    /// diferencia entre confiar y verificar.
    /// </summary>
    private AnalisisIAResultado InterpretarContenido(ContenidoModelo contenido)
    {
        if (contenido.Rechazo is not null)
        {
            return Fallo($"El modelo se nego a responder: {contenido.Rechazo}",
                         contenido.TokensEntrada, contenido.TokensSalida);
        }

        if (!contenido.TieneTexto)
        {
            return Fallo("El servicio de IA devolvio una respuesta vacia.",
                         contenido.TokensEntrada, contenido.TokensSalida);
        }

        var json = LectorRespuestaApi.AislarObjetoJson(contenido.Texto);

        if (json is null)
        {
            return Fallo("La respuesta del servicio de IA no contiene un objeto JSON valido.",
                         contenido.TokensEntrada, contenido.TokensSalida);
        }

        AnalisisIARespuesta? respuesta;

        try
        {
            respuesta = JsonSerializer.Deserialize<AnalisisIARespuesta>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _registro.Advertencia("El JSON devuelto por el servicio de IA no pudo interpretarse.", ex);

            return Fallo("La respuesta del servicio de IA no respeta el formato esperado.",
                         contenido.TokensEntrada, contenido.TokensSalida);
        }

        if (respuesta is null)
        {
            return Fallo("La respuesta del servicio de IA llego vacia tras interpretarla.",
                         contenido.TokensEntrada, contenido.TokensSalida);
        }

        if (!respuesta.Validar(out var motivo))
        {
            _registro.Advertencia($"La respuesta del modelo incumple el contrato: {motivo}");

            return Fallo($"La respuesta del modelo no cumple el contrato acordado. {motivo}",
                         contenido.TokensEntrada, contenido.TokensSalida);
        }

        return new AnalisisIAResultado
        {
            Exitoso = true,
            Modelo = _opciones.Modelo,
            PromptVersion = ContratoAnalisisIA.Version,
            Respuesta = respuesta,
            RespuestaJson = json,
            TokensEntrada = contenido.TokensEntrada,
            TokensSalida = contenido.TokensSalida
        };
    }

    // --------------------------------------------------------------------------- auxiliares

    private static bool EsEndpointNoDisponible(HttpStatusCode codigo) =>
        codigo is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented;

    private static bool EsTemporal(HttpStatusCode codigo) =>
        codigo == HttpStatusCode.TooManyRequests || (int)codigo >= 500;

    /// <summary>
    /// Espera antes del siguiente intento. Se respeta la cabecera Retry-After si el proveedor
    /// la envia; en su defecto se usa retroceso exponencial (2 s, 4 s, 8 s...) acotado.
    /// </summary>
    private static TimeSpan CalcularEspera(HttpResponseMessage respuesta, int intento)
    {
        var sugerida = respuesta.Headers.RetryAfter?.Delta;

        if (sugerida is { TotalSeconds: > 0 })
        {
            return sugerida.Value > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : sugerida.Value;
        }

        var segundos = Math.Min(30, Math.Pow(2, intento));
        return TimeSpan.FromSeconds(segundos);
    }

    /// <summary>
    /// Traduce el codigo HTTP a un mensaje util para el usuario. El texto que devuelve el
    /// proveedor se incorpora solo cuando es informativo, y nunca contiene la clave: es la
    /// descripcion del error, no el eco de la peticion.
    /// </summary>
    private static string TraducirErrorHttp(HttpStatusCode codigo, string cuerpo)
    {
        var detalle = LectorRespuestaApi.LeerMensajeDeError(cuerpo);
        var sufijo = string.IsNullOrWhiteSpace(detalle) ? string.Empty : $" Detalle: {Recortar(detalle)}";

        return codigo switch
        {
            HttpStatusCode.Unauthorized =>
                "La clave de API configurada no es valida o fue revocada. Genere una nueva y actualice " +
                "la variable de entorno OPENAI_API_KEY.",

            HttpStatusCode.Forbidden =>
                "La clave de API no tiene permiso para usar este modelo." + sufijo,

            HttpStatusCode.NotFound =>
                $"El modelo o el endpoint configurados no existen en el proveedor.{sufijo}",

            HttpStatusCode.TooManyRequests =>
                "Se alcanzo el limite de uso del servicio de IA. Espere unos minutos y vuelva a " +
                "intentarlo, o confirme la reserva con una justificacion de contingencia.",

            HttpStatusCode.BadRequest =>
                $"El servicio de IA rechazo la solicitud.{sufijo}",

            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout =>
                "El servicio de IA tardo demasiado en responder. Puede reintentarlo.",

            _ when (int)codigo >= 500 =>
                $"El servicio de IA presenta un problema temporal (codigo {(int)codigo}). Reintente en unos minutos.",

            _ =>
                $"El servicio de IA respondio con el codigo {(int)codigo}.{sufijo}"
        };
    }

    private static string Recortar(string texto) =>
        texto.Length <= 200 ? texto : texto[..200] + "...";

    private AnalisisIAResultado Fallo(string error, int? tokensEntrada = null, int? tokensSalida = null) => new()
    {
        Exitoso = false,
        Modelo = _opciones.Modelo,
        PromptVersion = ContratoAnalisisIA.Version,
        Error = error,
        TokensEntrada = tokensEntrada,
        TokensSalida = tokensSalida
    };

    public void Dispose()
    {
        if (_liberado)
        {
            return;
        }

        _liberado = true;

        // Solo se libera el HttpClient si lo creo esta clase. Si lo inyectaron desde fuera,
        // su ciclo de vida pertenece a quien lo creo.
        if (_httpPropio)
        {
            _http.Dispose();
        }
    }
}
