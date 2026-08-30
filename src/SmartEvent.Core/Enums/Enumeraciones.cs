namespace SmartEvent.Core.Enums;

/// <summary>
/// Estados del ciclo de vida de una reserva.
/// El flujo permitido (BORRADOR -> CONFIRMADA -> FINALIZADA, y cancelacion desde los dos
/// primeros) esta modelado como dato en la tabla evt.TransicionEstado; este enum solo
/// representa los valores, nunca decide por si mismo que transicion es valida.
/// </summary>
public enum EstadoReserva
{
    Borrador = 1,
    Confirmada = 2,
    Finalizada = 3,
    Cancelada = 4
}

/// <summary>Clasificacion de los recursos y servicios del catalogo.</summary>
public enum TipoRecurso
{
    Equipo = 1,
    Mobiliario = 2,
    Servicio = 3,
    Catering = 4
}

/// <summary>Resultado del intento de envio de un correo, tal como se audita en com.CorreoEnviado.</summary>
public enum EstadoCorreo
{
    Enviado = 1,
    Error = 2
}

/// <summary>Motivo por el que se genera una notificacion al cliente.</summary>
public enum TipoNotificacion
{
    Confirmacion = 1,
    Cancelacion = 2,
    /// <summary>Reintento explicito solicitado por el usuario tras un fallo SMTP (CA-07).</summary>
    Reenvio = 3
}

/// <summary>Nivel de riesgo devuelto por el analisis de IA dentro del JSON estructurado.</summary>
public enum NivelRiesgo
{
    Bajo = 1,
    Medio = 2,
    Alto = 3
}

/// <summary>
/// Codigos devueltos por seg.sp_Usuario_Autenticar. Se mantienen alineados con los valores
/// numericos del procedimiento almacenado.
/// </summary>
public enum ResultadoAutenticacion
{
    Correcto = 0,
    CredencialesInvalidas = 1,
    CuentaBloqueada = 2
}
