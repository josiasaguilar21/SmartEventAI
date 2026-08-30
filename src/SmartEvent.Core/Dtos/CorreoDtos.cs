using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Dtos;

/// <summary>Mensaje ya compuesto y listo para entregarse al servidor SMTP.</summary>
public sealed class MensajeCorreo
{
    public required int IdReserva { get; init; }
    public required TipoNotificacion Tipo { get; init; }
    public required string Destinatario { get; init; }
    public required string NombreDestinatario { get; init; }
    public required string Asunto { get; init; }
    public required string CuerpoHtml { get; init; }

    /// <summary>Version en texto plano para clientes que no muestran HTML.</summary>
    public required string CuerpoTexto { get; init; }
}

/// <summary>
/// Resultado de un intento de envio. Nunca lanza hacia arriba por un fallo SMTP: el envio del
/// correo NO puede revertir ni duplicar el cambio de estado de la reserva, asi que el fallo se
/// devuelve como dato, se audita y la interfaz ofrece reenviar (CA-07).
/// </summary>
public sealed class ResultadoCorreo
{
    public required bool Enviado { get; init; }
    public required string Destinatario { get; init; }
    public required string Asunto { get; init; }
    public required DateTime FechaIntento { get; init; }

    /// <summary>Mensaje tecnico controlado; sin credenciales, servidor ni contrasenas.</summary>
    public string? Error { get; init; }

    /// <summary>Identificador del registro en com.CorreoEnviado.</summary>
    public int IdCorreo { get; set; }

    public EstadoCorreo Estado => Enviado ? EstadoCorreo.Enviado : EstadoCorreo.Error;
}

/// <summary>Filtros de la pantalla de auditoria de correos.</summary>
public sealed class FiltroCorreoDto
{
    public int? IdReserva { get; set; }
    public string? Codigo { get; set; }
    public EstadoCorreo? Estado { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
}

/// <summary>Filtros de la pantalla de auditoria de analisis de IA.</summary>
public sealed class FiltroAnalisisDto
{
    public int? IdReserva { get; set; }
    public string? Codigo { get; set; }
    public bool? Exitoso { get; set; }
    public NivelRiesgo? NivelRiesgo { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
}
