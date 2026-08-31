using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Infrastructure.Integrations.Configuracion;
using SmartEvent.Infrastructure.Integrations.Correo;
using SmartEvent.Infrastructure.Integrations.IA;
using SmartEvent.PruebasIntegracion.Dobles;

namespace SmartEvent.PruebasIntegracion;

/// <summary>
/// Pruebas de las INTEGRACIONES EXTERNAS: composicion y envio de correo con MailKit, y cliente
/// del servicio de IA con salidas estructuradas.
///
/// Las del cliente de IA usan un manipulador HTTP simulado: no requieren clave, no gastan
/// cuota y son deterministas. Comprueban justo lo que hay que demostrar en la defensa: que la
/// peticion lleva el JSON Schema estricto, que la respuesta se valida antes de aceptarla y que
/// ningun fallo del proveedor tumba la aplicacion.
/// </summary>
internal static class PruebasIntegraciones
{
    public static async Task EjecutarAsync(IRegistroEventos registro, Action<bool, string> verificar,
                                           CancellationToken ct)
    {
        ProbarPlantillaCorreo(verificar);
        await ProbarServicioCorreo(registro, verificar, ct);
        await ProbarClienteIA(registro, verificar, ct);
    }

    // ------------------------------------------------------------------------------- correo

    /// <summary>
    /// La plantilla debe codificar TODO valor de negocio. Es la prueba de que un dato con
    /// etiquetas HTML no puede alterar la estructura del correo del destinatario.
    /// </summary>
    private static void ProbarPlantillaCorreo(Action<bool, string> verificar)
    {
        Console.WriteLine();
        Console.WriteLine("--- Composicion del correo HTML ----------------------------------");

        var opciones = new OpcionesSmtp
        {
            Host = "smtp.ejemplo.com",
            Puerto = 587,
            Usuario = "cuenta@ejemplo.com",
            Contrasena = "clave-ficticia",
            RemitenteCorreo = "no-responder@ejemplo.com"
        };

        var servicio = new ServicioCorreoMailKit(opciones, new RegistroSilencioso());

        var reserva = ConstruirReservaDePrueba();
        var mensaje = servicio.Componer(reserva, TipoNotificacion.Confirmacion);

        verificar(mensaje.Asunto.Contains(reserva.Codigo),
                  $"El asunto identifica la reserva: \"{mensaje.Asunto}\"");

        verificar(mensaje.CuerpoHtml.Contains("<table") && mensaje.CuerpoHtml.Contains("Proyector"),
                  "El cuerpo incluye la tabla HTML con el detalle de recursos.");

        verificar(mensaje.CuerpoHtml.Contains("1.164,95") || mensaje.CuerpoHtml.Contains("1164,95") ||
                  mensaje.CuerpoHtml.Contains("1,164.95"),
                  "El cuerpo muestra el total de la reserva con formato de moneda.");

        // El nombre del cliente contiene una etiqueta script; debe salir codificada.
        verificar(!mensaje.CuerpoHtml.Contains("<script>"),
                  "Un nombre con etiquetas HTML no inyecta codigo: la etiqueta script no aparece cruda.");

        verificar(mensaje.CuerpoHtml.Contains("&lt;script&gt;"),
                  "El valor peligroso aparece codificado como texto (&lt;script&gt;).");

        verificar(mensaje.CuerpoHtml.Contains("&amp;") ,
                  "El ampersand del nombre tambien se codifica correctamente.");

        verificar(!string.IsNullOrWhiteSpace(mensaje.CuerpoTexto) && mensaje.CuerpoTexto.Contains("TOTAL"),
                  "Se genera tambien la version en texto plano para clientes sin HTML.");

        // Cancelacion: debe incluir el motivo.
        reserva.Estado = EstadoReserva.Cancelada;
        reserva.MotivoCancelacion = "El cliente reprogramo el evento para el proximo trimestre.";

        var cancelacion = servicio.Componer(reserva, TipoNotificacion.Cancelacion);

        verificar(cancelacion.CuerpoHtml.Contains("reprogramo") && cancelacion.Asunto.Contains("Cancelacion"),
                  "El correo de cancelacion incluye el motivo y lo refleja en el asunto.");

        // Redireccion de pruebas: ningun correo debe llegar al cliente real.
        opciones.RedireccionPruebas = "buzon.pruebas@ejemplo.com";
        var redirigido = servicio.Componer(reserva, TipoNotificacion.Confirmacion);

        verificar(redirigido.Destinatario == "buzon.pruebas@ejemplo.com",
                  "Con redireccion de pruebas activa, el mensaje no va a la direccion del cliente.");
    }

