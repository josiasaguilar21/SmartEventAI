using SmartEvent.Application.Sesion;
using SmartEvent.Application.Validacion;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.Application.Servicios;

public interface IServicioCatalogos
{
    Task<IReadOnlyList<Cliente>> ConsultarClientesAsync(string? filtro, bool? estado, CancellationToken cancelacion);
    Task<Cliente?> ObtenerClienteAsync(int idCliente, CancellationToken cancelacion);
    Task<ResultadoOperacion> GuardarClienteAsync(Cliente cliente, CancellationToken cancelacion);
    Task<string> CambiarEstadoClienteAsync(int idCliente, bool estado, CancellationToken cancelacion);

    Task<IReadOnlyList<Salon>> ConsultarSalonesAsync(string? filtro, bool? estado, int? capacidadMinima, CancellationToken cancelacion);
    Task<Salon?> ObtenerSalonAsync(int idSalon, CancellationToken cancelacion);
    Task<ResultadoOperacion> GuardarSalonAsync(Salon salon, CancellationToken cancelacion);
    Task<string> CambiarEstadoSalonAsync(int idSalon, bool estado, CancellationToken cancelacion);

    Task<IReadOnlyList<Recurso>> ConsultarRecursosAsync(string? filtro, TipoRecurso? tipo, bool? estado, CancellationToken cancelacion);
    Task<Recurso?> ObtenerRecursoAsync(int idRecurso, CancellationToken cancelacion);
    Task<ResultadoOperacion> GuardarRecursoAsync(Recurso recurso, CancellationToken cancelacion);
    Task<string> CambiarEstadoRecursoAsync(int idRecurso, bool estado, CancellationToken cancelacion);
}

/// <summary>
/// Mantenimiento de clientes, salones y recursos.
///
/// Responsabilidades propias de esta capa:
///   - Comprobar el PERMISO: consultar puede cualquier rol; modificar, solo ADMINISTRADOR.
///   - Validar el contenido antes de gastar una ida y vuelta al servidor.
///   - Normalizar los textos (recortar espacios) para que "Salon A " y "Salon A" no se
///     conviertan en dos filas distintas que la restriccion UNIQUE no llegaria a detectar.
/// </summary>
public sealed class ServicioCatalogos : IServicioCatalogos
{
    private readonly IClienteRepositorio _clientes;
    private readonly ISalonRepositorio _salones;
    private readonly IRecursoRepositorio _recursos;
    private readonly IContextoSesion _contexto;

    public ServicioCatalogos(IClienteRepositorio clientes, ISalonRepositorio salones,
                             IRecursoRepositorio recursos, IContextoSesion contexto)
    {
        _clientes = clientes;
        _salones = salones;
        _recursos = recursos;
        _contexto = contexto;
    }

    // ------------------------------------------------------------------------------ clientes

    public Task<IReadOnlyList<Cliente>> ConsultarClientesAsync(string? filtro, bool? estado,
                                                               CancellationToken cancelacion) =>
        _clientes.ConsultarAsync(null, filtro, estado, cancelacion);

    public Task<Cliente?> ObtenerClienteAsync(int idCliente, CancellationToken cancelacion) =>
        _clientes.ObtenerPorIdAsync(idCliente, cancelacion);

    public Task<ResultadoOperacion> GuardarClienteAsync(Cliente cliente, CancellationToken cancelacion)
    {
        ExigirPermisoDeEdicion("clientes");

        cliente.Identificacion = cliente.Identificacion?.Trim() ?? string.Empty;
        cliente.Nombres = cliente.Nombres?.Trim() ?? string.Empty;
        cliente.Email = cliente.Email?.Trim() ?? string.Empty;
        cliente.Telefono = string.IsNullOrWhiteSpace(cliente.Telefono) ? null : cliente.Telefono.Trim();

        ExigirValido(ValidadorCatalogos.ValidarCliente(cliente));

        return _clientes.GuardarAsync(cliente, cancelacion);
    }

    public Task<string> CambiarEstadoClienteAsync(int idCliente, bool estado, CancellationToken cancelacion)
    {
        ExigirPermisoDeEdicion("clientes");
        return _clientes.CambiarEstadoAsync(idCliente, estado, cancelacion);
    }

    // ------------------------------------------------------------------------------- salones

    public Task<IReadOnlyList<Salon>> ConsultarSalonesAsync(string? filtro, bool? estado, int? capacidadMinima,
                                                            CancellationToken cancelacion) =>
        _salones.ConsultarAsync(null, filtro, estado, capacidadMinima, cancelacion);

    public Task<Salon?> ObtenerSalonAsync(int idSalon, CancellationToken cancelacion) =>
        _salones.ObtenerPorIdAsync(idSalon, cancelacion);

    public Task<ResultadoOperacion> GuardarSalonAsync(Salon salon, CancellationToken cancelacion)
    {
        ExigirPermisoDeEdicion("salones");

        salon.Nombre = salon.Nombre?.Trim() ?? string.Empty;
        salon.Ubicacion = string.IsNullOrWhiteSpace(salon.Ubicacion) ? null : salon.Ubicacion.Trim();

        ExigirValido(ValidadorCatalogos.ValidarSalon(salon));

        return _salones.GuardarAsync(salon, cancelacion);
    }

    public Task<string> CambiarEstadoSalonAsync(int idSalon, bool estado, CancellationToken cancelacion)
    {
        ExigirPermisoDeEdicion("salones");
        return _salones.CambiarEstadoAsync(idSalon, estado, cancelacion);
    }

    // ------------------------------------------------------------------------------ recursos

    public Task<IReadOnlyList<Recurso>> ConsultarRecursosAsync(string? filtro, TipoRecurso? tipo, bool? estado,
                                                               CancellationToken cancelacion) =>
        _recursos.ConsultarAsync(null, filtro, tipo, estado, cancelacion);

    public Task<Recurso?> ObtenerRecursoAsync(int idRecurso, CancellationToken cancelacion) =>
        _recursos.ObtenerPorIdAsync(idRecurso, cancelacion);

    public Task<ResultadoOperacion> GuardarRecursoAsync(Recurso recurso, CancellationToken cancelacion)
    {
        ExigirPermisoDeEdicion("recursos");

        recurso.Nombre = recurso.Nombre?.Trim() ?? string.Empty;

        ExigirValido(ValidadorCatalogos.ValidarRecurso(recurso));

        return _recursos.GuardarAsync(recurso, cancelacion);
    }

    public Task<string> CambiarEstadoRecursoAsync(int idRecurso, bool estado, CancellationToken cancelacion)
    {
        ExigirPermisoDeEdicion("recursos");
        return _recursos.CambiarEstadoAsync(idRecurso, estado, cancelacion);
    }

    // --------------------------------------------------------------------------------- apoyo

    /// <summary>
    /// El menu ya oculta estas opciones al coordinador, pero ocultar un boton no es una medida
    /// de seguridad: la comprobacion se repite aqui, en el servicio, que es por donde pasan
    /// obligatoriamente todas las modificaciones.
    /// </summary>
    private void ExigirPermisoDeEdicion(string catalogo)
    {
        var sesion = _contexto.Requerida;

        if (!sesion.PuedeEditarCatalogos)
        {
            throw new ReglaNegocioException(
                $"Su rol ({sesion.Rol}) permite consultar el catalogo de {catalogo}, pero no modificarlo. " +
                "Solicite el cambio a un administrador.");
        }
    }

    private static void ExigirValido(ResultadoValidacion validacion)
    {
        if (!validacion.EsValido)
        {
            throw new ReglaNegocioException(validacion.MensajeCompleto());
        }
    }
}
