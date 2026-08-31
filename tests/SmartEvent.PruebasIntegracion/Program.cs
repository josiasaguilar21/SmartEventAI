using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Core.Exceptions;
using SmartEvent.Infrastructure.Data.Conexion;
using SmartEvent.Infrastructure.Data.Registro;
using SmartEvent.Infrastructure.Data.Repositorios;
using SmartEvent.Infrastructure.Data.Seguridad;

namespace SmartEvent.PruebasIntegracion;

/// <summary>
/// ARNES DE PRUEBAS DE INTEGRACION de la capa de acceso a datos.
///
/// NO es la aplicacion principal (la aplicacion es SmartEvent.UI, en Windows Forms). Este
/// proyecto existe para ejercitar los repositorios contra una base real y dejar evidencia
/// reproducible de que el parametro tipo tabla, los parametros de salida, la transaccion y
/// las reglas de negocio funcionan desde C#, no solo desde SQL Server Management Studio.
///
/// Ejecucion:
///     set SMARTEVENT_CONNECTION=Server=.\INSTANCIA;Database=SmartEventAI;Integrated Security=True;TrustServerCertificate=True
///     dotnet run --project tests\SmartEvent.PruebasIntegracion
///
/// Limpia todo lo que crea: puede ejecutarse tantas veces como se quiera.
/// </summary>
internal static class Program
{
    private static int _pruebasEjecutadas;
    private static int _pruebasFallidas;

    /// <summary>Fabrica compartida para que la limpieza sea accesible desde las pruebas de aplicacion.</summary>
    private static IFabricaConexiones? _fabrica;

    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var cadena = ObtenerCadenaConexion(args);

        Console.WriteLine("==============================================================");
        Console.WriteLine(" PRUEBAS DE INTEGRACION - capa de acceso a datos");
        Console.WriteLine("==============================================================");

        // Composicion manual de dependencias: exactamente lo que hara la aplicacion al arrancar.
        var registro = new RegistroEventosArchivo();
        IFabricaConexiones fabrica;

