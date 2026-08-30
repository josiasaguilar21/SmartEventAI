namespace SmartEvent.Core.Entities;

/// <summary>Cliente que contrata el evento. La baja es logica (<see cref="Estado"/>), nunca fisica.</summary>
public sealed class Cliente
{
    public int IdCliente { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    /// <summary>Texto para combos y busquedas de la interfaz.</summary>
    public string Descripcion => $"{Identificacion} - {Nombres}";
}
