using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Abstractions;

/// <summary>
/// Mantenimiento de clientes. La baja siempre es logica: <c>CambiarEstadoAsync</c> nunca borra
/// filas, para no romper la trazabilidad de las reservas historicas.
/// </summary>
public interface IClienteRepositorio
{
    Task<IReadOnlyList<Cliente>> ConsultarAsync(int? idCliente, string? filtro, bool? estado, CancellationToken cancelacion);
    Task<Cliente?> ObtenerPorIdAsync(int idCliente, CancellationToken cancelacion);
    Task<ResultadoOperacion> GuardarAsync(Cliente cliente, CancellationToken cancelacion);
    Task<string> CambiarEstadoAsync(int idCliente, bool estado, CancellationToken cancelacion);
}

/// <summary>Mantenimiento de salones.</summary>
public interface ISalonRepositorio
{
    Task<IReadOnlyList<Salon>> ConsultarAsync(int? idSalon, string? filtro, bool? estado, int? capacidadMinima, CancellationToken cancelacion);
    Task<Salon?> ObtenerPorIdAsync(int idSalon, CancellationToken cancelacion);
    Task<ResultadoOperacion> GuardarAsync(Salon salon, CancellationToken cancelacion);
    Task<string> CambiarEstadoAsync(int idSalon, bool estado, CancellationToken cancelacion);
}

/// <summary>Mantenimiento de recursos y servicios.</summary>
public interface IRecursoRepositorio
{
    Task<IReadOnlyList<Recurso>> ConsultarAsync(int? idRecurso, string? filtro, TipoRecurso? tipo, bool? estado, CancellationToken cancelacion);
    Task<Recurso?> ObtenerPorIdAsync(int idRecurso, CancellationToken cancelacion);
    Task<ResultadoOperacion> GuardarAsync(Recurso recurso, CancellationToken cancelacion);
    Task<string> CambiarEstadoAsync(int idRecurso, bool estado, CancellationToken cancelacion);
}
