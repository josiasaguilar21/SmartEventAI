using SmartEvent.Application.Sesion;
using SmartEvent.Application.Validacion;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.Application.Servicios;

public interface IServicioReservas
{
    Task<Reserva?> ObtenerAsync(int idReserva, CancellationToken cancelacion);
    Task<PaginaResultado<ReservaResumenDto>> ConsultarAsync(ReservaFiltroDto filtro, CancellationToken cancelacion);
    Task<IReadOnlyList<ReservaAuditoria>> ObtenerHistorialAsync(int idReserva, CancellationToken cancelacion);

    /// <summary>Valida en el cliente, cargando el salon y los recursos implicados.</summary>
    Task<ResultadoValidacion> ValidarAsync(ReservaGuardarDto reserva, CancellationToken cancelacion);

    /// <summary>Comprobacion anticipada de cruce de horario, capacidad y stock concurrente.</summary>
    Task<DisponibilidadResultado> ComprobarDisponibilidadAsync(ReservaGuardarDto reserva, CancellationToken cancelacion);

    Task<ReservaGuardarResultado> GuardarAsync(ReservaGuardarDto reserva, CancellationToken cancelacion);

    Task<CambioEstadoResultado> ConfirmarAsync(int idReserva, string? justificacionContingencia, CancellationToken cancelacion);
    Task<CambioEstadoResultado> CancelarAsync(int idReserva, string motivo, CancellationToken cancelacion);
    Task<CambioEstadoResultado> FinalizarAsync(int idReserva, CancellationToken cancelacion);

    /// <summary>Reintento explicito del correo. No modifica el estado de la reserva (CA-07).</summary>
    Task<ResultadoCorreo> ReenviarNotificacionAsync(int idReserva, CancellationToken cancelacion);

    /// <summary>Ejecuta el analisis con IA y persiste su auditoria, haya funcionado o no.</summary>
    Task<AnalisisIAResultado> AnalizarConIAAsync(int idReserva, CancellationToken cancelacion);
}

/// <summary>
/// Servicio central del negocio. Coordina base de datos, correo e inteligencia artificial.
///
/// DOS PRINCIPIOS QUE EXPLICAN TODO EL DISENO DE ESTA CLASE:
///
/// 1. La transaccion de la reserva vive en SQL Server. Este servicio nunca abre transacciones
///    ni intenta "deshacer" nada a mano: envia la operacion completa y el motor la confirma o
///    la revierte entera.
///
/// 2. El correo y la IA son EFECTOS SECUNDARIOS y no pueden alterar el resultado del negocio.
///    Si el correo falla, la reserva sigue confirmada; se audita el fallo y se ofrece reenviar.
///    Si la IA falla, la reserva sigue siendo editable; se audita y el usuario decide.
///    Ninguno de los dos lanza excepciones hacia arriba por un fallo del servicio externo.
/// </summary>
public sealed class ServicioReservas : IServicioReservas
{
    private readonly IReservaRepositorio _reservas;
    private readonly ISalonRepositorio _salones;
    private readonly IRecursoRepositorio _recursos;
    private readonly IAuditoriaIntegracionesRepositorio _auditoria;
    private readonly IServicioCorreo _correo;
    private readonly IServicioAnalisisIA _analisisIA;
    private readonly IContextoSesion _contexto;
    private readonly IRegistroEventos _registro;

    public ServicioReservas(
        IReservaRepositorio reservas,
        ISalonRepositorio salones,
        IRecursoRepositorio recursos,
        IAuditoriaIntegracionesRepositorio auditoria,
        IServicioCorreo correo,
        IServicioAnalisisIA analisisIA,
        IContextoSesion contexto,
        IRegistroEventos registro)
    {
        _reservas = reservas;
        _salones = salones;
        _recursos = recursos;
        _auditoria = auditoria;
        _correo = correo;
        _analisisIA = analisisIA;
        _contexto = contexto;
        _registro = registro;
    }

    // ------------------------------------------------------------------------------ consulta

    public Task<Reserva?> ObtenerAsync(int idReserva, CancellationToken cancelacion) =>
        _reservas.ObtenerPorIdAsync(idReserva, cancelacion);