    /// <summary>Un fallo de SMTP debe devolverse como dato, nunca como excepcion.</summary>
    private static async Task ProbarServicioCorreo(IRegistroEventos registro, Action<bool, string> verificar,
                                                   CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-07 : el correo nunca lanza excepciones --------------------");

        // Sin configuracion.
        var sinConfigurar = new ServicioCorreoMailKit(new OpcionesSmtp(), registro);

        verificar(!sinConfigurar.EstaConfigurado, "Sin variables SMTP el servicio se declara no configurado.");

        var mensaje = sinConfigurar.Componer(ConstruirReservaDePrueba(), TipoNotificacion.Confirmacion);
        var resultadoSinConfig = await sinConfigurar.EnviarAsync(mensaje, ct);

        verificar(!resultadoSinConfig.Enviado && resultadoSinConfig.Error is not null,
                  $"Sin configuracion devuelve error controlado: {resultadoSinConfig.Error}");

        // Servidor inexistente: se resuelve por DNS y falla rapido.
        var opcionesMalas = new OpcionesSmtp
        {
            Host = "smtp.servidor-que-no-existe.invalid",
            Puerto = 587,
            Usuario = "cuenta@ejemplo.com",
            Contrasena = "clave-ficticia",
            TimeoutSegundos = 5
        };

        var servicioCaido = new ServicioCorreoMailKit(opcionesMalas, registro);
        var resultadoCaido = await servicioCaido.EnviarAsync(mensaje, ct);

        verificar(!resultadoCaido.Enviado,
                  $"Un servidor inalcanzable devuelve fallo sin lanzar: {resultadoCaido.Error}");

        verificar(resultadoCaido.Error is not null &&
                  !resultadoCaido.Error.Contains("clave-ficticia") &&
                  !resultadoCaido.Error.Contains("cuenta@ejemplo.com"),
                  "El mensaje de error no filtra el usuario ni la contrasena configurados.");

        verificar(resultadoCaido.Estado == EstadoCorreo.Error,
                  "El resultado se clasifica como ERROR para auditarlo en com.CorreoEnviado.");
    }

    // ----------------------------------------------------------------------------------- IA

    private static async Task ProbarClienteIA(IRegistroEventos registro, Action<bool, string> verificar,
                                              CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-08 : peticion con JSON Schema estricto --------------------");

        // ---- 1. Respuesta correcta por la Responses API.
        var handler = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.OK, RespuestaResponses(RespuestaValida()));

