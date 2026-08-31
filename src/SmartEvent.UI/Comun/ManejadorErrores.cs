using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.UI.Comun;

/// <summary>
/// Manejo CENTRALIZADO de excepciones en la interfaz.
///
/// Es el unico punto de la aplicacion que decide que texto ve el usuario cuando algo falla,
/// y aplica una regla sin excepciones:
///
///   - ReglaNegocioException     -> se muestra su mensaje tal cual. Viene de un THROW con
///                                  numero >= 50000 en un procedimiento almacenado o de una
///                                  validacion equivalente: esta redactado para el usuario.
///   - ErrorConectividadException-> mensaje de red, con opcion de reintentar.
///   - ConfiguracionException    -> mensaje que explica que falta configurar.
///   - Cualquier otra            -> mensaje GENERICO. El detalle tecnico va solo al log.
///
/// Nunca se muestra un stack trace, un nombre de tabla, una sentencia SQL ni una cadena de
/// conexion. Esa es la diferencia entre un error util y una filtracion de informacion.
/// </summary>
public static class ManejadorErrores
{
    private static IRegistroEventos? _registro;

    public static void Inicializar(IRegistroEventos registro) => _registro = registro;

    /// <summary>
    /// Muestra la excepcion al usuario con el mensaje que le corresponde y la registra.
    /// Devuelve false si la excepcion era una cancelacion (no hay nada que mostrar).
    /// </summary>
    public static bool Mostrar(IWin32Window? propietario, Exception excepcion, string? contexto = null)
    {
        ArgumentNullException.ThrowIfNull(excepcion);

        // Una cancelacion no es un error: la pidio el propio usuario.
        if (excepcion is OperationCanceledException)
        {
            return false;
        }

        var (titulo, mensaje, icono) = Traducir(excepcion);

        Registrar(excepcion, contexto);

        MessageBox.Show(propietario, mensaje, titulo, MessageBoxButtons.OK, icono);

        return true;
    }

    /// <summary>Traduce la excepcion al par titulo/mensaje que se mostrara.</summary>
    public static (string Titulo, string Mensaje, MessageBoxIcon Icono) Traducir(Exception excepcion) =>
        excepcion switch
        {
            ReglaNegocioException regla =>
                ("No se puede completar la operacion", regla.Message, MessageBoxIcon.Warning),

            ErrorConectividadException conectividad =>
                ("Sin conexion", conectividad.Message, MessageBoxIcon.Warning),

            ConfiguracionException configuracion =>
                ("Configuracion incompleta", configuracion.Message, MessageBoxIcon.Warning),

            ErrorTecnicoException tecnico =>
                ("Error del sistema", tecnico.Message, MessageBoxIcon.Error),

            TimeoutException =>
                ("Tiempo de espera agotado",
                 "La operacion tardo mas de lo previsto. Intentelo nuevamente.", MessageBoxIcon.Warning),

            _ =>
                // Cualquier excepcion no prevista se presenta con el mismo texto neutro.
                ("Error del sistema", ErrorTecnicoException.MensajeGenerico, MessageBoxIcon.Error)
        };

    private static void Registrar(Exception excepcion, string? contexto)
    {
        var descripcion = string.IsNullOrWhiteSpace(contexto)
            ? "Error mostrado al usuario."
            : $"Error mostrado al usuario durante: {contexto}.";

        switch (excepcion)
        {
            case ReglaNegocioException regla:
                // No es un fallo del sistema: es una regla funcionando. Se anota como informacion.
                _registro?.Informacion($"{descripcion} Regla de negocio (codigo {regla.CodigoError}).");
                break;

            case ErrorConectividadException conectividad:
                _registro?.Advertencia($"{descripcion} {conectividad.DetalleTecnico}");
                break;

            case ErrorTecnicoException tecnico:
                _registro?.Error($"{descripcion} {tecnico.DetalleTecnico}", excepcion);
                break;

            default:
                _registro?.Error(descripcion, excepcion);
                break;
        }
    }

    /// <summary>Confirmacion estandar para acciones que el usuario debe ratificar.</summary>
    public static bool Confirmar(IWin32Window? propietario, string mensaje, string titulo = "Confirmar") =>
        MessageBox.Show(propietario, mensaje, titulo,
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public static void Informar(IWin32Window? propietario, string mensaje, string titulo = "SmartEvent") =>
        MessageBox.Show(propietario, mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Advertir(IWin32Window? propietario, string mensaje, string titulo = "Atencion") =>
        MessageBox.Show(propietario, mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
