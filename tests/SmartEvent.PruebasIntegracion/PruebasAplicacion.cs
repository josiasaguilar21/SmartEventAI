using SmartEvent.Application.Calculo;
using SmartEvent.Application.Servicios;
using SmartEvent.Application.Sesion;
using SmartEvent.Application.Validacion;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Core.Exceptions;
using SmartEvent.PruebasIntegracion.Dobles;

namespace SmartEvent.PruebasIntegracion;

/// <summary>
/// Pruebas de la CAPA DE APLICACION.
///
/// A diferencia de las de la capa de datos, estas ejercitan la orquestacion: que ocurre cuando
/// el correo falla despues de confirmar, que pasa si la IA no esta configurada, y si los
/// permisos por rol se respetan. El correo y la IA se sustituyen por dobles, que es
/// precisamente lo que permite reproducir CA-07 y CA-09 sin apagar servidores reales.
/// </summary>
internal static class PruebasAplicacion
{
    public static async Task EjecutarAsync(
        IUsuarioRepositorio usuariosRepo,
        IClienteRepositorio clientesRepo,
        ISalonRepositorio salonesRepo,
        IRecursoRepositorio recursosRepo,
        IReservaRepositorio reservasRepo,
        IAuditoriaIntegracionesRepositorio auditoriaRepo,
        IRegistroEventos registro,
        SesionUsuario sesionAdministrador,
        Action<bool, string> verificar,
        CancellationToken ct)
    {
        var contexto = new ContextoSesion();
        contexto.Iniciar(sesionAdministrador);

        var correo = new CorreoSimulado();
        var ia = new AnalisisIASimulado();

        var servicioCatalogos = new ServicioCatalogos(clientesRepo, salonesRepo, recursosRepo, contexto);
        var servicioAuditoria = new ServicioAuditoria(auditoriaRepo, contexto, registro);

        var servicioReservas = new ServicioReservas(
            reservasRepo, salonesRepo, recursosRepo, auditoriaRepo,
            correo, ia, contexto, registro);

        var clientes = await servicioCatalogos.ConsultarClientesAsync(null, true, ct);
        var salones = await servicioCatalogos.ConsultarSalonesAsync(null, true, null, ct);
        var recursos = await servicioCatalogos.ConsultarRecursosAsync(null, null, true, ct);

        var cliente = clientes[0];
        var salon = salones.OrderByDescending(s => s.Capacidad).First();

        int? idReserva = null;

        try
        {
            ProbarCalculadora(salon, recursos, verificar);
            ProbarValidador(cliente, salon, recursos, sesionAdministrador, verificar);

            idReserva = await ProbarGuardado(servicioReservas, cliente, salon, recursos, verificar, ct);

            await ProbarAnalisisIA(servicioReservas, servicioAuditoria, ia, idReserva.Value, verificar, ct);
            await ProbarConfirmacionConCorreoCaido(servicioReservas, servicioAuditoria, correo, idReserva.Value, verificar, ct);
            await ProbarReenvioIdempotente(servicioReservas, servicioAuditoria, correo, reservasRepo, idReserva.Value, verificar, ct);
            await ProbarPermisosPorRol(clientesRepo, salonesRepo, recursosRepo, sesionAdministrador, verificar, ct);
            await ProbarCancelacion(servicioReservas, correo, idReserva.Value, verificar, ct);
        }
        finally
        {
            if (idReserva.HasValue)
            {
                await Program.LimpiarReservaAsync(idReserva.Value);
            }
        }
    }