        using (var servicio = CrearServicio(handler, registro))
        {
            verificar(servicio.EstaConfigurado, "Con clave configurada el servicio se declara operativo.");

            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(resultado.Exitoso && resultado.Respuesta is not null,
                      $"Analisis correcto. Nivel de riesgo: {resultado.Respuesta?.NivelRiesgo}.");

            verificar(resultado.TokensEntrada == 512 && resultado.TokensSalida == 210,
                      $"Se registra el consumo informado por el proveedor: {resultado.TokensEntrada} / {resultado.TokensSalida} tokens.");

            verificar(resultado.PromptVersion == "v1.0",
                      $"Se audita la version del prompt utilizada: {resultado.PromptVersion}.");

            verificar(resultado.Respuesta!.Recomendaciones.Count == 2 && resultado.Respuesta.Alertas.Count == 1,
                      "El contrato se deserializa completo: alertas y recomendaciones.");

            // Comprobacion de lo que REALMENTE se envio.
            var peticion = handler.Peticiones[0];

            verificar(peticion.Ruta.EndsWith("/responses"),
                      $"La peticion va al endpoint de la Responses API: {peticion.Ruta}.");

            verificar(peticion.Autorizacion is not null && peticion.Autorizacion.StartsWith("Bearer "),
                      "La clave viaja en la cabecera Authorization, nunca en la URL ni en el cuerpo.");

            var cuerpo = JsonNode.Parse(peticion.Cuerpo)!;
            var formato = cuerpo["text"]?["format"];

            verificar(formato?["type"]?.GetValue<string>() == "json_schema",
                      "El cuerpo declara text.format.type = json_schema.");

            verificar(formato?["strict"]?.GetValue<bool>() == true,
                      "El esquema se envia en modo estricto (strict = true).");

            verificar(formato?["schema"]?["additionalProperties"]?.GetValue<bool>() == false,
                      "El esquema prohibe propiedades adicionales, como exige el modo estricto.");

            var requeridos = formato?["schema"]?["required"]?.AsArray();
            verificar(requeridos is not null && requeridos.Count == 5,
                      $"Los cinco campos del contrato son obligatorios en el esquema: {requeridos?.Count}.");

            verificar(!peticion.Cuerpo.Contains("Identificacion") && !peticion.Cuerpo.Contains("@"),
                      "No se envian datos personales innecesarios del cliente (identificacion ni correo).");
        }

        // ---- 2. Proveedor sin /responses: repliegue a /chat/completions.
        Console.WriteLine();
        Console.WriteLine("--- Repliegue a /chat/completions --------------------------------");

