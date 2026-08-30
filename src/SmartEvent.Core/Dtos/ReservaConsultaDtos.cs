using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Dtos;

/// <summary>
/// Filtros combinables de la consulta historica. Todas las propiedades son opcionales: el
/// procedimiento las anula con el patron (@Param IS NULL OR columna = @Param), de forma que
/// no hace falta construir SQL dinamico ni concatenar nada.
/// </summary>
public sealed class ReservaFiltroDto
{
    public string? Codigo { get; set; }
    public int? IdCliente { get; set; }
    public string? TextoCliente { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public int? IdSalon { get; set; }
    public EstadoReserva? Estado { get; set; }

    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 50;
}

/// <summary>Fila de la grilla de consulta: lo justo para listar sin traer el detalle completo.</summary>
public sealed class ReservaResumenDto
{
    public int IdReserva { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public int IdSalon { get; set; }
    public string Salon { get; set; } = string.Empty;
    public DateOnly FechaEvento { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int NumeroInvitados { get; set; }
    public EstadoReserva Estado { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    public int TotalDetalles { get; set; }
    public DateTime? UltimoAnalisis { get; set; }
    public DateTime? UltimoCorreo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = string.Empty;

    // Columnas calculadas para enlazar directamente al DataGridView sin logica en el formulario.
    public string EstadoTexto => Estado.ATextoUsuario();
    public string HorarioTexto => $"{HoraInicio:HH\\:mm} - {HoraFin:HH\\:mm}";
    public string FechaEventoTexto => FechaEvento.ToString("dd/MM/yyyy");
    public bool TieneAnalisisIA => UltimoAnalisis.HasValue;
    public bool TieneCorreoEnviado => UltimoCorreo.HasValue;
}

/// <summary>
/// Pagina de resultados: los elementos visibles mas el total de coincidencias, para que la
/// interfaz pueda mostrar "1-50 de 320" sin volver a consultar.
/// </summary>
public sealed class PaginaResultado<T>
{
    public required IReadOnlyList<T> Elementos { get; init; }
    public required int TotalRegistros { get; init; }
    public required int Pagina { get; init; }
    public required int TamanoPagina { get; init; }

    public int TotalPaginas => TamanoPagina <= 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
    public bool HayPaginaSiguiente => Pagina < TotalPaginas;
    public bool HayPaginaAnterior => Pagina > 1;

    public static PaginaResultado<T> Vacia(int tamanoPagina = 50) => new()
    {
        Elementos = Array.Empty<T>(),
        TotalRegistros = 0,
        Pagina = 1,
        TamanoPagina = tamanoPagina
    };
}
