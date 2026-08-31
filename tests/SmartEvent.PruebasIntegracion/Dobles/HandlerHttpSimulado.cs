using System.Net;
using System.Text;

namespace SmartEvent.PruebasIntegracion.Dobles;

/// <summary>Una peticion capturada, para poder comprobar lo que realmente se envio.</summary>
internal sealed class PeticionCapturada
{
    public required string Ruta { get; init; }
    public required string Cuerpo { get; init; }
    public string? Autorizacion { get; init; }
}

/// <summary>
/// Manipulador HTTP de prueba.
///
/// Sustituye la red por una cola de respuestas programadas. Esto permite comprobar el cliente
/// de IA por completo (cabeceras, cuerpo enviado, repliegue de endpoint, reintentos, lectura
/// de la respuesta y validacion del contrato) de forma DETERMINISTA, sin clave de API, sin
/// coste y sin depender de que el proveedor este disponible.
///
/// Es la razon de que ServicioAnalisisIAHttp acepte un HttpClient por constructor: sin esa
/// costura, la unica forma de probarlo seria llamar al proveedor real.
/// </summary>
internal sealed class HandlerHttpSimulado : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _respuestas = new();

    public List<PeticionCapturada> Peticiones { get; } = new();

    /// <summary>Retardo artificial para provocar el tiempo de espera del cliente.</summary>
    public TimeSpan Retardo { get; set; } = TimeSpan.Zero;

    public HandlerHttpSimulado Responder(HttpStatusCode codigo, string cuerpo)
    {
        _respuestas.Enqueue(_ => new HttpResponseMessage(codigo)
        {
            Content = new StringContent(cuerpo, Encoding.UTF8, "application/json")
        });

        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion,
                                                                 CancellationToken cancelacion)
    {
        var cuerpo = peticion.Content is null
            ? string.Empty
            : await peticion.Content.ReadAsStringAsync(cancelacion);

        Peticiones.Add(new PeticionCapturada
        {
            Ruta = peticion.RequestUri?.AbsolutePath ?? string.Empty,
            Cuerpo = cuerpo,
            Autorizacion = peticion.Headers.Authorization?.ToString()
        });

        if (Retardo > TimeSpan.Zero)
        {
            await Task.Delay(Retardo, cancelacion);
        }

        if (_respuestas.Count == 0)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{\"error\":{\"message\":\"sin respuesta programada\"}}",
                                            Encoding.UTF8, "application/json")
            };
        }

        return _respuestas.Dequeue()(peticion);
    }
}