        try
        {
            fabrica = new FabricaConexiones(cadena, registro);
        }
        catch (ConfiguracionException ex)
        {
            Console.WriteLine($"FALLA | {ex.Message}");

            // En el arnes de pruebas si interesa ver el detalle tecnico para diagnosticar.
            if (ex.InnerException is not null)
            {
                Console.WriteLine($"        detalle: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
            }

            return 1;
        }

        _fabrica = fabrica;

        var hasheador = new HasheadorPbkdf2();
        var usuarios = new UsuarioRepositorio(fabrica, registro);
        var clientes = new ClienteRepositorio(fabrica, registro);
        var salones = new SalonRepositorio(fabrica, registro);
        var recursos = new RecursoRepositorio(fabrica, registro);
        var reservas = new ReservaRepositorio(fabrica, registro);
        var auditoria = new AuditoriaIntegracionesRepositorio(fabrica, registro);

        // Toda la ejecucion viaja con un token de cancelacion, igual que en la interfaz.
        using var origenCancelacion = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var ct = origenCancelacion.Token;

        Console.WriteLine($"INFO  | Conexion: {fabrica.DescripcionConexion}");
        Console.WriteLine($"INFO  | Registro: {registro.ArchivoActual}");

        int? idReservaCreada = null;

        try
        {
            if (!await fabrica.ProbarConexionAsync(ct))
            {
                Console.WriteLine("FALLA | No hay conexion con SQL Server. Verifique la cadena y que la base exista.");
                return 1;
            }

            Console.WriteLine("OK    | Conexion establecida con SQL Server.");

            var sesion = await ProbarAutenticacion(usuarios, hasheador, ct);
            if (sesion is null)
            {
                Console.WriteLine("FALLA | Sin sesion no se pueden ejecutar el resto de pruebas.");
                return 1;
            }

            var (cliente, salon, listaRecursos) = await ProbarCatalogos(clientes, salones, recursos, ct);

            idReservaCreada = await ProbarAltaConTvp(reservas, sesion, cliente, salon, listaRecursos, ct);
            await ProbarObtenerPorId(reservas, idReservaCreada.Value, ct);
            await ProbarConsultaPaginada(reservas, cliente, ct);
            await ProbarDisponibilidad(reservas, idReservaCreada.Value, salon, listaRecursos, ct);
            await ProbarRollback(reservas, sesion, cliente, salon, listaRecursos, ct);
            await ProbarCapacidad(reservas, sesion, cliente, salon, listaRecursos, ct);
            await ProbarCambioEstado(reservas, idReservaCreada.Value, sesion, ct);
            await ProbarAuditoriaIntegraciones(auditoria, idReservaCreada.Value, sesion, ct);
            await ProbarCancelacion(reservas, idReservaCreada.Value, sesion, ct);

            Console.WriteLine();
            Console.WriteLine("==============================================================");
            Console.WriteLine(" PRUEBAS DE INTEGRACION - capa de aplicacion");
            Console.WriteLine("==============================================================");

            await PruebasAplicacion.EjecutarAsync(
                usuarios, clientes, salones, recursos, reservas, auditoria,
                registro, sesion, Verificar, ct);

            Console.WriteLine();
            Console.WriteLine("==============================================================");
            Console.WriteLine(" PRUEBAS DE INTEGRACION - correo y servicio de IA");
            Console.WriteLine("==============================================================");

            await PruebasIntegraciones.EjecutarAsync(registro, Verificar, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FALLA | Excepcion no controlada: {ex.GetType().Name} - {ex.Message}");
            _pruebasFallidas++;
        }
        finally
        {
            if (idReservaCreada.HasValue)
            {
                await LimpiarReservaAsync(idReservaCreada.Value);
            }
        }

        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine($" RESULTADO: {_pruebasEjecutadas - _pruebasFallidas} de {_pruebasEjecutadas} pruebas correctas.");
        Console.WriteLine("==============================================================");

        return _pruebasFallidas == 0 ? 0 : 1;
    }

    // ------------------------------------------------------------------------------- pruebas

    /// <summary>
    /// Recorre el flujo real de autenticacion: pedir salt, derivar con PBKDF2 y enviar solo el
    /// hash. Si esta prueba pasa, significa que el hash sembrado por el script y el que calcula
    /// .NET coinciden byte a byte.
    /// </summary>
    private static async Task<SesionUsuario?> ProbarAutenticacion(
        IUsuarioRepositorio usuarios, IHasheadorContrasena hasheador, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Autenticacion -------------------------------------------------");

        var parametros = await usuarios.ObtenerParametrosHashAsync("admin", ct);
        Verificar(parametros.Salt.Length > 0 && parametros.Iteraciones >= 10000,
                  $"Parametros de derivacion recibidos: {parametros.Algoritmo}, {parametros.Iteraciones} iteraciones.");

        var hash = hasheador.Derivar("Admin123*", parametros);
        var resultado = await usuarios.AutenticarAsync("admin", hash, ct);
        Verificar(resultado.EsCorrecto, $"Autenticacion correcta de 'admin' con PBKDF2 real. {resultado.Mensaje}");

        if (resultado.Sesion is not null)
        {
            Verificar(resultado.Sesion.EsAdministrador && resultado.Sesion.PuedeEditarCatalogos,
                      $"Rol y permisos resueltos: {resultado.Sesion.TextoBarraEstado}.");
        }

        var hashMalo = hasheador.Derivar("clave-que-no-es", parametros);
        var rechazo = await usuarios.AutenticarAsync("admin", hashMalo, ct);
        Verificar(!rechazo.EsCorrecto && rechazo.Resultado == ResultadoAutenticacion.CredencialesInvalidas,
                  $"Contrasena incorrecta rechazada con mensaje generico: \"{rechazo.Mensaje}\"");

        // El usuario inexistente debe recibir parametros senuelo, no un error revelador.
        var senuelo = await usuarios.ObtenerParametrosHashAsync("no_existe_este_usuario", ct);
        Verificar(senuelo.Salt.Length > 0,
                  "Usuario inexistente recibe salt senuelo con el mismo formato (no se puede enumerar).");

        return resultado.Sesion;
    }

    private static async Task<(Cliente cliente, Salon salon, IReadOnlyList<Recurso> recursos)> ProbarCatalogos(
        IClienteRepositorio clientes, ISalonRepositorio salones, IRecursoRepositorio recursos, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Catalogos -----------------------------------------------------");

        var listaClientes = await clientes.ConsultarAsync(null, null, true, ct);
        Verificar(listaClientes.Count > 0, $"Clientes activos consultados: {listaClientes.Count}.");

        // El filtro de capacidad minima es el que usara el formulario de reserva.
        var listaSalones = await salones.ConsultarAsync(null, null, true, 100, ct);
        Verificar(listaSalones.Count > 0 && listaSalones.All(s => s.Capacidad >= 100),
                  $"Salones con capacidad >= 100: {listaSalones.Count}.");

        var listaRecursos = await recursos.ConsultarAsync(null, null, null, true, ct);
        Verificar(listaRecursos.Count >= 3, $"Recursos activos consultados: {listaRecursos.Count}.");

        var soloEquipos = await recursos.ConsultarAsync(null, null, TipoRecurso.Equipo, true, ct);
        Verificar(soloEquipos.Count > 0 && soloEquipos.All(r => r.Tipo == TipoRecurso.Equipo),
                  $"Filtro por tipo EQUIPO devuelve {soloEquipos.Count} recursos, todos del tipo correcto.");

        return (listaClientes[0], listaSalones[0], listaRecursos);
    }

    /// <summary>CA-01 desde C#: alta con tres detalles enviados en una sola llamada mediante TVP.</summary>
    private static async Task<int> ProbarAltaConTvp(IReservaRepositorio reservas, SesionUsuario sesion,
        Cliente cliente, Salon salon, IReadOnlyList<Recurso> recursos, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-01 : alta cabecera-detalle con TVP -------------------------");

        var tres = recursos.Take(3).ToList();

        var dto = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(60)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = Math.Min(40, salon.Capacidad),
            Descuento = 0m,
            Observacion = "Prueba de integracion de la capa de datos",
            Detalles = tres.Select(r => new ReservaDetalleGuardarDto
            {
                IdRecurso = r.IdRecurso,
                Cantidad = 2,
                PrecioUnitario = r.PrecioUnitario,
                PorcentajeDescuento = 0m
            }).ToList()
        };

        var resultado = await reservas.GuardarAsync(dto, sesion.IdUsuario, ct);

        Verificar(resultado.IdReserva > 0 && !string.IsNullOrWhiteSpace(resultado.Codigo),
                  $"Reserva creada: {resultado.Codigo} (Id {resultado.IdReserva}). {resultado.Mensaje}");

        return resultado.IdReserva;
    }

