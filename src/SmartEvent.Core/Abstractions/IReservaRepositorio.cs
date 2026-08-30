using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;

namespace SmartEvent.Core.Abstractions;

/// <summary>
/// Acceso a datos de reservas. Cada metodo se corresponde con un procedimiento almacenado:
/// la capa no arma sentencias, solo invoca procedimientos con parametros tipados.
/// </summary>
public interface IReservaRepositorio
{
    /// <summary>
    /// Ejecuta evt.sp_Reserva_Guardar. Envia toda la cabecera y TODO el detalle en una sola
    /// llamada mediante el parametro tipo tabla evt.ReservaDetalleType, de forma que el motor
    /// resuelve la operacion completa dentro de una unica transaccion.
    /// </summary>
    Task<ReservaGuardarResultado> GuardarAsync(ReservaGuardarDto reserva, int idUsuario, CancellationToken cancelacion);

    /// <summary>
    /// Comprobacion anticipada de disponibilidad para la interfaz. La validacion vinculante se
    /// repite dentro de la transaccion de guardado.
    /// </summary>
    Task<DisponibilidadResultado> ValidarDisponibilidadAsync(DisponibilidadConsultaDto consulta, CancellationToken cancelacion);

    /// <summary>Consulta paginada con filtros opcionales combinables.</summary>
    Task<PaginaResultado<ReservaResumenDto>> ConsultarAsync(ReservaFiltroDto filtro, CancellationToken cancelacion);

    /// <summary>Devuelve cabecera y detalle completo, leyendo los dos conjuntos de resultados del procedimiento.</summary>
    Task<Reserva?> ObtenerPorIdAsync(int idReserva, CancellationToken cancelacion);

    /// <summary>Ejecuta evt.sp_Reserva_CambiarEstado y devuelve el mensaje de confirmacion.</summary>
    Task<string> CambiarEstadoAsync(CambioEstadoDto cambio, int idUsuario, CancellationToken cancelacion);

    /// <summary>Historial de transiciones de estado de una reserva.</summary>
    Task<IReadOnlyList<ReservaAuditoria>> ObtenerAuditoriaAsync(int idReserva, CancellationToken cancelacion);
}
