using System.Data;
using SmartEvent.Core.Dtos;

namespace SmartEvent.Infrastructure.Data.Comun;

/// <summary>
/// Construye el parametro tipo tabla que transporta el detalle completo de una reserva.
///
/// ESTE ES EL MECANISMO CENTRAL DEL REQUISITO CABECERA-DETALLE: en lugar de recorrer la grilla
/// enviando un INSERT por fila (lo que dejaria la operacion a medias si una linea falla), se
/// arma un DataTable con todas las lineas y se envia como UN solo parametro. El procedimiento
/// almacenado abre una transaccion, sincroniza todo el detalle de golpe y confirma o revierte
/// el conjunto entero.
///
/// El orden y el tipo de las columnas deben coincidir EXACTAMENTE con evt.ReservaDetalleType:
///     IdRecurso INT, Cantidad INT, PrecioUnitario DECIMAL(12,2), PorcentajeDescuento DECIMAL(5,2)
/// </summary>
internal static class TablaDetalleReserva
{
    /// <summary>Nombre del tipo tabla en la base, con su esquema.</summary>
    public const string NombreTipo = "evt.ReservaDetalleType";

    public static DataTable Construir(IEnumerable<ReservaDetalleGuardarDto> detalles)
    {
        var tabla = new DataTable("ReservaDetalleType");

        tabla.Columns.Add("IdRecurso", typeof(int));
        tabla.Columns.Add("Cantidad", typeof(int));

        var precio = tabla.Columns.Add("PrecioUnitario", typeof(decimal));
        precio.ExtendedProperties["Precision"] = 12;
        precio.ExtendedProperties["Scale"] = 2;

        var descuento = tabla.Columns.Add("PorcentajeDescuento", typeof(decimal));
        descuento.ExtendedProperties["Precision"] = 5;
        descuento.ExtendedProperties["Scale"] = 2;

        foreach (var detalle in detalles)
        {
            tabla.Rows.Add(
                detalle.IdRecurso,
                detalle.Cantidad,
                decimal.Round(detalle.PrecioUnitario, 2, MidpointRounding.AwayFromZero),
                decimal.Round(detalle.PorcentajeDescuento, 2, MidpointRounding.AwayFromZero));
        }

        return tabla;
    }
}
