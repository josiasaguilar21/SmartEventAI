using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Entities;

/// <summary>
/// Registro de auditoria de una llamada al modelo de IA, exitosa o fallida.
/// Se guarda el JSON estructurado devuelto, el modelo, la version del prompt y el consumo de
/// tokens cuando el proveedor lo informa. NUNCA se guarda la clave de API.
/// </summary>
public sealed class AnalisisIA
{
    public int IdAnalisis { get; set; }
    public int IdReserva { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public string ClienteReserva { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;
    public string PromptVersion { get; set; } = string.Empty;

    /// <summary>Respuesta cruda validada contra el esquema JSON antes de persistirse.</summary>
    public string? RespuestaJson { get; set; }

    public NivelRiesgo? NivelRiesgo { get; set; }
    public int? TokensEntrada { get; set; }
    public int? TokensSalida { get; set; }
    public DateTime Fecha { get; set; }
    public bool Exitoso { get; set; }

    /// <summary>Mensaje tecnico controlado del fallo. Se muestra en la pantalla de auditoria.</summary>
    public string? Error { get; set; }

    public string? Usuario { get; set; }
}
