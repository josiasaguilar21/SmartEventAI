using System.Data;
using Microsoft.Data.SqlClient;

namespace SmartEvent.Infrastructure.Data.Comun;

/// <summary>
/// Construccion de parametros FUERTEMENTE TIPADOS.
///
/// Todos los valores del usuario entran por aqui. En ninguna parte del proyecto se concatena
/// un valor dentro de una cadena SQL: el tipo, el tamano, la precision y la escala se declaran
/// explicitamente. Declarar el tamano no es un detalle esteril: evita conversiones implicitas
/// que degradan el plan de ejecucion y deja el contrato del procedimiento a la vista.
/// </summary>
internal static class ExtensionesParametros
{
    public static SqlParameter AgregarEntero(this SqlCommand comando, string nombre, int? valor)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Int)
        {
            Value = valor.HasValue ? valor.Value : DBNull.Value
        };
        return comando.Parameters.Add(parametro);
    }

    public static SqlParameter AgregarTexto(this SqlCommand comando, string nombre, string? valor, int tamano)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.NVarChar, tamano)
        {
            Value = string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim()
        };
        return comando.Parameters.Add(parametro);
    }

    public static SqlParameter AgregarTextoAscii(this SqlCommand comando, string nombre, string? valor, int tamano)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.VarChar, tamano)
        {
            Value = string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim()
        };
        return comando.Parameters.Add(parametro);
    }

    public static SqlParameter AgregarBooleano(this SqlCommand comando, string nombre, bool? valor)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Bit)
        {
            Value = valor.HasValue ? valor.Value : DBNull.Value
        };
        return comando.Parameters.Add(parametro);
    }

    public static SqlParameter AgregarDecimal(this SqlCommand comando, string nombre, decimal? valor,
                                              byte precision = 12, byte escala = 2)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Decimal)
        {
            Precision = precision,
            Scale = escala,
            Value = valor.HasValue ? valor.Value : DBNull.Value
        };
        return comando.Parameters.Add(parametro);
    }

    /// <summary>
    /// DateOnly se envia como SqlDbType.Date. Se convierte a DateTime a medianoche porque es
    /// el mapeo garantizado en todas las versiones del proveedor; el motor recibe un DATE puro.
    /// </summary>
    public static SqlParameter AgregarFecha(this SqlCommand comando, string nombre, DateOnly? valor)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Date)
        {
            Value = valor.HasValue ? valor.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value
        };
        return comando.Parameters.Add(parametro);
    }

    /// <summary>TimeOnly se envia como SqlDbType.Time con precision de segundos, igual que TIME(0) en la tabla.</summary>
    public static SqlParameter AgregarHora(this SqlCommand comando, string nombre, TimeOnly? valor)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Time)
        {
            Scale = 0,
            Value = valor.HasValue ? valor.Value.ToTimeSpan() : DBNull.Value
        };
        return comando.Parameters.Add(parametro);
    }

    public static SqlParameter AgregarBinario(this SqlCommand comando, string nombre, byte[]? valor, int tamano)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.VarBinary, tamano)
        {
            Value = valor is null ? DBNull.Value : valor
        };
        return comando.Parameters.Add(parametro);
    }

    /// <summary>
    /// Parametro tipo tabla (TVP). <paramref name="nombreTipo"/> debe coincidir exactamente con
    /// el TYPE creado en la base, incluido su esquema (evt.ReservaDetalleType).
    /// </summary>
    public static SqlParameter AgregarTabla(this SqlCommand comando, string nombre, DataTable tabla, string nombreTipo)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Structured)
        {
            TypeName = nombreTipo,
            Value = tabla
        };
        return comando.Parameters.Add(parametro);
    }

    // ------------------------------------------------------------------ parametros de salida

    public static SqlParameter AgregarSalidaEntero(this SqlCommand comando, string nombre)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Int) { Direction = ParameterDirection.Output };
        return comando.Parameters.Add(parametro);
    }

    public static SqlParameter AgregarSalidaBooleano(this SqlCommand comando, string nombre)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Bit) { Direction = ParameterDirection.Output };
        return comando.Parameters.Add(parametro);
    }

    public static SqlParameter AgregarSalidaTexto(this SqlCommand comando, string nombre, int tamano)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.NVarChar, tamano) { Direction = ParameterDirection.Output };
        return comando.Parameters.Add(parametro);
    }

    // ------------------------------------------------------- lectura segura de los resultados

    public static int ValorEntero(this SqlParameter parametro) =>
        parametro.Value is null or DBNull ? 0 : Convert.ToInt32(parametro.Value);

    public static int? ValorEnteroNulo(this SqlParameter parametro) =>
        parametro.Value is null or DBNull ? null : Convert.ToInt32(parametro.Value);

    public static string ValorTexto(this SqlParameter parametro) =>
        parametro.Value is null or DBNull ? string.Empty : Convert.ToString(parametro.Value) ?? string.Empty;

    public static bool ValorBooleano(this SqlParameter parametro) =>
        parametro.Value is not (null or DBNull) && Convert.ToBoolean(parametro.Value);
}
