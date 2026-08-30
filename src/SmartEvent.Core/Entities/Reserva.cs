using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Entities;

/// <summary>
/// Cabecera de la reserva con su detalle asociado.
///
/// Los importes (<see cref="Subtotal"/>, <see cref="Impuesto"/>, <see cref="Total"/>) son de
/// solo lectura conceptual: se muestran para que el usuario los vea calculados en tiempo real,
/// pero el valor definitivo es SIEMPRE el que devuelve evt.sp_Reserva_Guardar. La aplicacion
/// nunca envia totales al guardar; los recibe ya recalculados por el motor.
/// </summary>
public sealed class Reserva
{
    public int IdReserva { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public int IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string IdentificacionCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string? TelefonoCliente { get; set; }

    public int IdSalon { get; set; }
    public string Salon { get; set; } = string.Empty;
    public int CapacidadSalon { get; set; }
    public decimal TarifaBaseSalon { get; set; }

    public DateOnly FechaEvento { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int NumeroInvitados { get; set; }

    public EstadoReserva Estado { get; set; } = EstadoReserva.Borrador;

    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }

    public string? Observacion { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string? JustificacionContingencia { get; set; }

    public int IdUsuarioCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int? IdUsuarioModificacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public List<ReservaDetalle> Detalles { get; set; } = new();

    /// <summary>Duracion del evento; la regla de negocio la limita a un rango de 2 a 12 horas.</summary>
    public TimeSpan Duracion => HoraFin.ToTimeSpan() - HoraInicio.ToTimeSpan();

    public string HorarioTexto => $"{HoraInicio:HH\\:mm} - {HoraFin:HH\\:mm}";

    /// <summary>Base sobre la que se calcula el impuesto del 15 por ciento.</summary>
    public decimal BaseNeta => Subtotal - Descuento;
}