    public Task<PaginaResultado<ReservaResumenDto>> ConsultarAsync(ReservaFiltroDto filtro,
                                                                   CancellationToken cancelacion)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        // Se acotan los valores de paginacion aqui para que la interfaz no pueda pedir una
        // pagina absurda por un error de calculo y arrastrar al servidor con ella.
        if (filtro.Pagina < 1) filtro.Pagina = 1;
        if (filtro.TamanoPagina is < 1 or > 500) filtro.TamanoPagina = 50;

        // Rango de fechas invertido: se corrige en lugar de devolver una lista vacia sin explicacion.
        if (filtro.FechaDesde.HasValue && filtro.FechaHasta.HasValue && filtro.FechaDesde > filtro.FechaHasta)
        {
            (filtro.FechaDesde, filtro.FechaHasta) = (filtro.FechaHasta, filtro.FechaDesde);
        }

        return _reservas.ConsultarAsync(filtro, cancelacion);
    }

    public Task<IReadOnlyList<ReservaAuditoria>> ObtenerHistorialAsync(int idReserva, CancellationToken cancelacion) =>
        _reservas.ObtenerAuditoriaAsync(idReserva, cancelacion);

    // -------------------------------------------------------------------- validacion y alta

    public async Task<ResultadoValidacion> ValidarAsync(ReservaGuardarDto reserva, CancellationToken cancelacion)
    {
        ArgumentNullException.ThrowIfNull(reserva);

        var sesion = _contexto.Requerida;

        var salon = reserva.IdSalon > 0
            ? await _salones.ObtenerPorIdAsync(reserva.IdSalon, cancelacion).ConfigureAwait(false)
            : null;

        var recursosPorId = await ObtenerRecursosImplicadosAsync(reserva, cancelacion).ConfigureAwait(false);

        return ValidadorReserva.Validar(reserva, salon, recursosPorId, sesion);
    }

    public Task<DisponibilidadResultado> ComprobarDisponibilidadAsync(ReservaGuardarDto reserva,
                                                                      CancellationToken cancelacion)
    {
        ArgumentNullException.ThrowIfNull(reserva);

        return _reservas.ValidarDisponibilidadAsync(new DisponibilidadConsultaDto
        {
            IdReserva = reserva.IdReserva,
            IdSalon = reserva.IdSalon,
            FechaEvento = reserva.FechaEvento,
            HoraInicio = reserva.HoraInicio,
            HoraFin = reserva.HoraFin,
            NumeroInvitados = reserva.NumeroInvitados,
            Detalles = reserva.Detalles
        }, cancelacion);
    }

    public async Task<ReservaGuardarResultado> GuardarAsync(ReservaGuardarDto reserva, CancellationToken cancelacion)
    {
        ArgumentNullException.ThrowIfNull(reserva);

        var sesion = _contexto.Requerida;

        // Se revalida aunque la pantalla ya lo haya hecho: el servicio es el punto por el que
        // pasan obligatoriamente todas las altas, incluida cualquier futura automatizacion.
        var validacion = await ValidarAsync(reserva, cancelacion).ConfigureAwait(false);

        if (!validacion.EsValido)
        {
            throw new ReglaNegocioException(validacion.MensajeCompleto());
        }

        // Una reserva que ya no esta en BORRADOR no admite edicion. Se comprueba antes para dar
        // un mensaje inmediato; el bloqueo real lo impone evt.sp_Reserva_Guardar.
        if (reserva.IdReserva.HasValue)
        {
            var existente = await _reservas.ObtenerPorIdAsync(reserva.IdReserva.Value, cancelacion)
                                           .ConfigureAwait(false);

            if (existente is null)
            {
                throw new ReglaNegocioException("La reserva que intenta editar ya no existe.");
            }

            if (!existente.Estado.PermiteEdicion())
            {
                throw new ReglaNegocioException(
                    $"La reserva {existente.Codigo} esta en estado {existente.Estado.ATextoUsuario()} " +
                    "y ya no puede modificarse. Solo puede cancelarse o finalizarse.");
            }
        }

        return await _reservas.GuardarAsync(reserva, sesion.IdUsuario, cancelacion).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------ cambios de estado

    /// <summary>
    /// Confirma la reserva y notifica al cliente.
    ///
    /// El orden importa: PRIMERO se cambia el estado (operacion transaccional y verificada por
    /// el motor) y solo DESPUES se intenta el correo. Si el correo falla, la confirmacion sigue
    /// siendo valida y el resultado indica que hace falta reintentar el envio.
    /// </summary>
    public Task<CambioEstadoResultado> ConfirmarAsync(int idReserva, string? justificacionContingencia,
                                                      CancellationToken cancelacion) =>
        CambiarEstadoAsync(new CambioEstadoDto
        {
            IdReserva = idReserva,
            EstadoNuevo = EstadoReserva.Confirmada,
            JustificacionContingencia = string.IsNullOrWhiteSpace(justificacionContingencia)
                ? null
                : justificacionContingencia.Trim()
        }, TipoNotificacion.Confirmacion, cancelacion);

    public Task<CambioEstadoResultado> CancelarAsync(int idReserva, string motivo, CancellationToken cancelacion)
    {
        motivo = motivo?.Trim() ?? string.Empty;

        // Se comprueba aqui para poder devolver el foco al cuadro de texto con un mensaje util,
        // en lugar de esperar el rechazo del procedimiento almacenado.
        if (motivo.Length < 20)
        {
            throw new ReglaNegocioException(
                "Indique el motivo de la cancelacion con al menos 20 caracteres. " +
                $"Ha escrito {motivo.Length}.");
        }

        return CambiarEstadoAsync(new CambioEstadoDto
        {
            IdReserva = idReserva,
            EstadoNuevo = EstadoReserva.Cancelada,
            Motivo = motivo
        }, TipoNotificacion.Cancelacion, cancelacion);
    }

    /// <summary>Cierre operativo del evento. No genera notificacion al cliente.</summary>
    public Task<CambioEstadoResultado> FinalizarAsync(int idReserva, CancellationToken cancelacion) =>
        CambiarEstadoAsync(new CambioEstadoDto
        {
            IdReserva = idReserva,
            EstadoNuevo = EstadoReserva.Finalizada
        }, tipoNotificacion: null, cancelacion);

    private async Task<CambioEstadoResultado> CambiarEstadoAsync(CambioEstadoDto cambio,
                                                                 TipoNotificacion? tipoNotificacion,
                                                                 CancellationToken cancelacion)
    {
        var sesion = _contexto.Requerida;

        var reservaPrevia = await _reservas.ObtenerPorIdAsync(cambio.IdReserva, cancelacion).ConfigureAwait(false)
            ?? throw new ReglaNegocioException("La reserva indicada ya no existe.");

        var estadoAnterior = reservaPrevia.Estado;

        // PASO 1: el cambio de estado. Si el motor lo rechaza (transicion invalida, falta de
        // analisis de IA, disponibilidad perdida), la excepcion sube y no se envia ningun correo.
        var mensaje = await _reservas.CambiarEstadoAsync(cambio, sesion.IdUsuario, cancelacion)
                                     .ConfigureAwait(false);

        // PASO 2: la notificacion. A partir de aqui, pase lo que pase, el estado ya cambio.
        ResultadoCorreo? resultadoCorreo = null;

        if (tipoNotificacion.HasValue)
        {
            var reservaActualizada = await _reservas.ObtenerPorIdAsync(cambio.IdReserva, cancelacion)
                                                    .ConfigureAwait(false) ?? reservaPrevia;

            resultadoCorreo = await NotificarAsync(reservaActualizada, tipoNotificacion.Value, cancelacion)
                                    .ConfigureAwait(false);
        }

        return new CambioEstadoResultado
        {
            EstadoCambiado = true,
            Mensaje = mensaje,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = cambio.EstadoNuevo,
            Correo = resultadoCorreo
        };
    }

    // ------------------------------------------------------------------------------- correo

    /// <summary>
    /// Reintento explicito y auditable (CA-07).
    ///
    /// No toca el estado de la reserva: por eso reintentar es idempotente y no puede provocar
    /// una segunda transicion ni duplicar la reserva. Cada intento deja su propio registro en
    /// com.CorreoEnviado, de modo que el historial muestra el fallo y el reenvio posterior.
    /// </summary>
    public async Task<ResultadoCorreo> ReenviarNotificacionAsync(int idReserva, CancellationToken cancelacion)
    {
        _ = _contexto.Requerida;

        var reserva = await _reservas.ObtenerPorIdAsync(idReserva, cancelacion).ConfigureAwait(false)
            ?? throw new ReglaNegocioException("La reserva indicada ya no existe.");

        if (reserva.Estado is not (EstadoReserva.Confirmada or EstadoReserva.Cancelada))
        {
            throw new ReglaNegocioException(
                $"Solo se notifica al cliente cuando la reserva esta confirmada o cancelada. " +
                $"La reserva {reserva.Codigo} esta en estado {reserva.Estado.ATextoUsuario()}.");
        }

        return await NotificarAsync(reserva, TipoNotificacion.Reenvio, cancelacion).ConfigureAwait(false);
    }

    /// <summary>
    /// Compone, envia y AUDITA el correo. Nunca propaga una excepcion del servidor de correo:
    /// devuelve el fallo como dato para que quien llama decida como mostrarlo.
    /// </summary>
    private async Task<ResultadoCorreo> NotificarAsync(Reserva reserva, TipoNotificacion tipo,
                                                       CancellationToken cancelacion)
    {
        // El tipo REENVIO no describe el contenido, solo el intento: el cuerpo se compone segun
        // el estado real en el que se encuentra la reserva.
        var tipoContenido = tipo == TipoNotificacion.Reenvio
            ? (reserva.Estado == EstadoReserva.Cancelada ? TipoNotificacion.Cancelacion : TipoNotificacion.Confirmacion)
            : tipo;

        ResultadoCorreo resultado;

        try
        {
            if (!_correo.EstaConfigurado)
            {
                resultado = new ResultadoCorreo
                {
                    Enviado = false,
                    Destinatario = reserva.EmailCliente,
                    Asunto = $"Reserva {reserva.Codigo}",
                    FechaIntento = DateTime.Now,
                    Error = "El servicio de correo no esta configurado en este equipo. " +
                            "Revise las variables SMARTEVENT_SMTP_* descritas en el README."
                };
            }
            else
            {
                var mensaje = _correo.Componer(reserva, tipoContenido);
                resultado = await _correo.EnviarAsync(mensaje, cancelacion).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Red de seguridad: ni un fallo inesperado al componer o enviar puede tumbar una
            // confirmacion que el motor ya dio por buena.
            _registro.Error($"Fallo inesperado al notificar la reserva {reserva.Codigo}.", ex);

            resultado = new ResultadoCorreo
            {
                Enviado = false,
                Destinatario = reserva.EmailCliente,
                Asunto = $"Reserva {reserva.Codigo}",
                FechaIntento = DateTime.Now,
                Error = "No fue posible generar o enviar la notificacion. El detalle quedo registrado en el log."
            };
        }

        await AuditarCorreoAsync(reserva.IdReserva, resultado, tipo, cancelacion).ConfigureAwait(false);

        return resultado;
    }

    private async Task AuditarCorreoAsync(int idReserva, ResultadoCorreo resultado, TipoNotificacion tipo,
                                          CancellationToken cancelacion)
    {
        try
        {
            resultado.IdCorreo = await _auditoria
                .RegistrarCorreoAsync(idReserva, resultado, tipo, _contexto.Actual?.IdUsuario, cancelacion)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Perder la auditoria es grave, pero menos que interrumpir al usuario despues de una
            // operacion ya confirmada. Se registra en el log local para no perder el rastro.
            _registro.Error($"No se pudo registrar la auditoria del correo de la reserva {idReserva}.", ex);
        }
    }

    // ----------------------------------------------------------------------------------- IA

    /// <summary>
    /// Ejecuta el analisis con IA sobre una reserva y persiste SIEMPRE el resultado en
    /// evt.AnalisisIA, tanto si funciono como si no (CA-08 y CA-09).
    ///
    /// La IA solo asesora: este metodo no cambia el estado de la reserva, no toca los importes
    /// y no envia ningun correo. Lo unico que produce es informacion para el usuario.
    /// </summary>
    public async Task<AnalisisIAResultado> AnalizarConIAAsync(int idReserva, CancellationToken cancelacion)
    {
        _ = _contexto.Requerida;

        var reserva = await _reservas.ObtenerPorIdAsync(idReserva, cancelacion).ConfigureAwait(false)
            ?? throw new ReglaNegocioException("La reserva indicada ya no existe.");

        if (reserva.Detalles.Count == 0)
        {
            throw new ReglaNegocioException(
                "Agregue al menos un recurso a la reserva antes de solicitar el analisis con IA.");
        }

        AnalisisIAResultado resultado;

        if (!_analisisIA.EstaConfigurado)
        {
            // Falta de clave: es un caso esperado, no un error de programa. Se audita igual.
            resultado = AnalisisIAResultado.Fallo(
                _analisisIA.Modelo,
                "n/d",
                "El servicio de analisis con IA no esta configurado en este equipo. " +
                "Defina la variable de entorno OPENAI_API_KEY o la configuracion local descrita en el README.");
        }
        else
        {
            try
            {
                resultado = await _analisisIA.AnalizarAsync(ConstruirSolicitud(reserva), cancelacion)
                                             .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // La cancelacion la pidio el usuario con el boton Cancelar: no se audita como fallo.
                throw;
            }
            catch (Exception ex)
            {
                _registro.Error($"Fallo inesperado en el analisis con IA de la reserva {reserva.Codigo}.", ex);

                resultado = AnalisisIAResultado.Fallo(
                    _analisisIA.Modelo,
                    "n/d",
                    "No fue posible completar el analisis. El detalle tecnico quedo registrado en el log local.");
            }
        }

        try
        {
            resultado.IdAnalisis = await _auditoria
                .RegistrarAnalisisAsync(idReserva, resultado, _contexto.Actual?.IdUsuario, cancelacion)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _registro.Error($"No se pudo registrar la auditoria del analisis de la reserva {idReserva}.", ex);
        }

        return resultado;
    }

    /// <summary>
    /// Proyeccion MINIMA que se envia al modelo.
    ///
    /// Se incluye lo necesario para valorar el riesgo operativo (ocupacion, duracion, holgura
    /// de stock, anticipacion, importes) y se excluyen deliberadamente la identificacion, el
    /// correo y el telefono del cliente: no aportan nada al analisis y no hay razon para
    /// enviarlos a un servicio externo.
    /// </summary>
    private static AnalisisIASolicitud ConstruirSolicitud(Reserva reserva) => new()
    {
        CodigoReserva = reserva.Codigo,
        NombreCliente = reserva.Cliente,
        Salon = reserva.Salon,
        CapacidadSalon = reserva.CapacidadSalon,
        FechaEvento = reserva.FechaEvento,
        HoraInicio = reserva.HoraInicio,
        HoraFin = reserva.HoraFin,
        NumeroInvitados = reserva.NumeroInvitados,
        Estado = reserva.Estado.ASql(),
        Subtotal = reserva.Subtotal,
        Descuento = reserva.Descuento,
        Total = reserva.Total,
        Observacion = reserva.Observacion,
        Recursos = reserva.Detalles.Select(d => new AnalisisIARecursoSolicitud
        {
            Nombre = d.Recurso,
            Tipo = d.TipoRecurso.ASql(),
            Cantidad = d.Cantidad,
            StockTotal = d.StockTotal,
            PorcentajeDescuento = d.PorcentajeDescuento
        }).ToList()
    };

    // --------------------------------------------------------------------------------- apoyo

    /// <summary>
    /// Carga en una sola consulta los recursos que intervienen en el detalle, indexados por
    /// identificador, para que el validador no tenga que ir a la base linea por linea.
    /// </summary>
    private async Task<IReadOnlyDictionary<int, Recurso>> ObtenerRecursosImplicadosAsync(
        ReservaGuardarDto reserva, CancellationToken cancelacion)
    {
        if (reserva.Detalles.Count == 0)
        {
            return new Dictionary<int, Recurso>();
        }

        var necesarios = reserva.Detalles.Select(d => d.IdRecurso).ToHashSet();

        var todos = await _recursos.ConsultarAsync(null, null, null, null, cancelacion).ConfigureAwait(false);

        return todos.Where(r => necesarios.Contains(r.IdRecurso))
                    .ToDictionary(r => r.IdRecurso);
    }
}