    /// <summary>El calculo local debe reproducir exactamente lo que persiste SQL Server.</summary>
    private static void ProbarCalculadora(Salon salon, IReadOnlyList<Recurso> recursos, Action<bool, string> verificar)
    {
        Console.WriteLine();
        Console.WriteLine("--- Calculo de totales en el cliente ------------------------------");

        var linea = CalculadoraTotales.CalcularSubtotalLinea(4, 12.50m, 10m);
        verificar(linea == 45.00m, $"Subtotal de linea con descuento del 10 por ciento: {linea:N2} (esperado 45,00).");

        var detalles = new List<ReservaDetalleGuardarDto>
        {
            new() { IdRecurso = 1, Cantidad = 2, PrecioUnitario = 35.00m, PorcentajeDescuento = 0m },
            new() { IdRecurso = 2, Cantidad = 4, PrecioUnitario = 12.50m, PorcentajeDescuento = 10m }
        };

        var totales = CalculadoraTotales.Calcular(500.00m, detalles, 0m);

        verificar(totales.Subtotal == 615.00m, $"Subtotal = tarifa 500,00 + lineas 115,00 = {totales.Subtotal:N2}.");
        verificar(totales.Impuesto == 92.25m, $"Impuesto del 15 por ciento = {totales.Impuesto:N2}.");
        verificar(totales.Total == 707.25m, $"Total = {totales.Total:N2}.");

        var conDescuento = CalculadoraTotales.Calcular(500.00m, detalles, 115.00m);
        verificar(conDescuento.BaseNeta == 500.00m && conDescuento.Impuesto == 75.00m && conDescuento.Total == 575.00m,
                  $"Con descuento global de 115,00: base {conDescuento.BaseNeta:N2}, impuesto {conDescuento.Impuesto:N2}, total {conDescuento.Total:N2}.");

        // Un descuento excesivo se acota en la previsualizacion en lugar de mostrar negativos.
        var excesivo = CalculadoraTotales.Calcular(500.00m, detalles, 99_999m);
        verificar(excesivo.BaseNeta == 0m && excesivo.Total == 0m,
                  "Un descuento mayor que el subtotal se acota y nunca produce importes negativos.");
    }

    /// <summary>El validador debe acumular TODOS los problemas, no solo el primero.</summary>
    private static void ProbarValidador(Cliente cliente, Salon salon, IReadOnlyList<Recurso> recursos,
                                        SesionUsuario sesionAdmin, Action<bool, string> verificar)
    {
        Console.WriteLine();
        Console.WriteLine("--- Validacion de negocio en el cliente ---------------------------");

        var recursosPorId = recursos.ToDictionary(r => r.IdRecurso);
        var primero = recursos[0];

        var valida = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = 20,
            Detalles = new List<ReservaDetalleGuardarDto>
            {
                new() { IdRecurso = primero.IdRecurso, Cantidad = 1, PrecioUnitario = primero.PrecioUnitario }
            }
        };

        verificar(ValidadorReserva.Validar(valida, salon, recursosPorId, sesionAdmin).EsValido,
                  "Una reserva correcta pasa la validacion sin errores.");

        // Reserva con varios problemas simultaneos.
        var invalida = new ReservaGuardarDto
        {
            IdCliente = 0,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(10, 0),
            NumeroInvitados = salon.Capacidad + 1,
            Detalles = new List<ReservaDetalleGuardarDto>()
        };

        var resultado = ValidadorReserva.Validar(invalida, salon, recursosPorId, sesionAdmin);

        verificar(resultado.Errores.Count >= 5,
                  $"Se acumulan todos los problemas de una vez: {resultado.Errores.Count} errores detectados.");
        verificar(resultado.PrimerCampoConError == nameof(invalida.IdCliente),
                  $"El primer campo con error se identifica para devolver el foco: {resultado.PrimerCampoConError}.");

