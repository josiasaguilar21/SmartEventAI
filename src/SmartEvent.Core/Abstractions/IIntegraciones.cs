using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Abstractions;

/// <summary>
/// Composicion y envio de las notificaciones al cliente.
///
/// La implementacion vive en SmartEvent.Infrastructure.Integrations y usa MailKit. La capa de
/// aplicacion solo conoce esta interfaz, de modo que un fallo de correo jamas puede afectar la
/// transaccion de la reserva.
/// </summary>
public interface IServicioCorreo
{
    /// <summary>
    /// Compone el mensaje HTML de la reserva. Todos los valores de negocio se codifican para
    /// HTML antes de insertarse en la plantilla, de modo que un nombre con caracteres como
    /// &lt; o &amp; no pueda alterar la estructura del correo.
    /// </summary>
    MensajeCorreo Componer(Reserva reserva, TipoNotificacion tipo);

    /// <summary>
    /// Intenta enviar el mensaje. NO lanza excepcion ante un fallo de SMTP: devuelve el
    /// resultado con el error tecnico controlado para que se audite y pueda reintentarse.
    /// </summary>
    Task<ResultadoCorreo> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancelacion);

    /// <summary>Indica si hay configuracion SMTP suficiente para intentar un envio.</summary>
    bool EstaConfigurado { get; }
}

/// <summary>
/// Analisis de la reserva con un modelo de lenguaje, usando salidas estructuradas validadas
/// contra un JSON Schema estricto.
///
/// La IA SOLO ASESORA: esta interfaz no expone ninguna operacion que confirme, cancele,
/// modifique totales o toque la base de datos. Devuelve texto estructurado y nada mas.
/// </summary>
public interface IServicioAnalisisIA
{
    /// <summary>
    /// Ejecuta el analisis. Ante timeout, falta de clave, error HTTP, respuesta vacia o JSON
    /// invalido devuelve un resultado con Exitoso = false y un mensaje tecnico controlado;
    /// nunca deja caer la aplicacion (CA-09).
    /// </summary>
    Task<AnalisisIAResultado> AnalizarAsync(AnalisisIASolicitud solicitud, CancellationToken cancelacion);

    /// <summary>Indica si hay clave configurada para intentar la llamada.</summary>
    bool EstaConfigurado { get; }

    /// <summary>Modelo configurado; se muestra en la pantalla de auditoria.</summary>
    string Modelo { get; }
}
