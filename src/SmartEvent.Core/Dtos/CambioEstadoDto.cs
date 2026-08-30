using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Dtos;

/// <summary>
/// Solicitud de cambio de estado. La transicion la valida evt.sp_Reserva_CambiarEstado contra
/// la tabla evt.TransicionEstado; aqui solo se transporta la intencion del usuario.
/// </summary>
public sealed class CambioEstadoDto
{
    public required int IdReserva { get; init; }
    public required EstadoReserva EstadoNuevo { get; init; }

    /// <summary>Obligatorio y de al menos 20 caracteres al cancelar.</summary>
    public string? Motivo { get; init; }

    /// <summary>
    /// Justificacion escrita por el usuario cuando confirma sin haber podido ejecutar el
    /// analisis de IA. Se guarda en la reserva y queda auditada: es la contingencia manual
    /// prevista por la regla de negocio.
    /// </summary>
    public string? JustificacionContingencia { get; init; }
}

/// <summary>
/// Resultado de confirmar o cancelar. Incluye tanto el cambio de estado (que ya ocurrio y es
/// definitivo) como el resultado del correo, que puede haber fallado sin invalidar aquel.
/// </summary>
public sealed class CambioEstadoResultado
{
    public required bool EstadoCambiado { get; init; }
    public required string Mensaje { get; init; }
    public required EstadoReserva EstadoAnterior { get; init; }
    public required EstadoReserva EstadoNuevo { get; init; }

    /// <summary>Resultado del correo asociado. Nulo si la transicion no genera notificacion.</summary>
    public ResultadoCorreo? Correo { get; init; }

    /// <summary>
    /// Verdadero cuando la reserva cambio de estado correctamente pero el correo no pudo
    /// enviarse. La interfaz lo usa para avisar y ofrecer el reenvio explicito.
    /// </summary>
    public bool RequiereReintentoCorreo => EstadoCambiado && Correo is { Enviado: false };
}
