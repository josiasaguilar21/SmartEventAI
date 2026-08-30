using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Entities;

/// <summary>
/// Movimiento del historial de estados de una reserva: quien lo hizo, cuando, desde donde
/// hacia donde y con que motivo. Es la evidencia de CA-06 y CA-07.
/// </summary>
public sealed class ReservaAuditoria
{
    public long IdAuditoria { get; set; }
    public int IdReserva { get; set; }
    public EstadoReserva? EstadoAnterior { get; set; }
    public EstadoReserva EstadoNuevo { get; set; }
    public string? Motivo { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }

    public string TransicionTexto =>
        EstadoAnterior is null
            ? EstadoNuevo.ATextoUsuario()
            : $"{EstadoAnterior.Value.ATextoUsuario()} -> {EstadoNuevo.ATextoUsuario()}";
}