        var handlerFallback = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.NotFound, "{\"error\":{\"message\":\"Unknown endpoint\"}}")
            .Responder(HttpStatusCode.OK, RespuestaChatCompletions(RespuestaValida()));

        using (var servicio = CrearServicio(handlerFallback, registro))
        {
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(resultado.Exitoso,
                      "Si el proveedor no expone /responses, el analisis se completa igualmente.");

            verificar(handlerFallback.Peticiones.Count == 2 &&
                      handlerFallback.Peticiones[1].Ruta.EndsWith("/chat/completions"),
                      "Se reintenta automaticamente contra /chat/completions.");

            var cuerpo = JsonNode.Parse(handlerFallback.Peticiones[1].Cuerpo)!;

            verificar(cuerpo["response_format"]?["json_schema"]?["strict"]?.GetValue<bool>() == true,
                      "El mismo esquema estricto se envia en response_format.json_schema.");
        }

        // ---- 3. Limite de uso: reintento con espera y exito posterior.
        Console.WriteLine();
        Console.WriteLine("--- CA-09 : limite de uso, errores y respuestas invalidas --------");

        var handlerLimite = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.TooManyRequests, "{\"error\":{\"message\":\"rate limit\"}}")
            .Responder(HttpStatusCode.OK, RespuestaResponses(RespuestaValida()));

        using (var servicio = CrearServicio(handlerLimite, registro))
        {
            var cronometro = System.Diagnostics.Stopwatch.StartNew();
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);
            cronometro.Stop();

            verificar(resultado.Exitoso && handlerLimite.Peticiones.Count == 2,
                      $"Ante un 429 se reintenta y el analisis termina bien ({cronometro.Elapsed.TotalSeconds:0.#} s).");
        }

        // ---- 4. Clave invalida: no se reintenta y el mensaje es claro.
        var handlerClave = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.Unauthorized, "{\"error\":{\"message\":\"Invalid API key\"}}");

        using (var servicio = CrearServicio(handlerClave, registro))
        {
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(!resultado.Exitoso && resultado.Error!.Contains("no es valida"),
                      $"Clave invalida: {resultado.Error}");

            verificar(handlerClave.Peticiones.Count == 1,
                      "Un error permanente no se reintenta: insistir no lo resolveria.");
        }

        // ---- 5. Tiempo de espera agotado.
        var handlerLento = new HandlerHttpSimulado { Retardo = TimeSpan.FromSeconds(10) };
        handlerLento.Responder(HttpStatusCode.OK, RespuestaResponses(RespuestaValida()));

        using (var servicio = CrearServicio(handlerLento, registro, timeoutSegundos: 5))
        {
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(!resultado.Exitoso && resultado.Error!.Contains("no respondio"),
                      $"Tiempo de espera agotado sin colapsar la aplicacion: {resultado.Error}");
        }

        // ---- 6. Respuesta que no es JSON.
        var handlerBasura = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.OK, RespuestaResponses("lo siento, no puedo generar eso"));

        using (var servicio = CrearServicio(handlerBasura, registro))
        {
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(!resultado.Exitoso && resultado.Error!.Contains("JSON"),
                      $"Texto libre en lugar de JSON: {resultado.Error}");
        }

        // ---- 7. JSON valido pero que incumple el contrato de negocio.
        var fueraDeContrato = new JsonObject
        {
            ["nivelRiesgo"] = "MEDIO",
            ["resumen"] = new string('x', 400),          // supera los 300 caracteres
            ["alertas"] = new JsonArray(),
            ["recomendaciones"] = new JsonArray(),        // deberia tener al menos una
            ["correoSugerido"] = "Estimado cliente"
        }.ToJsonString();

        var handlerContrato = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.OK, RespuestaResponses(fueraDeContrato));

        using (var servicio = CrearServicio(handlerContrato, registro))
        {
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(!resultado.Exitoso && resultado.Error!.Contains("contrato"),
                      $"JSON bien formado pero fuera del contrato, rechazado: {resultado.Error}");
        }

        // ---- 8. El modelo se niega a responder.
        var handlerRechazo = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.OK,
                "{\"output\":[{\"content\":[{\"type\":\"refusal\",\"refusal\":\"No puedo ayudar con eso\"}]}]}");

        using (var servicio = CrearServicio(handlerRechazo, registro))
        {
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(!resultado.Exitoso && resultado.Error!.Contains("nego"),
                      $"Negativa del modelo tratada como caso previsto: {resultado.Error}");
        }

        // ---- 9. JSON envuelto en un bloque de codigo (proveedores menos estrictos).
        var handlerEnvuelto = new HandlerHttpSimulado()
            .Responder(HttpStatusCode.OK,
                RespuestaResponses("Aqui tienes el analisis:\n```json\n" + RespuestaValida() + "\n```"));

        using (var servicio = CrearServicio(handlerEnvuelto, registro))
        {
            var resultado = await servicio.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(resultado.Exitoso,
                      "Un JSON envuelto en un bloque de codigo se recupera en lugar de fallar.");
        }

        // ---- 10. Sin clave configurada.
        var sinClave = new ServicioAnalisisIAHttp(
            new OpcionesAnalisisIA { ApiKey = string.Empty }, registro,
            new HttpClient(new HandlerHttpSimulado()));

        using (sinClave)
        {
            verificar(!sinClave.EstaConfigurado, "Sin OPENAI_API_KEY el servicio se declara no configurado.");

            var resultado = await sinClave.AnalizarAsync(ConstruirSolicitud(), ct);

            verificar(!resultado.Exitoso && resultado.Error!.Contains("OPENAI_API_KEY"),
                      $"Sin clave, mensaje accionable y aplicacion operativa: {resultado.Error}");
        }
    }

    // -------------------------------------------------------------------------------- apoyo

    private static ServicioAnalisisIAHttp CrearServicio(HandlerHttpSimulado handler, IRegistroEventos registro,
                                                        int timeoutSegundos = 30) =>
        new(new OpcionesAnalisisIA
        {
            ApiKey = "clave-de-prueba-no-real",
            BaseUrl = "https://proveedor.simulado/v1",
            Modelo = "modelo-de-prueba",
            TimeoutSegundos = timeoutSegundos,
            MaximoReintentos = 1
        }, registro, new HttpClient(handler));

    private static string RespuestaValida() => new JsonObject
    {
        ["nivelRiesgo"] = "MEDIO",
        ["resumen"] = "Ocupacion alta del salon y poco margen de anticipacion para el montaje.",
        ["alertas"] = new JsonArray("La ocupacion supera el 90 por ciento de la capacidad."),
        ["recomendaciones"] = new JsonArray(
            "Confirmar el montaje con 24 horas de anticipacion.",
            "Asignar personal adicional de logistica."),
        ["correoSugerido"] = "Estimado cliente, confirmamos los detalles de su reserva."
    }.ToJsonString();

    /// <summary>Envoltura con la forma de la Responses API.</summary>
    private static string RespuestaResponses(string contenido) => new JsonObject
    {
        ["id"] = "resp_prueba",
        ["output"] = new JsonArray
        {
            new JsonObject
            {
                ["type"] = "message",
                ["content"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "output_text",
                        ["text"] = contenido
                    }
                }
            }
        },
        ["usage"] = new JsonObject
        {
            ["input_tokens"] = 512,
            ["output_tokens"] = 210
        }
    }.ToJsonString();

    /// <summary>Envoltura con la forma de Chat Completions.</summary>
    private static string RespuestaChatCompletions(string contenido) => new JsonObject
    {
        ["id"] = "chatcmpl_prueba",
        ["choices"] = new JsonArray
        {
            new JsonObject
            {
                ["index"] = 0,
                ["message"] = new JsonObject
                {
                    ["role"] = "assistant",
                    ["content"] = contenido
                }
            }
        },
        ["usage"] = new JsonObject
        {
            ["prompt_tokens"] = 512,
            ["completion_tokens"] = 210
        }
    }.ToJsonString();

    private static AnalisisIASolicitud ConstruirSolicitud() => new()
    {
        CodigoReserva = "RSV-2026-000099",
        NombreCliente = "Corporacion Andina S.A.",
        Salon = "Salon Esmeralda",
        CapacidadSalon = 120,
        FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
        HoraInicio = new TimeOnly(10, 0),
        HoraFin = new TimeOnly(15, 0),
        NumeroInvitados = 115,
        Estado = "BORRADOR",
        Subtotal = 1013.00m,
        Descuento = 0m,
        Total = 1164.95m,
        Observacion = "Evento corporativo anual",
        Recursos = new List<AnalisisIARecursoSolicitud>
        {
            new() { Nombre = "Proyector Full HD", Tipo = "EQUIPO", Cantidad = 2, StockTotal = 10, PorcentajeDescuento = 0m },
            new() { Nombre = "Servicio de coffee break", Tipo = "CATERING", Cantidad = 80, StockTotal = 300, PorcentajeDescuento = 0m }
        }
    };

    /// <summary>
    /// Reserva de prueba con caracteres peligrosos en el nombre del cliente, para comprobar
    /// que la plantilla los codifica.
    /// </summary>
    private static Reserva ConstruirReservaDePrueba() => new()
    {
        IdReserva = 99,
        Codigo = "RSV-2026-000099",
        Cliente = "Eventos <script>alert(1)</script> & Cia.",
        EmailCliente = "cliente@ejemplo.com",
        Salon = "Salon Esmeralda",
        CapacidadSalon = 120,
        TarifaBaseSalon = 450.00m,
        FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
        HoraInicio = new TimeOnly(10, 0),
        HoraFin = new TimeOnly(15, 0),
        NumeroInvitados = 80,
        Estado = EstadoReserva.Confirmada,
        Subtotal = 1013.00m,
        Descuento = 0m,
        Impuesto = 151.95m,
        Total = 1164.95m,
        Detalles = new List<ReservaDetalle>
        {
            new()
            {
                IdRecurso = 1, Recurso = "Proyector Full HD", TipoRecurso = TipoRecurso.Equipo,
                Cantidad = 2, PrecioUnitario = 35.00m, PorcentajeDescuento = 0m, StockTotal = 10
            },
            new()
            {
                IdRecurso = 7, Recurso = "Servicio de coffee break", TipoRecurso = TipoRecurso.Catering,
                Cantidad = 80, PrecioUnitario = 4.50m, PorcentajeDescuento = 0m, StockTotal = 300
            }
        }
    };

    /// <summary>Registro que descarta los mensajes; evita ruido en las pruebas de plantilla.</summary>
    private sealed class RegistroSilencioso : IRegistroEventos
    {
        public void Informacion(string mensaje) { }
        public void Advertencia(string mensaje, Exception? excepcion = null) { }
        public void Error(string mensaje, Exception? excepcion = null) { }
    }
}
