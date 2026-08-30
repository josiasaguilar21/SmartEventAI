namespace SmartEvent.Core.Dtos;

/// <summary>
/// Datos que la interfaz envia para crear o actualizar una reserva.
///
/// Observese lo que NO esta aqui: Subtotal, Impuesto y Total. La aplicacion no puede enviar
/// importes; los calcula y persiste evt.sp_Reserva_Guardar. Tampoco esta el Estado: cambiarlo
/// es responsabilidad exclusiva de evt.sp_Reserva_CambiarEstado.
/// </summary>
public sealed class ReservaGuardarDto
{
    /// <summary>Nulo para alta; con valor para edicion (y entonces se excluye a si misma del cruce).</summary>
    public int? IdReserva { get; set; }

    public int IdCliente { get; set; }
    public int IdSalon { get; set; }
    public DateOnly FechaEvento { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int NumeroInvitados { get; set; }

    /// <summary>Descuento global en importe, aplicado sobre el subtotal antes del impuesto.</summary>
    public decimal Descuento { get; set; }

    public string? Observacion { get; set; }

    public List<ReservaDetalleGuardarDto> Detalles { get; set; } = new();
}

/// <summary>
/// Linea del detalle que viaja dentro del parametro tipo tabla evt.ReservaDetalleType.
/// Todo el detalle se envia en UNA sola llamada, no fila por fila.
/// </summary>
public sealed class ReservaDetalleGuardarDto
{
    public int IdRecurso { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeDescuento { get; set; }
}

/// <summary>Valores devueltos por evt.sp_Reserva_Guardar tras confirmar la transaccion.</summary>
public sealed class ReservaGuardarResultado
{
    public required int IdReserva { get; init; }
    public required string Codigo { get; init; }
    public required string Mensaje { get; init; }
}
