using Microsoft.Data.SqlClient;

namespace SmartEvent.Infrastructure.Data.Conexion;

/// <summary>
/// Origen de conexiones a SQL Server.
///
/// DECISION DE DISENO: no existe ninguna conexion global ni compartida. Cada operacion pide
/// una conexion nueva, la usa dentro de un <c>await using</c> y la devuelve al pool de
/// Microsoft.Data.SqlClient al terminar. El pool es quien reutiliza los sockets, no nosotros:
/// mantener un SqlConnection abierto en un campo estatico es justamente lo que prohibe el
/// enunciado y lo que provoca fugas y bloqueos en aplicaciones de escritorio.
/// </summary>
public interface IFabricaConexiones
{
    /// <summary>Crea y abre una conexion. Quien la recibe es responsable de liberarla.</summary>
    Task<SqlConnection> CrearAbiertaAsync(CancellationToken cancelacion);

    /// <summary>
    /// Descripcion del servidor y base para la barra de estado. Se construye desde el
    /// SqlConnectionStringBuilder tomando SOLO servidor y catalogo: nunca usuario ni contrasena.
    /// </summary>
    string DescripcionConexion { get; }

    /// <summary>Comprueba la conectividad sin lanzar excepciones; alimenta el indicador de estado.</summary>
    Task<bool> ProbarConexionAsync(CancellationToken cancelacion);
}