    /// <summary>Verifica que se leen correctamente los DOS conjuntos de resultados.</summary>
    private static async Task ProbarObtenerPorId(IReservaRepositorio reservas, int idReserva, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Lectura de cabecera y detalle ---------------------------------");

        var reserva = await reservas.ObtenerPorIdAsync(idReserva, ct);

        Verificar(reserva is not null, "Cabecera recuperada.");
        if (reserva is null) return;

        Verificar(reserva.Detalles.Count == 3, $"Detalles recuperados: {reserva.Detalles.Count} de 3.");
        Verificar(reserva.Estado == EstadoReserva.Borrador, $"Estado inicial correcto: {reserva.Estado.ATextoUsuario()}.");
        Verificar(reserva.FechaEvento != DateOnly.MinValue && reserva.HoraFin > reserva.HoraInicio,
                  $"Fecha y horario mapeados: {reserva.FechaEvento:dd/MM/yyyy} {reserva.HorarioTexto} ({reserva.Duracion.TotalHours:0.#} h).");

        // El motor es la fuente de verdad de los importes: se comprueba la coherencia aritmetica.
        var sumaLineas = reserva.Detalles.Sum(d => d.SubtotalLinea);
        var subtotalEsperado = reserva.TarifaBaseSalon + sumaLineas;
        var impuestoEsperado = Math.Round((subtotalEsperado - reserva.Descuento) * 0.15m, 2, MidpointRounding.AwayFromZero);

        Verificar(reserva.Subtotal == subtotalEsperado,
                  $"Subtotal persistido {reserva.Subtotal:N2} = tarifa {reserva.TarifaBaseSalon:N2} + lineas {sumaLineas:N2}.");
        Verificar(reserva.Impuesto == impuestoEsperado,
                  $"Impuesto del 15 por ciento correcto: {reserva.Impuesto:N2}.");
        Verificar(reserva.Total == reserva.BaseNeta + reserva.Impuesto,
                  $"Total coherente: {reserva.Total:N2}.");
    }

