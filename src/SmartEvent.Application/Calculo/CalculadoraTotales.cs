using SmartEvent.Core.Dtos;

namespace SmartEvent.Application.Calculo;

/// <summary>Importes de una reserva ya desglosados.</summary>
public sealed class ResumenTotales
{
    public required decimal TarifaBase { get; init; }
    public required decimal SumaLineas { get; init; }
    public required decimal Subtotal { get; init; }
    public required decimal Descuento { get; init; }
    public required decimal BaseNeta { get; init; }
    public required decimal Impuesto { get; init; }
    public required decimal Total { get; init; }

    public static ResumenTotales Vacio => new()
    {
        TarifaBase = 0m, SumaLineas = 0m, Subtotal = 0m,
        Descuento = 0m, BaseNeta = 0m, Impuesto = 0m, Total = 0m
    };
}

/// <summary>
/// Calculo de importes en el cliente.
///
/// ESTE CALCULO NO ES LA FUENTE DE VERDAD. Existe unicamente para que la grilla muestre los
/// totales mientras el usuario escribe, sin ir al servidor en cada pulsacion. El importe que
/// se persiste lo recalcula evt.sp_Reserva_Guardar y es el que vuelve en la respuesta.
///
/// Por eso la formula esta duplicada a proposito, y por eso se replica exactamente:
///     SubtotalLinea = Cantidad * PrecioUnitario * (1 - PorcentajeDescuento / 100)
///     Subtotal      = TarifaBase del salon + suma de los subtotales de linea
///     BaseNeta      = Subtotal - Descuento global
///     Impuesto      = 15 por ciento de la BaseNeta
///     Total         = BaseNeta + Impuesto
///
/// El redondeo usa MidpointRounding.AwayFromZero para coincidir con el redondeo aritmetico de
/// SQL Server; el modo por defecto de .NET (ToEven) daria diferencias de un centavo.
/// </summary>
public static class CalculadoraTotales
{
    /// <summary>
    /// Tasa de impuesto del 15 por ciento. Esta constante es el espejo de @TasaImpuesto en
    /// evt.sp_Reserva_Guardar: si cambia una, debe cambiar la otra.
    /// </summary>
    public const decimal TasaImpuesto = 0.15m;

    public static decimal CalcularSubtotalLinea(int cantidad, decimal precioUnitario, decimal porcentajeDescuento) =>
        decimal.Round(cantidad * precioUnitario * (1m - (porcentajeDescuento / 100m)), 2, MidpointRounding.AwayFromZero);

    public static ResumenTotales Calcular(decimal tarifaBase, IEnumerable<ReservaDetalleGuardarDto> detalles,
                                          decimal descuentoGlobal)
    {
        ArgumentNullException.ThrowIfNull(detalles);

        var sumaLineas = detalles.Sum(d =>
            CalcularSubtotalLinea(d.Cantidad, d.PrecioUnitario, d.PorcentajeDescuento));

        var subtotal = decimal.Round(tarifaBase + sumaLineas, 2, MidpointRounding.AwayFromZero);

        // El descuento global nunca puede dejar la base en negativo; se acota para que la
        // previsualizacion sea coherente aunque el usuario escriba un valor excesivo. El
        // rechazo formal de ese valor lo hace la validacion y, en ultima instancia, SQL Server.
        var descuento = Math.Clamp(decimal.Round(descuentoGlobal, 2, MidpointRounding.AwayFromZero), 0m, subtotal);

        var baseNeta = decimal.Round(subtotal - descuento, 2, MidpointRounding.AwayFromZero);
        var impuesto = decimal.Round(baseNeta * TasaImpuesto, 2, MidpointRounding.AwayFromZero);
        var total = decimal.Round(baseNeta + impuesto, 2, MidpointRounding.AwayFromZero);

        return new ResumenTotales
        {
            TarifaBase = tarifaBase,
            SumaLineas = sumaLineas,
            Subtotal = subtotal,
            Descuento = descuento,
            BaseNeta = baseNeta,
            Impuesto = impuesto,
            Total = total
        };
    }
}
