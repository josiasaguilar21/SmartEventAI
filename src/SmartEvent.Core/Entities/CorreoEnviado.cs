using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Entities;

/// <summary>
/// Auditoria de un INTENTO de envio de correo. Se registra tanto el exito como el fallo, con
/// su mensaje tecnico controlado, para que FrmAuditoriaIntegraciones permita diagnosticar y
/// reenviar de forma explicita sin duplicar la reserva ni su cambio de estado (CA-07).
/// No almacena credenciales SMTP de ningun tipo.
/// </summary>
public sealed class CorreoEnviado
{
    public int IdCorreo { get; set; }
    public int IdReserva { get; set; }
    public string CodigoReserva { get; set; } = string.Empty;
    public string ClienteReserva { get; set; } = string.Empty;
    public TipoNotificacion TipoNotificacion { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public DateTime FechaIntento { get; set; }
    public EstadoCorreo Estado { get; set; }
    public string? Error { get; set; }
    public string? Usuario { get; set; }
}