    private static async Task ProbarConsultaPaginada(IReservaRepositorio reservas, Cliente cliente, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Consulta con filtros combinados y paginacion ------------------");

        var pagina = await reservas.ConsultarAsync(new ReservaFiltroDto
        {
            IdCliente = cliente.IdCliente,
            FechaDesde = DateOnly.FromDateTime(DateTime.Today),
            Estado = EstadoReserva.Borrador,
            Pagina = 1,
            TamanoPagina = 10
        }, ct);

        Verificar(pagina.TotalRegistros >= 1,
                  $"Filtros combinados (cliente + fecha + estado): {pagina.TotalRegistros} coincidencia(s), pagina de {pagina.Elementos.Count}.");

        // Un filtro imposible debe devolver una pagina vacia, no una excepcion.
        var vacia = await reservas.ConsultarAsync(new ReservaFiltroDto { Codigo = "NO-EXISTE-XYZ" }, ct);
        Verificar(vacia.TotalRegistros == 0 && vacia.Elementos.Count == 0,
                  "Filtro sin coincidencias devuelve pagina vacia sin error.");
    }

    /// <summary>CA-03 y CA-04 desde C#: cruce detectado y edicion sin autoconflicto.</summary>
    private static async Task ProbarDisponibilidad(IReservaRepositorio reservas, int idReserva, Salon salon,
        IReadOnlyList<Recurso> recursos, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-03 / CA-04 : disponibilidad --------------------------------");

        var detalle = new List<ReservaDetalleGuardarDto>
        {
            new() { IdRecurso = recursos[0].IdRecurso, Cantidad = 1, PrecioUnitario = recursos[0].PrecioUnitario }
        };

        // Sin excluir la reserva: su propia franja debe aparecer como conflicto.
        var conCruce = await reservas.ValidarDisponibilidadAsync(new DisponibilidadConsultaDto
        {
            IdReserva = null,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(60)),
            HoraInicio = new TimeOnly(11, 0),
            HoraFin = new TimeOnly(15, 0),
            NumeroInvitados = 20,
            Detalles = detalle
        }, ct);

        Verificar(!conCruce.EsValido && conCruce.TieneConflictoDeTipo("CRUCE"),
                  $"Cruce parcial detectado: {conCruce.Conflictos.Count} conflicto(s). {conCruce.Conflictos.FirstOrDefault()?.Detalle}");

        // Excluyendo la propia reserva: no debe haber autoconflicto (CA-04).
        var sinAutoconflicto = await reservas.ValidarDisponibilidadAsync(new DisponibilidadConsultaDto
        {
            IdReserva = idReserva,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(60)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = 20,
            Detalles = detalle
        }, ct);

