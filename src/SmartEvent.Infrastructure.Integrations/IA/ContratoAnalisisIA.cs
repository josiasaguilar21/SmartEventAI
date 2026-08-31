using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartEvent.Core.Dtos;

namespace SmartEvent.Infrastructure.Integrations.IA;

/// <summary>
/// Contrato que se le impone al modelo: el JSON Schema de la respuesta y el prompt.
///
/// Se mantiene en un archivo aparte y con VERSION EXPLICITA porque la version se persiste en
/// evt.AnalisisIA junto a cada resultado. Si mañana se ajusta el prompt, los analisis
/// antiguos siguen siendo interpretables: se sabe con que instrucciones se generaron.
/// </summary>
internal static class ContratoAnalisisIA
{
    /// <summary>
    /// Version del prompt y del esquema. Debe incrementarse ante cualquier cambio en el texto
    /// de las instrucciones o en la forma de la respuesta.
    /// </summary>
    public const string Version = "v1.0";

    /// <summary>Nombre del esquema que se envia en la peticion.</summary>
    public const string NombreEsquema = "analisis_reserva";

    private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;

    /// <summary>
    /// JSON Schema en modo estricto.
    ///
    /// En modo strict el proveedor GARANTIZA que la respuesta tendra exactamente estas claves,
    /// con estos tipos y sin propiedades extra. Por eso todas las propiedades aparecen en
    /// "required" y "additionalProperties" es false: son requisitos del modo estricto.
    ///
    /// Los limites numericos (resumen de 300 caracteres, de 0 a 5 alertas, de 1 a 5
    /// recomendaciones) NO se expresan como minItems/maxItems: esas palabras clave no estan
    /// soportadas de forma uniforme por todos los proveedores compatibles y su presencia hace
    /// que algunos rechacen el esquema. Se piden en la descripcion de cada campo y, sobre
    /// todo, se COMPRUEBAN en el cliente con AnalisisIARespuesta.Validar antes de mostrar o
    /// persistir nada. La aplicacion no confia en que el proveedor cumpla su palabra.
    /// </summary>
    public static JsonNode ConstruirEsquema() => new JsonObject
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["required"] = new JsonArray("nivelRiesgo", "resumen", "alertas", "recomendaciones", "correoSugerido"),
        ["properties"] = new JsonObject
        {
            ["nivelRiesgo"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray("BAJO", "MEDIO", "ALTO"),
                ["description"] = "Nivel de riesgo operativo global de la reserva."
            },
            ["resumen"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Sintesis del analisis en espanol, maximo 300 caracteres, sin saltos de linea."
            },
            ["alertas"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Entre 0 y 5 riesgos concretos detectados. Arreglo vacio si no hay ninguno."
            },
            ["recomendaciones"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Entre 1 y 5 acciones operativas concretas y accionables."
            },
            ["correoSugerido"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Borrador profesional de correo para el cliente, en espanol. " +
                                  "Es solo una sugerencia: la aplicacion nunca lo envia automaticamente."
            }
        }
    };

    /// <summary>
    /// Instrucciones del sistema. Delimitan lo que el modelo puede hacer: analizar y sugerir.
    /// Se le indica de forma explicita que no decide y que no debe inventar datos, porque la
    /// regla de negocio es que la IA asesora y la persona conserva el control.
    /// </summary>
    public const string InstruccionesSistema =
        "Eres un analista de operaciones de una empresa que administra reservas de salones y recursos " +
        "para eventos corporativos. Recibes los datos de UNA reserva y evaluas su riesgo operativo.\n\n" +
        "Criterios a considerar:\n" +
        "- Ocupacion del salon respecto de su capacidad: por encima del 90 por ciento el montaje se complica.\n" +
        "- Anticipacion: menos de 7 dias deja poco margen para coordinar proveedores.\n" +
        "- Duracion del evento y su efecto sobre turnos de personal y catering.\n" +
        "- Holgura de cada recurso frente a su inventario total.\n" +
        "- Descuentos aplicados y su efecto sobre el margen.\n\n" +
        "Reglas estrictas:\n" +
        "1. Responde UNICAMENTE con el objeto JSON del esquema indicado. Sin texto adicional.\n" +
        "2. Escribe en espanol neutro y profesional.\n" +
        "3. El campo resumen no debe superar los 300 caracteres.\n" +
        "4. Incluye entre 0 y 5 alertas y entre 1 y 5 recomendaciones.\n" +
        "5. No inventes datos que no aparezcan en la entrada. Si algo no consta, no lo supongas.\n" +
        "6. No decides nada: no confirmas, no cancelas y no modificas importes. Solo informas " +
        "para que una persona tome la decision.";

    /// <summary>
    /// Construye la entrada del usuario. Se envia como JSON compacto y no como prosa porque el
    /// modelo interpreta mejor una estructura explicita y porque deja a la vista, para
    /// cualquiera que audite el codigo, exactamente que datos salen de la aplicacion.
    /// </summary>
    public static string ConstruirEntrada(AnalisisIASolicitud solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        var recursos = new JsonArray();

        foreach (var recurso in solicitud.Recursos)
        {
            recursos.Add(new JsonObject
            {
                ["recurso"] = recurso.Nombre,
                ["tipo"] = recurso.Tipo,
                ["cantidadSolicitada"] = recurso.Cantidad,
                ["inventarioTotal"] = recurso.StockTotal,
                ["porcentajeDescuento"] = recurso.PorcentajeDescuento
            });
        }

        var datos = new JsonObject
        {
            ["codigoReserva"] = solicitud.CodigoReserva,
            ["cliente"] = solicitud.NombreCliente,
            ["salon"] = solicitud.Salon,
            ["capacidadSalon"] = solicitud.CapacidadSalon,
            ["numeroInvitados"] = solicitud.NumeroInvitados,
            ["porcentajeOcupacion"] = solicitud.PorcentajeOcupacion,
            ["fechaEvento"] = solicitud.FechaEvento.ToString("yyyy-MM-dd", Invariante),
            ["horaInicio"] = solicitud.HoraInicio.ToString("HH:mm", Invariante),
            ["horaFin"] = solicitud.HoraFin.ToString("HH:mm", Invariante),
            ["horasDuracion"] = solicitud.HorasDuracion,
            ["diasHastaEvento"] = solicitud.DiasHastaEvento,
            ["estado"] = solicitud.Estado,
            ["subtotal"] = solicitud.Subtotal,
            ["descuentoGlobal"] = solicitud.Descuento,
            ["total"] = solicitud.Total,
            ["observacion"] = solicitud.Observacion ?? string.Empty,
            ["recursos"] = recursos
        };

        var texto = new StringBuilder()
            .AppendLine("Analiza la siguiente reserva y responde con el JSON del esquema indicado.")
            .AppendLine()
            .Append(datos.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        return texto.ToString();
    }
}
