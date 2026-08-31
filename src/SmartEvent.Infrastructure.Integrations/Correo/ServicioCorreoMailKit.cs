using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Infrastructure.Integrations.Configuracion;

namespace SmartEvent.Infrastructure.Integrations.Correo;

/// <summary>
/// Envio de notificaciones por SMTP con MailKit.
///
/// TRES DECISIONES QUE DEFINEN ESTA CLASE:
///
/// 1. NUNCA LANZA POR UN FALLO DE ENVIO. Devuelve <see cref="ResultadoCorreo"/> con el error.
///    Un servidor de correo caido no puede revertir una reserva que SQL Server ya confirmo,
///    asi que el fallo tiene que ser un dato que se audita, no una excepcion que interrumpe.
///
/// 2. LA CONEXION ES POR ENVIO. Se abre, se envia y se cierra dentro de un using. No se
///    mantiene un SmtpClient vivo entre operaciones: los servidores cierran las sesiones
///    inactivas y un cliente compartido acaba fallando de forma intermitente.
///
/// 3. LOS MENSAJES DE ERROR SE REDACTAN AQUI. Se traduce la excepcion tecnica a un texto
///    controlado, sin host, sin usuario y sin contrasena, apto para guardarse en
///    com.CorreoEnviado y mostrarse en la pantalla de auditoria.
/// </summary>
public sealed class ServicioCorreoMailKit : IServicioCorreo
{
    private readonly OpcionesSmtp _opciones;
    private readonly IRegistroEventos _registro;

    public ServicioCorreoMailKit(OpcionesSmtp opciones, IRegistroEventos registro)
    {
        _opciones = opciones;
        _registro = registro;
    }

    public bool EstaConfigurado => _opciones.EstaCompleta;

    public MensajeCorreo Componer(Reserva reserva, TipoNotificacion tipo)
    {
        ArgumentNullException.ThrowIfNull(reserva);

        // La redireccion de pruebas permite demostrar el envio con datos ficticios sin escribir
        // nunca a la direccion real de un cliente.
        var destinatario = string.IsNullOrWhiteSpace(_opciones.RedireccionPruebas)
            ? reserva.EmailCliente
            : _opciones.RedireccionPruebas;

        return new MensajeCorreo
        {
            IdReserva = reserva.IdReserva,
            Tipo = tipo,
            Destinatario = destinatario,
            NombreDestinatario = reserva.Cliente,
            Asunto = PlantillaCorreoHtml.ComponerAsunto(reserva, tipo),
            CuerpoHtml = PlantillaCorreoHtml.ComponerHtml(reserva, tipo),
            CuerpoTexto = PlantillaCorreoHtml.ComponerTexto(reserva, tipo)
        };
    }

