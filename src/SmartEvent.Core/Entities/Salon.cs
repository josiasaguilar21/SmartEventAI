namespace SmartEvent.Core.Entities;

/// <summary>
/// Salon reservable. <see cref="TarifaBase"/> es el punto de partida del subtotal de toda
/// reserva y <see cref="Capacidad"/> el limite duro de invitados que valida SQL Server.
/// </summary>
public sealed class Salon
{
    public int IdSalon { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public int Capacidad { get; set; }
    public decimal TarifaBase { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public string Descripcion => $"{Nombre} (capacidad {Capacidad})";
}