        Verificar(sinAutoconflicto.EsValido,
                  $"La reserva no se detecta a si misma al editarse: {sinAutoconflicto.Mensaje}");
    }

    /// <summary>CA-02 desde C#: un detalle invalido revierte toda la operacion.</summary>
    private static async Task ProbarRollback(IReservaRepositorio reservas, SesionUsuario sesion, Cliente cliente,
        Salon salon, IReadOnlyList<Recurso> recursos, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-02 : rollback de la transaccion ----------------------------");

        var antes = await reservas.ConsultarAsync(new ReservaFiltroDto { TamanoPagina = 1 }, ct);

        var dto = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(61)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = 20,
            Observacion = "Prueba de rollback",
            Detalles = new List<ReservaDetalleGuardarDto>
            {
                new() { IdRecurso = recursos[0].IdRecurso, Cantidad = 1, PrecioUnitario = recursos[0].PrecioUnitario },
                new() { IdRecurso = 999999, Cantidad = 1, PrecioUnitario = 10m }   // recurso inexistente
            }
        };

        try
        {
            await reservas.GuardarAsync(dto, sesion.IdUsuario, ct);
            Verificar(false, "El detalle invalido deberia haber sido rechazado.");
        }
        catch (ReglaNegocioException ex)
        {
            Verificar(true, $"Rechazado como regla de negocio (codigo {ex.CodigoError}): {ex.Message}");
        }

        var despues = await reservas.ConsultarAsync(new ReservaFiltroDto { TamanoPagina = 1 }, ct);
        Verificar(antes.TotalRegistros == despues.TotalRegistros,
                  $"Sin cabeceras huerfanas: habia {antes.TotalRegistros} reservas y siguen habiendo {despues.TotalRegistros}.");
    }

    /// <summary>CA-05 desde C#: el rechazo por capacidad llega desde SQL Server.</summary>
    private static async Task ProbarCapacidad(IReservaRepositorio reservas, SesionUsuario sesion, Cliente cliente,
        Salon salon, IReadOnlyList<Recurso> recursos, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-05 : capacidad del salon -----------------------------------");

        var dto = new ReservaGuardarDto
        {
            IdCliente = cliente.IdCliente,
            IdSalon = salon.IdSalon,
            FechaEvento = DateOnly.FromDateTime(DateTime.Today.AddDays(62)),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(13, 0),
            NumeroInvitados = salon.Capacidad + 500,
            Observacion = "Prueba de capacidad",
            Detalles = new List<ReservaDetalleGuardarDto>
            {
                new() { IdRecurso = recursos[0].IdRecurso, Cantidad = 1, PrecioUnitario = recursos[0].PrecioUnitario }
            }
        };

        try
        {
            await reservas.GuardarAsync(dto, sesion.IdUsuario, ct);
            Verificar(false, "Se acepto una reserva por encima de la capacidad del salon.");
        }
        catch (ReglaNegocioException ex)
        {
            Verificar(true, $"Rechazado desde SQL Server: {ex.Message}");
        }
    }

    private static async Task ProbarCambioEstado(IReservaRepositorio reservas, int idReserva,
        SesionUsuario sesion, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-06 : transiciones de estado --------------------------------");

        try
        {
            await reservas.CambiarEstadoAsync(new CambioEstadoDto
            {
                IdReserva = idReserva,
                EstadoNuevo = EstadoReserva.Finalizada
            }, sesion.IdUsuario, ct);

            Verificar(false, "Se permitio la transicion BORRADOR -> FINALIZADA.");
        }
        catch (ReglaNegocioException ex)
        {
            Verificar(true, $"Transicion invalida rechazada: {ex.Message}");
        }

        try
        {
            await reservas.CambiarEstadoAsync(new CambioEstadoDto
            {
                IdReserva = idReserva,
                EstadoNuevo = EstadoReserva.Confirmada
            }, sesion.IdUsuario, ct);

            Verificar(false, "Se confirmo sin analisis de IA ni contingencia.");
        }
        catch (ReglaNegocioException ex)
        {
            Verificar(true, $"Confirmacion bloqueada sin analisis de IA: {ex.Message}");
        }

        var mensaje = await reservas.CambiarEstadoAsync(new CambioEstadoDto
        {
            IdReserva = idReserva,
            EstadoNuevo = EstadoReserva.Confirmada,
            JustificacionContingencia = "Analisis de IA no disponible durante la prueba de integracion."
        }, sesion.IdUsuario, ct);

        Verificar(mensaje.Contains("CONFIRMADA"), $"Confirmacion con contingencia auditada: {mensaje}");

        var historial = await reservas.ObtenerAuditoriaAsync(idReserva, ct);
        Verificar(historial.Count >= 2,
                  $"Auditoria de transiciones: {historial.Count} movimiento(s). Ultimo: {historial[0].TransicionTexto}.");
    }

    private static async Task ProbarAuditoriaIntegraciones(IAuditoriaIntegracionesRepositorio auditoria,
        int idReserva, SesionUsuario sesion, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- CA-07 / CA-08 : auditoria de integraciones --------------------");

        // Primer intento fallido de correo, tal como ocurriria con SMTP caido.
        var fallo = new ResultadoCorreo
        {
            Enviado = false,
            Destinatario = "cliente@ejemplo.com",
            Asunto = "Confirmacion de reserva",
            FechaIntento = DateTime.Now,
            Error = "No fue posible conectar con el servidor SMTP (tiempo de espera agotado)."
        };

        var idFallo = await auditoria.RegistrarCorreoAsync(idReserva, fallo, TipoNotificacion.Confirmacion,
                                                           sesion.IdUsuario, ct);
        Verificar(idFallo > 0, $"Intento de correo con ERROR auditado (Id {idFallo}).");

        // Reintento explicito que si funciona: quedan los DOS intentos registrados.
        var exito = new ResultadoCorreo
        {
            Enviado = true,
            Destinatario = "cliente@ejemplo.com",
            Asunto = "Confirmacion de reserva",
            FechaIntento = DateTime.Now
        };

        var idExito = await auditoria.RegistrarCorreoAsync(idReserva, exito, TipoNotificacion.Reenvio,
                                                           sesion.IdUsuario, ct);
        Verificar(idExito > 0, $"Reenvio con estado ENVIADO auditado (Id {idExito}).");

        var correos = await auditoria.ConsultarCorreosAsync(new FiltroCorreoDto { IdReserva = idReserva }, ct);
        Verificar(correos.Count == 2 && correos.Any(c => c.Estado == EstadoCorreo.Error) && correos.Any(c => c.Estado == EstadoCorreo.Enviado),
                  $"Los dos intentos quedan auditados y consultables: {correos.Count} registros.");

        // Analisis de IA fallido: se audita igual que el exitoso (CA-09).
        var analisisFallido = AnalisisIAResultado.Fallo("modelo-de-prueba", "v1",
            "Tiempo de espera agotado al contactar con el proveedor del modelo.");

        var idAnalisisFallo = await auditoria.RegistrarAnalisisAsync(idReserva, analisisFallido, sesion.IdUsuario, ct);
        Verificar(idAnalisisFallo > 0, $"Analisis de IA fallido auditado (Id {idAnalisisFallo}).");

        // Analisis exitoso con el JSON estructurado del contrato.
        var respuesta = new AnalisisIARespuesta
        {
            NivelRiesgo = "MEDIO",
            Resumen = "Reserva con ocupacion moderada y margen de tiempo suficiente.",
            Alertas = new List<string> { "El evento se realiza en horario de alta demanda." },
            Recomendaciones = new List<string> { "Confirmar el montaje con 24 horas de anticipacion." },
            CorreoSugerido = "Estimado cliente, su reserva ha sido registrada correctamente."
        };

        var esValida = respuesta.Validar(out var motivo);
        Verificar(esValida, $"La respuesta estructurada cumple el contrato JSON. {motivo}");

        var analisisExitoso = new AnalisisIAResultado
        {
            Exitoso = true,
            Modelo = "modelo-de-prueba",
            PromptVersion = "v1",
            Respuesta = respuesta,
            RespuestaJson = System.Text.Json.JsonSerializer.Serialize(respuesta),
            TokensEntrada = 350,
            TokensSalida = 120
        };

        var idAnalisis = await auditoria.RegistrarAnalisisAsync(idReserva, analisisExitoso, sesion.IdUsuario, ct);
        Verificar(idAnalisis > 0, $"Analisis de IA exitoso persistido con su JSON (Id {idAnalisis}).");

        var ultimo = await auditoria.ObtenerUltimoAnalisisAsync(idReserva, ct);
        Verificar(ultimo is { Exitoso: true, NivelRiesgo: NivelRiesgo.Medio },
                  $"Ultimo analisis exitoso recuperado con nivel de riesgo {ultimo?.NivelRiesgo}.");

        var listaAnalisis = await auditoria.ConsultarAnalisisAsync(new FiltroAnalisisDto { IdReserva = idReserva }, ct);
        Verificar(listaAnalisis.Count == 2,
                  $"Auditoria de IA con exito y fallo: {listaAnalisis.Count} registros consultables.");
    }

    private static async Task ProbarCancelacion(IReservaRepositorio reservas, int idReserva,
        SesionUsuario sesion, CancellationToken ct)
    {
        Console.WriteLine();
        Console.WriteLine("--- Cancelacion y estado terminal ---------------------------------");

        try
        {
            await reservas.CambiarEstadoAsync(new CambioEstadoDto
            {
                IdReserva = idReserva,
                EstadoNuevo = EstadoReserva.Cancelada,
                Motivo = "Muy corto"
            }, sesion.IdUsuario, ct);

            Verificar(false, "Se acepto un motivo de cancelacion menor a 20 caracteres.");
        }
        catch (ReglaNegocioException ex)
        {
            Verificar(true, $"Motivo insuficiente rechazado: {ex.Message}");
        }

        var mensaje = await reservas.CambiarEstadoAsync(new CambioEstadoDto
        {
            IdReserva = idReserva,
            EstadoNuevo = EstadoReserva.Cancelada,
            Motivo = "El cliente solicito la anulacion por reprogramacion del evento corporativo."
        }, sesion.IdUsuario, ct);

        Verificar(mensaje.Contains("CANCELADA"), $"Cancelacion registrada: {mensaje}");

        var reserva = await reservas.ObtenerPorIdAsync(idReserva, ct);
        Verificar(reserva is not null && reserva.Estado.EsTerminal(),
                  $"La reserva queda en estado terminal: {reserva?.Estado.ATextoUsuario()}.");
    }

    // -------------------------------------------------------------------------------- apoyo

    private static void Verificar(bool condicion, string descripcion)
    {
        _pruebasEjecutadas++;

        if (condicion)
        {
            Console.WriteLine($"OK    | {descripcion}");
        }
        else
        {
            _pruebasFallidas++;
            Console.WriteLine($"FALLA | {descripcion}");
        }
    }

    private static string ObtenerCadenaConexion(string[] args)
    {
        // Solo se acepta como cadena de conexion un argumento que realmente lo parezca: asi los
        // modificadores que 'dotnet run' reenvia a la aplicacion no se confunden con la cadena.
        var argumento = args.FirstOrDefault(a => a.Contains('=') && !a.StartsWith('-'));

        if (!string.IsNullOrWhiteSpace(argumento))
        {
            return argumento;
        }

        var variable = Environment.GetEnvironmentVariable("SMARTEVENT_CONNECTION");

        // Valor por defecto sin secretos: autenticacion integrada de Windows contra la instancia
        // local. Cualquier otro entorno se configura con la variable SMARTEVENT_CONNECTION.
        return string.IsNullOrWhiteSpace(variable)
            ? "Server=(local);Database=SmartEventAI;Integrated Security=True;TrustServerCertificate=True"
            : variable;
    }

    /// <summary>
    /// Elimina la reserva creada por las pruebas. La eliminacion en cascada definida en el
    /// esquema retira detalles, auditoria, analisis y correos asociados.
    /// </summary>
    internal static async Task LimpiarReservaAsync(int idReserva)
    {
        if (_fabrica is null)
        {
            return;
        }

        try
        {
            await using var conexion = await _fabrica.CrearAbiertaAsync(CancellationToken.None);
            await using var comando = conexion.CreateCommand();

            comando.CommandText = "DELETE FROM evt.Reserva WHERE IdReserva = @IdReserva;";
            comando.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@IdReserva", idReserva));

            await comando.ExecuteNonQueryAsync();

            Console.WriteLine();
            Console.WriteLine($"INFO  | Limpieza: reserva de prueba {idReserva} eliminada.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AVISO | No se pudo limpiar la reserva de prueba: {ex.Message}");
        }
    }
}