        // Recurso repetido.
        var repetido = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = 20,
            Detalles = new List<ReservaDetalleGuardarDto>
            {
                new() { IdRecurso = primero.IdRecurso, Cantidad = 1, PrecioUnitario = primero.PrecioUnitario },
                new() { IdRecurso = primero.IdRecurso, Cantidad = 2, PrecioUnitario = primero.PrecioUnitario }
            }
        };

        verificar(!ValidadorReserva.Validar(repetido, salon, recursosPorId, sesionAdmin).EsValido,
                  "Se detecta el mismo recurso repetido en dos lineas del detalle.");

        // Descuento por encima del 10 por ciento con un rol sin permiso.
        var sesionCoordinador = new SesionUsuario
        {
            IdUsuario = 999,
            NombreUsuario = "coordinador.prueba",
            NombreCompleto = "Coordinador de prueba",
            Rol = "COORDINADOR"
        };

        var conDescuento = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = 20,
            Detalles = new List<ReservaDetalleGuardarDto>
            {
                new() { IdRecurso = primero.IdRecurso, Cantidad = 1, PrecioUnitario = primero.PrecioUnitario, PorcentajeDescuento = 15m }
            }
        };

        verificar(!ValidadorReserva.Validar(conDescuento, salon, recursosPorId, sesionCoordinador).EsValido,
                  "Un COORDINADOR no puede aplicar un descuento de linea del 15 por ciento.");
        verificar(ValidadorReserva.Validar(conDescuento, salon, recursosPorId, sesionAdmin).EsValido,
                  "El mismo descuento del 15 por ciento si es valido para un ADMINISTRADOR.");

        // Validacion de correo, alineada con el CHECK de la tabla.
        verificar(ValidadorCatalogos.EsCorreoValido("eventos@corpandina.com"), "Correo valido aceptado.");
        verificar(!ValidadorCatalogos.EsCorreoValido("sin-arroba.com"), "Correo sin arroba rechazado.");
        verificar(!ValidadorCatalogos.EsCorreoValido("con espacio@dominio.com"), "Correo con espacios rechazado.");
        verificar(!ValidadorCatalogos.EsCorreoValido("usuario@dominio"), "Correo sin extension de dominio rechazado.");
    }

    private static async Task<int> ProbarGuardado(IServicioReservas servicio, Cliente cliente, Salon salon,
        IReadOnlyList<Recurso> recursos, Action<bool, string> verificar, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Servicio de reservas: alta ------------------------------------");

        var dto = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(75)),
            HoraInicio = new TimeOnly(14, 0),
            HoraFin = new TimeOnly(19, 0),
            NumeroInvitados = Math.Min(30, salon.Capacidad),
            Observacion = "Prueba de la capa de aplicacion",
            Detalles = recursos.Take(2).Select(r => new ReservaDetalleGuardarDto
            {
                IdRecurso = r.IdRecurso,
                Cantidad = 2,
                PrecioUnitario = r.PrecioUnitario
            }).ToList()
        };

        var validacion = await servicio.ValidarAsync(dto, ct);
        verificar(validacion.EsValido, "El servicio valida la reserva cargando salon y recursos desde la base.");

        var disponibilidad = await servicio.ComprobarDisponibilidadAsync(dto, ct);
        verificar(disponibilidad.EsValido, $"Comprobacion anticipada de disponibilidad: {disponibilidad.Mensaje}");

        var resultado = await servicio.GuardarAsync(dto, ct);
        verificar(resultado.IdReserva > 0, $"Reserva creada a traves del servicio: {resultado.Codigo}.");

        // Una reserva invalida debe rechazarse en la capa de aplicacion, sin llegar a la base.
        var sinDetalles = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(76)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = 10,
            Detalles = new List<ReservaDetalleGuardarDto>()
        };

        try
        {
            await servicio.GuardarAsync(sinDetalles, ct);
            verificar(false, "Se guardo una reserva sin detalles.");
        }
        catch (ReglaNegocioException ex)
        {
            verificar(true, $"Reserva sin detalles rechazada antes de ir a la base: {ex.Message}");
        }

        return resultado.IdReserva;
    }

    /// <summary>CA-08 y CA-09: analisis exitoso y analisis con el servicio no configurado.</summary>
    private static async Task ProbarAnalisisIA(IServicioReservas servicio, IServicioAuditoria auditoria,
        AnalisisIASimulado ia, int idReserva, Action<bool, string> verificar, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-09 : IA no configurada ------------------------------------");

        ia.EstaConfigurado = false;

        var sinClave = await servicio.AnalizarConIAAsync(idReserva, ct);

        verificar(!sinClave.Exitoso && sinClave.Error is not null,
                  $"Sin clave configurada la aplicacion sigue operativa: {sinClave.Error}");
        verificar(sinClave.IdAnalisis > 0, $"El intento fallido queda auditado (Id {sinClave.IdAnalisis}).");
        verificar(ia.Llamadas == 0, "No se intento ninguna llamada al proveedor sin configuracion.");

        Console.WriteLine();
        Console.WriteLine("--- CA-09 : fallo del proveedor ----------------------------------");

        ia.EstaConfigurado = true;
        ia.DebeFallar = true;

        var conFallo = await servicio.AnalizarConIAAsync(idReserva, ct);
        verificar(!conFallo.Exitoso && conFallo.IdAnalisis > 0,
                  $"Fallo del proveedor auditado sin interrumpir la aplicacion: {conFallo.Error}");

        Console.WriteLine();
        Console.WriteLine("--- CA-08 : analisis con salida estructurada ---------------------");

        ia.DebeFallar = false;

        var exitoso = await servicio.AnalizarConIAAsync(idReserva, ct);

        verificar(exitoso.Exitoso && exitoso.Respuesta is not null,
                  $"Analisis completado con nivel de riesgo {exitoso.Respuesta?.NivelRiesgo}.");
        verificar(exitoso.Respuesta is not null && exitoso.Respuesta.Validar(out _),
                  "La respuesta cumple el contrato: resumen, alertas, recomendaciones y correo sugerido.");
        verificar(exitoso.IdAnalisis > 0, $"Analisis exitoso persistido (Id {exitoso.IdAnalisis}).");

        // El nivel de riesgo debe persistirse tal como vino, no con el valor por defecto.
        var ultimo = await auditoria.ObtenerUltimoAnalisisAsync(idReserva, ct);
        verificar(ultimo is not null && ultimo.NivelRiesgo is not null &&
                  ultimo.NivelRiesgo.Value.ASql() == exitoso.Respuesta!.NivelRiesgo,
                  $"El nivel de riesgo persistido coincide con el devuelto por el modelo: {ultimo?.NivelRiesgo}.");

        var interpretado = auditoria.InterpretarRespuesta(ultimo?.RespuestaJson);
        verificar(interpretado is not null && interpretado.Recomendaciones.Count > 0,
                  $"El JSON auditado se reinterpreta correctamente: {interpretado?.Recomendaciones.Count} recomendaciones.");

        var historial = await auditoria.ConsultarAnalisisAsync(new FiltroAnalisisDto { IdReserva = idReserva }, ct);
        verificar(historial.Count == 3,
                  $"Los tres intentos (sin clave, fallo y exito) quedan auditados: {historial.Count} registros.");
    }

    /// <summary>CA-06 y CA-07: la reserva se confirma aunque el correo falle.</summary>
    private static async Task ProbarConfirmacionConCorreoCaido(IServicioReservas servicio, IServicioAuditoria auditoria,
        CorreoSimulado correo, int idReserva, Action<bool, string> verificar, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-06 / CA-07 : confirmacion con SMTP caido ------------------");

        correo.DebeFallar = true;

        // Se confirma SIN justificacion de contingencia: ahora esta permitido porque la reserva
        // ya tiene un analisis de IA exitoso registrado.
        var resultado = await servicio.ConfirmarAsync(idReserva, justificacionContingencia: null, ct);

        verificar(resultado.EstadoCambiado && resultado.EstadoNuevo == EstadoReserva.Confirmada,
                  $"La reserva se confirmo: {resultado.Mensaje}");
        verificar(resultado.Correo is { Enviado: false },
                  $"El correo fallo: {resultado.Correo?.Error}");
        verificar(resultado.RequiereReintentoCorreo,
                  "El resultado indica que hace falta reintentar el envio, sin revertir la confirmacion.");

        var correos = await auditoria.ConsultarCorreosAsync(new FiltroCorreoDto { IdReserva = idReserva }, ct);
        verificar(correos.Count == 1 && correos[0].Estado == EstadoCorreo.Error,
                  $"El intento fallido queda auditado: {correos.Count} registro con estado {correos[0].Estado}.");
    }

    /// <summary>CA-07: el reenvio no duplica la reserva ni el cambio de estado.</summary>
    private static async Task ProbarReenvioIdempotente(IServicioReservas servicio, IServicioAuditoria auditoria,
        CorreoSimulado correo, IReservaRepositorio reservasRepo, int idReserva,
        Action<bool, string> verificar, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-07 : reenvio explicito y auditable ------------------------");

        correo.DebeFallar = false;

        var reenvio = await servicio.ReenviarNotificacionAsync(idReserva, ct);
        verificar(reenvio.Enviado, $"Reenvio correcto a {reenvio.Destinatario}.");

        var correos = await auditoria.ConsultarCorreosAsync(new FiltroCorreoDto { IdReserva = idReserva }, ct);
        verificar(correos.Count == 2,
                  $"Quedan auditados los DOS intentos (fallo y reenvio): {correos.Count} registros.");

        var reserva = await servicio.ObtenerAsync(idReserva, ct);
        verificar(reserva is not null && reserva.Estado == EstadoReserva.Confirmada,
                  $"La reserva sigue CONFIRMADA una sola vez: {reserva?.Estado.ATextoUsuario()}.");

        var historial = await reservasRepo.ObtenerAuditoriaAsync(idReserva, ct);
        var confirmaciones = historial.Count(h => h.EstadoNuevo == EstadoReserva.Confirmada);
        verificar(confirmaciones == 1,
                  $"El reenvio no genero una segunda transicion: {confirmaciones} confirmacion registrada.");

        // Confirmar de nuevo debe rechazarse: la reserva ya esta en ese estado.
        try
        {
            await servicio.ConfirmarAsync(idReserva, null, ct);
            verificar(false, "Se permitio confirmar dos veces la misma reserva.");
        }
        catch (ReglaNegocioException ex)
        {
            verificar(true, $"Segunda confirmacion rechazada: {ex.Message}");
        }
    }

    private static async Task ProbarPermisosPorRol(IClienteRepositorio clientesRepo, ISalonRepositorio salonesRepo,
        IRecursoRepositorio recursosRepo, SesionUsuario sesionAdmin, Action<bool, string> verificar,
        CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Permisos por rol ---------------------------------------------");

        var contextoCoordinador = new ContextoSesion();
        contextoCoordinador.Iniciar(new SesionUsuario
        {
            IdUsuario = sesionAdmin.IdUsuario,
            NombreUsuario = "coordinador",
            NombreCompleto = "Coordinador de Eventos",
            Rol = "COORDINADOR"
        });

        var servicio = new ServicioCatalogos(clientesRepo, salonesRepo, recursosRepo, contextoCoordinador);

        var lista = await servicio.ConsultarClientesAsync(null, true, ct);
        verificar(lista.Count > 0, $"El COORDINADOR si puede consultar catalogos: {lista.Count} clientes.");

        try
        {
            await servicio.GuardarClienteAsync(new Cliente
            {
                Identificacion = "9999999999",
                Nombres = "Cliente no autorizado",
                Email = "prueba@ejemplo.com"
            }, ct);

            verificar(false, "Un COORDINADOR pudo modificar el catalogo de clientes.");
        }
        catch (ReglaNegocioException ex)
        {
            verificar(true, $"Modificacion bloqueada por rol: {ex.Message}");
        }

        // Sin sesion activa, ninguna operacion de negocio debe proceder.
        var contextoVacio = new ContextoSesion();
        var servicioSinSesion = new ServicioCatalogos(clientesRepo, salonesRepo, recursosRepo, contextoVacio);

        try
        {
            await servicioSinSesion.CambiarEstadoClienteAsync(1, false, ct);
            verificar(false, "Se opero sin sesion activa.");
        }
        catch (ReglaNegocioException ex)
        {
            verificar(true, $"Operacion sin sesion rechazada: {ex.Message}");
        }
    }

    private static async Task ProbarCancelacion(IServicioReservas servicio, CorreoSimulado correo, int idReserva,
        Action<bool, string> verificar, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Cancelacion con notificacion ---------------------------------");

        var intentosPrevios = correo.IntentosDeEnvio;

        try
        {
            await servicio.CancelarAsync(idReserva, "Muy corto", ct);
            verificar(false, "Se acepto un motivo de cancelacion demasiado breve.");
        }
        catch (ReglaNegocioException ex)
        {
            verificar(true, $"Motivo insuficiente rechazado en la capa de aplicacion: {ex.Message}");
        }

        verificar(correo.IntentosDeEnvio == intentosPrevios,
                  "Un cambio de estado rechazado no genera ningun intento de correo.");

        var resultado = await servicio.CancelarAsync(idReserva,
            "El cliente reprogramo el evento para el proximo trimestre por motivos presupuestarios.", ct);

        verificar(resultado.EstadoCambiado && resultado.EstadoNuevo == EstadoReserva.Cancelada,
                  $"Cancelacion aplicada: {resultado.Mensaje}");
        verificar(resultado.Correo is { Enviado: true },
                  $"Se notifico la cancelacion a {resultado.Correo?.Destinatario}.");
        verificar(correo.UltimoMensaje is not null && correo.UltimoMensaje.Tipo == TipoNotificacion.Cancelacion,
                  "El contenido compuesto corresponde a una cancelacion.");
    }
}
