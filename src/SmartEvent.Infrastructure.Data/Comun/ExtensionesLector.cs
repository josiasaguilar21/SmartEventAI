using Microsoft.Data.SqlClient;

namespace SmartEvent.Infrastructure.Data.Comun;

/// <summary>
/// Lectura por NOMBRE de columna, con tratamiento explicito de nulos.
///
/// NOTA SOBRE ASINCRONIA: la operacion de red es <c>ReadAsync</c>, que trae la fila completa al
/// bufer del cliente. Una vez dentro de la fila, los descriptores como GetString o GetInt32 no
/// realizan entrada/salida, por lo que usarlos de forma sincrona no bloquea el hilo de la
/// interfaz. Lo que nunca se hace es abrir, ejecutar o avanzar de fila de forma sincrona.
///
/// Leer por nombre y no por indice numerico evita que anadir una columna al SELECT de un
/// procedimiento rompa silenciosamente el mapeo.
/// </summary>
internal static class ExtensionesLector
{
    public static int Entero(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? 0 : lector.GetInt32(indice);
    }

    public static int? EnteroNulo(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetInt32(indice);
    }

    public static long EnteroLargo(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? 0L : lector.GetInt64(indice);
    }

    public static string Texto(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? string.Empty : lector.GetString(indice);
    }

    public static string? TextoNulo(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetString(indice);
    }

    public static bool Booleano(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return !lector.IsDBNull(indice) && lector.GetBoolean(indice);
    }

    public static decimal Decimal(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? 0m : lector.GetDecimal(indice);
    }

    public static DateTime FechaHora(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? DateTime.MinValue : lector.GetDateTime(indice);
    }

    public static DateTime? FechaHoraNula(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetDateTime(indice);
    }

    /// <summary>Convierte una columna DATE de SQL Server a DateOnly.</summary>
    public static DateOnly Fecha(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice)
            ? DateOnly.MinValue
            : DateOnly.FromDateTime(lector.GetDateTime(indice));
    }

    /// <summary>Convierte una columna TIME de SQL Server a TimeOnly.</summary>
    public static TimeOnly Hora(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice)
            ? TimeOnly.MinValue
            : TimeOnly.FromTimeSpan(lector.GetFieldValue<TimeSpan>(indice));
    }

    public static byte[] Binario(this SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? Array.Empty<byte>() : (byte[])lector.GetValue(indice);
    }
}