    public async Task<ResultadoCorreo> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancelacion)
    {
        ArgumentNullException.ThrowIfNull(mensaje);

        var inicio = DateTime.Now;

        if (!EstaConfigurado)
        {
            return Fallo(mensaje, inicio,
                "El servicio de correo no esta configurado en este equipo. " +
                "Defina las variables SMARTEVENT_SMTP_* indicadas en el README.");
        }

        try
        {
            var correo = ConstruirMimeMessage(mensaje);

            // SmtpClient de MailKit implementa IDisposable, no IAsyncDisposable: el using es
            // sincronico, pero todas las operaciones de red que hace dentro son asincronicas.
            using var cliente = new SmtpClient
            {
                // Tiempo maximo de cada operacion de red, en milisegundos. Sin esto, un servidor
                // que acepta la conexion y no responde dejaria la interfaz esperando sin limite.
                Timeout = _opciones.TimeoutSegundos * 1000
            };

            var seguridad = _opciones.UsarSslImplicito
                ? SecureSocketOptions.SslOnConnect          // normalmente puerto 465
                : SecureSocketOptions.StartTls;             // normalmente puerto 587

            await cliente.ConnectAsync(_opciones.Host, _opciones.Puerto, seguridad, cancelacion)
                         .ConfigureAwait(false);

            await cliente.AuthenticateAsync(_opciones.Usuario, _opciones.Contrasena, cancelacion)
                         .ConfigureAwait(false);

            await cliente.SendAsync(correo, cancelacion).ConfigureAwait(false);

            await cliente.DisconnectAsync(quit: true, cancelacion).ConfigureAwait(false);

            _registro.Informacion(
                $"Correo enviado para la reserva {mensaje.IdReserva} ({mensaje.Tipo}).");

            return new ResultadoCorreo
            {
                Enviado = true,
                Destinatario = mensaje.Destinatario,
                Asunto = mensaje.Asunto,
                FechaIntento = inicio
            };
        }
        catch (OperationCanceledException)
        {
            // Cancelacion pedida por el usuario: se deja subir para que la interfaz no la
            // registre como un fallo del servidor.
            throw;
        }
        catch (AuthenticationException ex)
        {
            return RegistrarYFallar(mensaje, inicio, ex,
                "El servidor de correo rechazo las credenciales configuradas. " +
                "Verifique el usuario y la clave de aplicacion en las variables de entorno.");
        }
        catch (SslHandshakeException ex)
        {
            return RegistrarYFallar(mensaje, inicio, ex,
                "No se pudo establecer una conexion segura con el servidor de correo. " +
                "Revise el puerto y el modo de cifrado configurados.");
        }
        catch (SmtpCommandException ex)
        {
            // El servidor respondio pero rechazo la operacion: destinatario invalido, buzon
            // lleno, remitente no autorizado. Se informa el codigo, que no es un secreto.
            return RegistrarYFallar(mensaje, inicio, ex,
                $"El servidor de correo rechazo el mensaje (codigo {ex.StatusCode}). " +
                "Verifique que la direccion del destinatario sea correcta.");
        }
        catch (SmtpProtocolException ex)
        {
            return RegistrarYFallar(mensaje, inicio, ex,
                "Error de protocolo al comunicarse con el servidor de correo.");
        }
        catch (TimeoutException ex)
        {
            return RegistrarYFallar(mensaje, inicio, ex,
                $"El servidor de correo no respondio dentro de los {_opciones.TimeoutSegundos} segundos configurados.");
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            return RegistrarYFallar(mensaje, inicio, ex,
                "No fue posible conectar con el servidor de correo. Verifique su conexion de red.");
        }
        catch (IOException ex)
        {
            return RegistrarYFallar(mensaje, inicio, ex,
                "Se interrumpio la comunicacion con el servidor de correo durante el envio.");
        }
        catch (Exception ex)
        {
            // Red de seguridad final: ningun fallo inesperado del correo puede propagarse.
            return RegistrarYFallar(mensaje, inicio, ex,
                "No fue posible enviar la notificacion. El detalle tecnico quedo en el registro local.");
        }
    }

    private MimeMessage ConstruirMimeMessage(MensajeCorreo mensaje)
    {
        var correo = new MimeMessage();

        correo.From.Add(new MailboxAddress(_opciones.RemitenteNombre, _opciones.CorreoRemitenteEfectivo));
        correo.To.Add(new MailboxAddress(mensaje.NombreDestinatario, mensaje.Destinatario));
        correo.Subject = mensaje.Asunto;

        // Cuerpo multiparte: HTML para los clientes que lo muestran y texto plano de respaldo.
        var cuerpo = new BodyBuilder
        {
            HtmlBody = mensaje.CuerpoHtml,
            TextBody = mensaje.CuerpoTexto
        };

        correo.Body = cuerpo.ToMessageBody();

        return correo;
    }

    private ResultadoCorreo RegistrarYFallar(MensajeCorreo mensaje, DateTime inicio, Exception excepcion,
                                             string mensajeUsuario)
    {
        // El log guarda el tipo de excepcion para diagnosticar; el saneamiento del registro
        // enmascara cualquier credencial que pudiera arrastrar el mensaje de la libreria.
        _registro.Advertencia(
            $"Fallo el envio del correo de la reserva {mensaje.IdReserva} ({excepcion.GetType().Name}).",
            excepcion);

        return Fallo(mensaje, inicio, mensajeUsuario);
    }

    private static ResultadoCorreo Fallo(MensajeCorreo mensaje, DateTime inicio, string error) => new()
    {
        Enviado = false,
        Destinatario = mensaje.Destinatario,
        Asunto = mensaje.Asunto,
        FechaIntento = inicio,
        Error = error
    };
}
