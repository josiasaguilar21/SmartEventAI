namespace SmartEvent.Core.Exceptions;

/// <summary>
/// Raiz de las excepciones propias de SmartEvent. Permite que la capa de presentacion
/// distinga en un solo catch lo que es "nuestro" de lo que viene del entorno.
/// </summary>
public abstract class SmartEventException : Exception
{
    protected SmartEventException(string mensaje, Exception? interna = null)
        : base(mensaje, interna) { }

    /// <summary>
    /// Indica si <see cref="Exception.Message"/> puede mostrarse tal cual al usuario final.
    /// Solo es verdadero en las excepciones cuyo texto fue redactado deliberadamente para el.
    /// </summary>
    public abstract bool MensajeAptoParaUsuario { get; }
}

/// <summary>
/// Violacion de una regla de negocio. El mensaje SIEMPRE esta redactado para el usuario final:
/// procede de un THROW con numero >= 50000 en un procedimiento almacenado o de una validacion
/// equivalente en la capa de aplicacion. Nunca contiene SQL, rutas ni datos de conexion.
/// </summary>
public sealed class ReglaNegocioException : SmartEventException
{
    /// <summary>Numero de error de SQL Server cuando la regla la impuso la base (0 si es una validacion local).</summary>
    public int CodigoError { get; }

    public ReglaNegocioException(string mensaje, int codigoError = 0, Exception? interna = null)
        : base(mensaje, interna)
    {
        CodigoError = codigoError;
    }

    public override bool MensajeAptoParaUsuario => true;
}

/// <summary>
/// Fallo tecnico no atribuible al usuario: error del motor, esquema inesperado, conversion
/// imposible. El detalle tecnico viaja en <see cref="DetalleTecnico"/> para el log; el
/// <see cref="Exception.Message"/> es un texto neutro que si puede llegar a la pantalla.
/// </summary>
public sealed class ErrorTecnicoException : SmartEventException
{
    public const string MensajeGenerico =
        "Ocurrio un problema al procesar la operacion. Intentelo nuevamente; " +
        "si el problema persiste comuniquese con el administrador del sistema.";

    public string DetalleTecnico { get; }

    public ErrorTecnicoException(string detalleTecnico, Exception? interna = null)
        : base(MensajeGenerico, interna)
    {
        DetalleTecnico = detalleTecnico;
    }

    public override bool MensajeAptoParaUsuario => true;
}

/// <summary>
/// Problema de conectividad o tiempo de espera contra un recurso externo (SQL Server, SMTP,
/// proveedor de IA). Se separa del error tecnico porque la interfaz suele ofrecer reintentar.
/// </summary>
public sealed class ErrorConectividadException : SmartEventException
{
    public string DetalleTecnico { get; }

    public ErrorConectividadException(string mensajeUsuario, string detalleTecnico, Exception? interna = null)
        : base(mensajeUsuario, interna)
    {
        DetalleTecnico = detalleTecnico;
    }

    public override bool MensajeAptoParaUsuario => true;
}

/// <summary>
/// Error de configuracion detectado al arrancar o al usar una integracion: falta la cadena de
/// conexion, la variable OPENAI_API_KEY o las credenciales SMTP. Nunca incluye el valor faltante.
/// </summary>
public sealed class ConfiguracionException : SmartEventException
{
    public ConfiguracionException(string mensaje, Exception? interna = null)
        : base(mensaje, interna) { }

    public override bool MensajeAptoParaUsuario => true;
}
