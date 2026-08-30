using System.Text.Json.Serialization;
using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Dtos;

/// <summary>
/// CONTRATO ESTRUCTURADO que se le exige al modelo mediante JSON Schema estricto.
/// Los nombres JSON coinciden exactamente con los del esquema enviado en la peticion.
///
/// La respuesta se valida con <see cref="Validar"/> ANTES de mostrarse o persistirse: aunque
/// el proveedor garantice el esquema, la aplicacion no confia en ello y comprueba los limites
/// de negocio (longitud del resumen, cantidad de alertas y recomendaciones, nivel de riesgo).
/// </summary>
public sealed class AnalisisIARespuesta
{
    [JsonPropertyName("nivelRiesgo")]
    public string NivelRiesgo { get; set; } = string.Empty;

    [JsonPropertyName("resumen")]
    public string Resumen { get; set; } = string.Empty;

    [JsonPropertyName("alertas")]
    public List<string> Alertas { get; set; } = new();

    [JsonPropertyName("recomendaciones")]
    public List<string> Recomendaciones { get; set; } = new();

    [JsonPropertyName("correoSugerido")]
    public string CorreoSugerido { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de riesgo convertido al enum del dominio.
    ///
    /// Es una propiedad DERIVADA del campo de texto, no un valor que haya que recordar asignar:
    /// si dependiera de que alguien llame antes a <see cref="Validar"/>, un analisis de riesgo
    /// ALTO podria acabar auditado como BAJO por un simple olvido en el orden de las llamadas.
    /// Cuando el texto no pertenece al contrato se devuelve Bajo, pero esa respuesta nunca llega
    /// a persistirse porque <see cref="Validar"/> la rechaza antes.
    /// </summary>
    [JsonIgnore]
    public Enums.NivelRiesgo NivelRiesgoEnum =>
        ConversionesEnum.TryANivelRiesgo(NivelRiesgo, out var nivel) ? nivel : Enums.NivelRiesgo.Bajo;

    public const int MaximoCaracteresResumen = 300;
    public const int MaximoAlertas = 5;
    public const int MinimoRecomendaciones = 1;
    public const int MaximoRecomendaciones = 5;

    /// <summary>
    /// Comprueba que la respuesta cumple el contrato. Devuelve false con un motivo legible en
    /// lugar de lanzar, porque una respuesta mal formada del modelo es un caso esperado que la
    /// aplicacion debe auditar y mostrar sin interrumpir el trabajo del usuario (CA-09).
    /// </summary>
    public bool Validar(out string motivo)
    {
        if (!ConversionesEnum.TryANivelRiesgo(NivelRiesgo, out _))
        {
            motivo = $"El campo nivelRiesgo tiene un valor fuera del contrato: '{NivelRiesgo}'.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Resumen))
        {
            motivo = "El campo resumen llego vacio.";
            return false;
        }

        if (Resumen.Length > MaximoCaracteresResumen)
        {
            motivo = $"El resumen supera los {MaximoCaracteresResumen} caracteres permitidos ({Resumen.Length}).";
            return false;
        }

        Alertas.RemoveAll(string.IsNullOrWhiteSpace);
        Recomendaciones.RemoveAll(string.IsNullOrWhiteSpace);

        if (Alertas.Count > MaximoAlertas)
        {
            motivo = $"Se recibieron {Alertas.Count} alertas y el maximo permitido es {MaximoAlertas}.";
            return false;
        }

        if (Recomendaciones.Count < MinimoRecomendaciones || Recomendaciones.Count > MaximoRecomendaciones)
        {
            motivo = $"Se esperaban entre {MinimoRecomendaciones} y {MaximoRecomendaciones} recomendaciones y llegaron {Recomendaciones.Count}.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CorreoSugerido))
        {
            motivo = "El campo correoSugerido llego vacio.";
            return false;
        }

        motivo = string.Empty;
        return true;
    }
}

/// <summary>
/// Datos MINIMOS de la reserva que se envian al modelo. Es una proyeccion deliberadamente
/// reducida: se excluyen identificacion, correo y telefono del cliente porque el analisis de
/// riesgo operativo no los necesita. Enviar solo lo necesario es parte del requisito.
/// </summary>
public sealed class AnalisisIASolicitud
{
    public required string CodigoReserva { get; init; }
    public required string NombreCliente { get; init; }
    public required string Salon { get; init; }
    public required int CapacidadSalon { get; init; }
    public required DateOnly FechaEvento { get; init; }
    public required TimeOnly HoraInicio { get; init; }
    public required TimeOnly HoraFin { get; init; }
    public required int NumeroInvitados { get; init; }
    public required string Estado { get; init; }
    public required decimal Subtotal { get; init; }
    public required decimal Descuento { get; init; }
    public required decimal Total { get; init; }
    public string? Observacion { get; init; }
    public required IReadOnlyList<AnalisisIARecursoSolicitud> Recursos { get; init; }

    public double HorasDuracion => (HoraFin.ToTimeSpan() - HoraInicio.ToTimeSpan()).TotalHours;

    /// <summary>Ocupacion del salon en porcentaje; es una de las senales de riesgo mas utiles.</summary>
    public double PorcentajeOcupacion =>
        CapacidadSalon <= 0 ? 0 : Math.Round(NumeroInvitados * 100d / CapacidadSalon, 1);

    public int DiasHastaEvento =>
        FechaEvento.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
}

/// <summary>Linea de recurso incluida en la solicitud de analisis.</summary>
public sealed class AnalisisIARecursoSolicitud
{
    public required string Nombre { get; init; }
    public required string Tipo { get; init; }
    public required int Cantidad { get; init; }
    public required int StockTotal { get; init; }
    public required decimal PorcentajeDescuento { get; init; }
}

/// <summary>
/// Resultado completo de un analisis, exitoso o no. Siempre se persiste en evt.AnalisisIA:
/// tambien los fallos, porque la auditoria de intentos es parte de lo evaluado.
/// </summary>
public sealed class AnalisisIAResultado
{
    public required bool Exitoso { get; init; }
    public required string Modelo { get; init; }
    public required string PromptVersion { get; init; }

    /// <summary>Respuesta validada. Nula si el intento fallo.</summary>
    public AnalisisIARespuesta? Respuesta { get; init; }

    /// <summary>JSON crudo devuelto por el proveedor, ya comprobado como JSON valido.</summary>
    public string? RespuestaJson { get; init; }

    public int? TokensEntrada { get; init; }
    public int? TokensSalida { get; init; }

    /// <summary>Mensaje tecnico controlado del fallo, apto para mostrarse en auditoria.</summary>
    public string? Error { get; init; }

    /// <summary>Identificador del registro de auditoria una vez persistido.</summary>
    public int IdAnalisis { get; set; }

    public static AnalisisIAResultado Fallo(string modelo, string promptVersion, string error) =>
        new() { Exitoso = false, Modelo = modelo, PromptVersion = promptVersion, Error = error };
}
