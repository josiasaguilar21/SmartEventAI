using System.Text;
using System.Text.Json;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;

namespace SmartEvent.PruebasIntegracion.Dobles;

/// <summary>
/// Doble de prueba del servicio de correo.
///
/// Permite provocar a voluntad el fallo de SMTP que exige CA-07 sin necesidad de apagar un
/// servidor real. Es exactamente lo que hace posible que la capa de aplicacion se pruebe:
/// como ServicioReservas depende de la interfaz IServicioCorreo y no de MailKit, aqui se
/// sustituye la implementacion sin tocar una linea del servicio.
/// </summary>
internal sealed class CorreoSimulado : IServicioCorreo
{
    public bool EstaConfigurado { get; set; } = true;

    /// <summary>Cuando es verdadero, el envio falla como si el servidor SMTP no respondiera.</summary>
    public bool DebeFallar { get; set; }

    public int IntentosDeEnvio { get; private set; }

    public MensajeCorreo? UltimoMensaje { get; private set; }

    public MensajeCorreo Componer(Reserva reserva, TipoNotificacion tipo)
    {
        var titulo = tipo == TipoNotificacion.Cancelacion ? "Cancelacion" : "Confirmacion";

        var cuerpo = new StringBuilder()
            .Append("<h1>").Append(titulo).Append(" de reserva ").Append(reserva.Codigo).Append("</h1>")
            .Append("<p>Cliente: ").Append(reserva.Cliente).Append("</p>")
            .Append("<p>Total: ").Append(reserva.Total.ToString("N2")).Append("</p>")
            .ToString();

        UltimoMensaje = new MensajeCorreo
        {
            IdReserva = reserva.IdReserva,
            Tipo = tipo,
            Destinatario = reserva.EmailCliente,
            NombreDestinatario = reserva.Cliente,
            Asunto = $"{titulo} de su reserva {reserva.Codigo}",
            CuerpoHtml = cuerpo,
            CuerpoTexto = $"{titulo} de reserva {reserva.Codigo}. Total: {reserva.Total:N2}"
        };

        return UltimoMensaje;
    }

    public Task<ResultadoCorreo> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancelacion)
    {
        IntentosDeEnvio++;

        var resultado = new ResultadoCorreo
        {
            Enviado = !DebeFallar,
            Destinatario = mensaje.Destinatario,
            Asunto = mensaje.Asunto,
            FechaIntento = DateTime.Now,
            Error = DebeFallar
                ? "No fue posible conectar con el servidor SMTP (tiempo de espera agotado)."
                : null
        };

        return Task.FromResult(resultado);
    }
}

/// <summary>
/// Doble de prueba del servicio de analisis con IA.
///
/// Reproduce los tres escenarios que hay que demostrar: servicio sin configurar (CA-09),
/// analisis exitoso con JSON estructurado valido (CA-08) y fallo del proveedor.
/// </summary>
internal sealed class AnalisisIASimulado : IServicioAnalisisIA
{
    public bool EstaConfigurado { get; set; } = true;

    public string Modelo => "modelo-simulado-pruebas";

    public bool DebeFallar { get; set; }

    public int Llamadas { get; private set; }

    public Task<AnalisisIAResultado> AnalizarAsync(AnalisisIASolicitud solicitud, CancellationToken cancelacion)
    {
        Llamadas++;

        if (DebeFallar)
        {
            return Task.FromResult(AnalisisIAResultado.Fallo(
                Modelo, "v1", "Tiempo de espera agotado al contactar con el proveedor del modelo."));
        }

        // Respuesta coherente con la solicitud, para comprobar que la proyeccion llega completa.
        var respuesta = new AnalisisIARespuesta
        {
            NivelRiesgo = solicitud.PorcentajeOcupacion > 90 ? "ALTO" : "MEDIO",
            Resumen = $"Evento de {solicitud.HorasDuracion:0.#} horas en {solicitud.Salon} " +
                      $"con {solicitud.PorcentajeOcupacion:0.#} por ciento de ocupacion.",
            Alertas = solicitud.DiasHastaEvento < 7
                ? new List<string> { "El evento se realiza con menos de una semana de anticipacion." }
                : new List<string>(),
            Recomendaciones = new List<string>
            {
                "Confirmar el montaje con el proveedor 24 horas antes.",
                "Verificar la disponibilidad del personal de logistica."
            },
            CorreoSugerido = $"Estimado cliente, confirmamos su reserva {solicitud.CodigoReserva}."
        };

        var resultado = new AnalisisIAResultado
        {
            Exitoso = true,
            Modelo = Modelo,
            PromptVersion = "v1",
            Respuesta = respuesta,
            RespuestaJson = JsonSerializer.Serialize(respuesta),
            TokensEntrada = 420,
            TokensSalida = 180
        };

        return Task.FromResult(resultado);
    }
}
