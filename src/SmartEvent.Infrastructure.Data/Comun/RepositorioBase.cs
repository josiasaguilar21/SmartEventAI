using System.Data;
using Microsoft.Data.SqlClient;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Exceptions;
using SmartEvent.Infrastructure.Data.Conexion;

namespace SmartEvent.Infrastructure.Data.Comun;

/// <summary>
/// Base comun de los repositorios. Concentra tres cosas que de otro modo se repetirian en
/// cada metodo y que son justamente donde se cometen los errores:
///   1. Abrir la conexion y liberarla siempre (<c>await using</c>, tambien si hay excepcion).
///   2. Ejecutar SIEMPRE por CommandType.StoredProcedure: no hay texto SQL en esta capa.
///   3. Traducir la SqlException a una excepcion del dominio con mensaje seguro.
/// </summary>
public abstract class RepositorioBase
{
    protected const int TiempoEsperaSegundos = 30;

    private readonly IFabricaConexiones _fabrica;

    protected RepositorioBase(IFabricaConexiones fabrica, IRegistroEventos registro)
    {
        _fabrica = fabrica;
        Registro = registro;
    }

    protected IRegistroEventos Registro { get; }

    /// <summary>
    /// Ejecuta una operacion contra un procedimiento almacenado gestionando conexion, comando,
    /// cancelacion y traduccion de errores. El delegado recibe el comando ya preparado.
    /// </summary>
    protected async Task<T> EjecutarAsync<T>(
        string procedimiento,
        Func<SqlCommand, CancellationToken, Task<T>> operacion,
        CancellationToken cancelacion)
    {
        try
        {
            await using var conexion = await _fabrica.CrearAbiertaAsync(cancelacion).ConfigureAwait(false);
            await using var comando = conexion.CreateCommand();

            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandText = procedimiento;
            comando.CommandTimeout = TiempoEsperaSegundos;

            return await operacion(comando, cancelacion).ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            throw TraductorErroresSql.Traducir(ex, procedimiento, Registro);
        }
        catch (OperationCanceledException)
        {
            // La cancelacion la pide el usuario (cerro la pantalla o relanzo la busqueda).
            // No es un error: se deja subir tal cual para que la interfaz simplemente la ignore.
            throw;
        }
        catch (InvalidOperationException ex)
        {
            Registro.Error($"Estado invalido al ejecutar {procedimiento}.", ex);
            throw new ErrorTecnicoException($"{procedimiento}: {ex.Message}", ex);
        }
    }
}
