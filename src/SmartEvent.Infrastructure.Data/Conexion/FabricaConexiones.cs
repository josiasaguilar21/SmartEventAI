using Microsoft.Data.SqlClient;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.Infrastructure.Data.Conexion;

/// <summary>
/// Implementacion de <see cref="IFabricaConexiones"/> sobre Microsoft.Data.SqlClient.
/// La cadena de conexion se recibe una sola vez en el constructor, se guarda en un campo
/// privado y no se expone por ninguna propiedad publica: asi no puede acabar en un mensaje
/// de error, en un log ni en una captura de pantalla.
/// </summary>
public sealed class FabricaConexiones : IFabricaConexiones
{
    private readonly string _cadenaConexion;
    private readonly IRegistroEventos _registro;

    public FabricaConexiones(string cadenaConexion, IRegistroEventos registro)
    {
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new ConfiguracionException(
                "No se ha configurado la cadena de conexion a SQL Server. " +
                "Revise la seccion ConnectionStrings del archivo appsettings.json " +
                "o la variable de entorno SMARTEVENT_CONNECTION.");
        }

        _cadenaConexion = cadenaConexion;
        _registro = registro;

        // Se valida el formato al arrancar para fallar temprano y con un mensaje comprensible,
        // en lugar de hacerlo en medio de una operacion del usuario.
        try
        {
            var constructor = new SqlConnectionStringBuilder(cadenaConexion);
            DescripcionConexion = $"{constructor.DataSource} / {constructor.InitialCatalog}";
        }
        catch (Exception ex)
        {
            throw new ConfiguracionException(
                "La cadena de conexion configurada no tiene un formato valido. " +
                "Compare su appsettings.json con appsettings.example.json.", ex);
        }
    }

    public string DescripcionConexion { get; }

    public async Task<SqlConnection> CrearAbiertaAsync(CancellationToken cancelacion)
    {
        var conexion = new SqlConnection(_cadenaConexion);

        try
        {
            await conexion.OpenAsync(cancelacion).ConfigureAwait(false);
            return conexion;
        }
        catch (Exception)
        {
            // Si la apertura falla hay que liberar el objeto igualmente: nunca se devuelve
            // una conexion a medio abrir ni se deja sin disponer.
            await conexion.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<bool> ProbarConexionAsync(CancellationToken cancelacion)
    {
        try
        {
            await using var conexion = await CrearAbiertaAsync(cancelacion).ConfigureAwait(false);
            await using var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT 1;";
            comando.CommandTimeout = 5;

            await comando.ExecuteScalarAsync(cancelacion).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // El indicador de conectividad no debe interrumpir el trabajo: se registra y se
            // informa como "sin conexion".
            _registro.Advertencia("No fue posible verificar la conexion con SQL Server.", ex);
            return false;
        }
    }
}
