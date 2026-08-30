using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Entities;

/// <summary>
/// Recurso o servicio contratable. <see cref="StockTotal"/> es el inventario maximo; la
/// disponibilidad real se calcula por fecha y franja horaria en evt.sp_Disponibilidad_Validar,
/// porque un mismo recurso puede estar comprometido en otra reserva del mismo dia.
/// </summary>
public sealed class Recurso
{
    public int IdRecurso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoRecurso Tipo { get; set; }
    public int StockTotal { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public string Descripcion => $"{Nombre} ({Tipo})";
}
