using Microsoft.Data.SqlClient;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.Infrastructure.Data.Comun;

/// <summary>
/// Convierte una <see cref="SqlException"/> en una excepcion del dominio.
///
/// ESTA ES LA FRONTERA DE SEGURIDAD DE LOS MENSAJES DE ERROR:
///   - Numero &gt;= 50000  -> lo lanzo yo desde un procedimiento almacenado con un texto
///                            redactado para el usuario final. Se propaga tal cual.
///   - Errores de red / tiempo de espera -> mensaje de conectividad, con opcion de reintentar.
///   - Cualquier otro -> mensaje generico. El detalle tecnico (numero, procedimiento y linea)
///                       va al log local, NUNCA a la pantalla: asi no se filtra el nombre de
///                       una tabla, una restriccion, la cadena de conexion ni un stack trace.
/// </summary>
internal static class TraductorErroresSql
{
    /// <summary>Numero minimo reservado para los errores de negocio definidos con THROW.</summary>
    private const int PrimerErrorDeNegocio = 50000;

    /// <summary>Errores del motor asociados a red, tiempo de espera o base inaccesible.</summary>
    private static readonly HashSet<int> ErroresDeConectividad = new()
    {
        -2,     // tiempo de espera agotado
        2,      // no se encontro el servidor
        53,     // ruta de red no encontrada
        233,    // conexion cerrada por el servidor
        258,    // tiempo de espera en el handshake
        4060,   // no se puede abrir la base solicitada
        10053,  // conexion abortada por el software del host
        10054,  // conexion restablecida por el interlocutor
        10060,  // no se pudo establecer la conexion
        11001,  // host desconocido
        40613   // base de datos no disponible
    };

    public static Exception Traducir(SqlException excepcion, string operacion, IRegistroEventos registro)
    {
        // Una SqlException puede agrupar varios errores; el primero con numero de negocio manda.
        foreach (SqlError error in excepcion.Errors)
        {
            if (error.Number >= PrimerErrorDeNegocio)
            {
                // No se registra como error del sistema: es una regla de negocio funcionando.
                registro.Informacion($"Regla de negocio aplicada en {operacion} (codigo {error.Number}).");
                return new ReglaNegocioException(error.Message, error.Number, excepcion);
            }
        }

        if (ErroresDeConectividad.Contains(excepcion.Number))
        {
            registro.Advertencia(
                $"Problema de conectividad con SQL Server durante {operacion} (numero {excepcion.Number}).", excepcion);

            return new ErrorConectividadException(
                "No hay comunicacion con el servidor de base de datos. " +
                "Verifique su conexion de red y vuelva a intentarlo.",
                $"{operacion}: SQL {excepcion.Number} - {excepcion.Message}",
                excepcion);
        }

        registro.Error(
            $"Error de SQL Server durante {operacion}. Numero {excepcion.Number}, " +
            $"estado {excepcion.State}, procedimiento '{excepcion.Procedure}', linea {excepcion.LineNumber}.",
            excepcion);

        return new ErrorTecnicoException(
            $"{operacion}: SQL {excepcion.Number} en {excepcion.Procedure} linea {excepcion.LineNumber}.",
            excepcion);
    }
}
