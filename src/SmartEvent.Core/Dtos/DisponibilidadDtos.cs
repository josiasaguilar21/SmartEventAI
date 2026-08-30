namespace SmartEvent.Core.Dtos;

/// <summary>
/// Un motivo concreto por el que la reserva no puede guardarse. El procedimiento devuelve la
/// lista completa (no solo el primero) para que el usuario corrija todo de una vez.
/// </summary>
public sealed class ConflictoDisponibilidad
{
    public required string Tipo { get; init; }
    public string? Referencia { get; init; }
    public required string Detalle { get; init; }
}

/// <summary>
/// Resultado de evt.sp_Disponibilidad_Validar cuando lo invoca la interfaz ANTES de guardar.
/// Es una comprobacion anticipada para dar buena experiencia de uso; la validacion vinculante
/// vuelve a ejecutarse dentro de la transaccion de guardado, de modo que saltarse esta
/// pantalla no permite crear una reserva invalida (CA-05).
/// </summary>
public sealed class DisponibilidadResultado
{
    public required bool EsValido { get; init; }
    public required string Mensaje { get; init; }
    public IReadOnlyList<ConflictoDisponibilidad> Conflictos { get; init; } = Array.Empty<ConflictoDisponibilidad>();

    public bool TieneConflictoDeTipo(string tipo) =>
        Conflictos.Any(c => string.Equals(c.Tipo, tipo, StringComparison.OrdinalIgnoreCase));
}

/// <summary>Parametros de la consulta anticipada de disponibilidad.</summary>
public sealed class DisponibilidadConsultaDto
{
    public int? IdReserva { get; set; }
    public int IdSalon { get; set; }
    public DateOnly FechaEvento { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int NumeroInvitados { get; set; }
    public List<ReservaDetalleGuardarDto> Detalles { get; set; } = new();
}
