using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Entities;

/// <summary>
/// Linea del detalle de una reserva: un recurso, su cantidad, precio y descuento de linea.
///
/// <see cref="SubtotalLinea"/> se calcula aqui unicamente para dar respuesta inmediata a la
/// grilla mientras el usuario escribe. En la base es una COLUMNA CALCULADA PERSISTIDA, de modo
/// que el importe real no depende de que la aplicacion lo envie bien: lo deriva el motor.
/// </summary>
public sealed class ReservaDetalle
{
    public int IdDetalle { get; set; }
    public int IdReserva { get; set; }
    public int IdRecurso { get; set; }
    public string Recurso { get; set; } = string.Empty;
    public TipoRecurso TipoRecurso { get; set; }
    public int StockTotal { get; set; }

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PorcentajeDescuento { get; set; }

    /// <summary>Espejo local del calculo que persiste SQL Server: Cantidad * Precio * (1 - Descuento/100).</summary>
    public decimal SubtotalLinea =>
        Math.Round(Cantidad * PrecioUnitario * (1m - (PorcentajeDescuento / 100m)), 2, MidpointRounding.AwayFromZero);
}
