using System.Text;
using System.Text.RegularExpressions;
using SmartEvent.Core.Abstractions;

namespace SmartEvent.Infrastructure.Data.Registro;

/// <summary>
/// Registro de eventos en archivo de texto con rotacion diaria.
///
/// Es un servicio transversal: lo usan la capa de datos, las integraciones y la presentacion.
/// Todas dependen de <see cref="IRegistroEventos"/>, nunca de esta clase, de modo que puede
/// sustituirse sin tocar el resto del sistema.
///
/// SANEAMIENTO OBLIGATORIO: antes de escribir, <see cref="Sanear"/> enmascara todo lo que
/// parezca una contrasena, una clave de API o una cadena de conexion. Es una segunda barrera;
/// la primera es no pasar nunca esos datos a este servicio.
/// </summary>
public sealed class RegistroEventosArchivo : IRegistroEventos
{
    private readonly string _carpeta;
    private readonly object _candado = new();

    /// <summary>
    /// Patrones de datos sensibles. Si alguna vez un mensaje los arrastra por descuido,
    /// el valor queda enmascarado en el archivo.
    /// </summary>
    private static readonly Regex[] PatronesSensibles =
    {
        new(@"(?i)(password|pwd|contrase(?:n|ñ)a)\s*=\s*[^;""',\s]+", RegexOptions.Compiled),
        new(@"(?i)(user\s*id|uid)\s*=\s*[^;""',\s]+", RegexOptions.Compiled),
        new(@"(?i)(api[_-]?key|authorization|bearer)\s*[:=]\s*[^;""',\s]+", RegexOptions.Compiled),
        new(@"sk-[A-Za-z0-9_\-]{16,}", RegexOptions.Compiled),
        new(@"(?i)(data\s*source|server)\s*=\s*[^;""',\s]+", RegexOptions.Compiled)
    };

    public RegistroEventosArchivo(string? carpeta = null)
    {
        _carpeta = string.IsNullOrWhiteSpace(carpeta)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                           "SmartEventAI", "logs")
            : carpeta;

        Directory.CreateDirectory(_carpeta);
    }

    /// <summary>Ruta del archivo del dia; se muestra en la pantalla de auditoria para diagnostico.</summary>
    public string ArchivoActual => Path.Combine(_carpeta, $"smartevent-{DateTime.Now:yyyyMMdd}.log");

    public void Informacion(string mensaje) => Escribir("INFO ", mensaje, null);

    public void Advertencia(string mensaje, Exception? excepcion = null) => Escribir("AVISO", mensaje, excepcion);

    public void Error(string mensaje, Exception? excepcion = null) => Escribir("ERROR", mensaje, excepcion);

    private void Escribir(string nivel, string mensaje, Exception? excepcion)
    {
        var linea = new StringBuilder()
            .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
            .Append(" [").Append(nivel).Append("] ")
            .Append(Sanear(mensaje));

        if (excepcion is not null)
        {
            linea.AppendLine()
                 .Append("        -> ").Append(excepcion.GetType().Name).Append(": ")
                 .Append(Sanear(excepcion.Message));

            if (excepcion.InnerException is not null)
            {
                linea.AppendLine()
                     .Append("        -> interna: ").Append(excepcion.InnerException.GetType().Name)
                     .Append(": ").Append(Sanear(excepcion.InnerException.Message));
            }
        }

        try
        {
            // El registro se escribe desde varios hilos (operaciones asincronas simultaneas).
            // El bloqueo es breve y solo protege la escritura del archivo.
            lock (_candado)
            {
                File.AppendAllText(ArchivoActual, linea.AppendLine().ToString(), Encoding.UTF8);
            }
        }
        catch (Exception)
        {
            // Si no se puede escribir el log (disco lleno, permisos), la aplicacion debe seguir
            // funcionando. Un fallo del registro nunca puede tumbar una operacion del usuario.
        }
    }

    /// <summary>Enmascara valores sensibles antes de que lleguen al archivo.</summary>
    internal static string Sanear(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return string.Empty;
        }

        var resultado = texto;

        foreach (var patron in PatronesSensibles)
        {
            resultado = patron.Replace(resultado, coincidencia =>
            {
                var separador = coincidencia.Value.Contains('=') ? '=' : ':';
                var indice = coincidencia.Value.IndexOf(separador);

                return indice < 0
                    ? "***"
                    : string.Concat(coincidencia.Value.AsSpan(0, indice + 1), "***");
            });
        }

        return resultado;
    }
}
