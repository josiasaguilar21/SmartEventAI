using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Infrastructure.Data.Comun;
using SmartEvent.Infrastructure.Data.Conexion;

namespace SmartEvent.Infrastructure.Data.Repositorios;

/// <summary>
/// Repositorio de reservas: la pieza donde se concreta el requisito cabecera-detalle.
///
/// Ninguna operacion de esta clase abre una transaccion desde el cliente. La transaccion vive
/// dentro de evt.sp_Reserva_Guardar, que es quien puede garantizar atomicidad real incluso si
/// la aplicacion se cierra a mitad de la llamada. Desde aqui solo se envia el paquete completo
/// (cabecera + TVP con todo el detalle) y se recoge el resultado.
/// </summary>
public sealed class ReservaRepositorio : RepositorioBase, IReservaRepositorio
{
    public ReservaRepositorio(IFabricaConexiones fabrica, IRegistroEventos registro)
        : base(fabrica, registro) { }

    /// <summary>
    /// Ejecuta evt.sp_Reserva_Guardar. Observese que NO se envian Subtotal, Impuesto ni Total:
    /// los calcula y persiste el motor, que es la fuente de verdad de los importes.
    /// </summary>
    public Task<ReservaGuardarResultado> GuardarAsync(ReservaGuardarDto reserva, int idUsuario,
                                                      CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Reserva_Guardar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", reserva.IdReserva);
            comando.AgregarEntero("@IdCliente", reserva.IdCliente);
            comando.AgregarEntero("@IdSalon", reserva.IdSalon);
            comando.AgregarFecha("@FechaEvento", reserva.FechaEvento);
            comando.AgregarHora("@HoraInicio", reserva.HoraInicio);
            comando.AgregarHora("@HoraFin", reserva.HoraFin);
            comando.AgregarEntero("@NumeroInvitados", reserva.NumeroInvitados);
            comando.AgregarDecimal("@Descuento", reserva.Descuento);
            comando.AgregarTexto("@Observacion", reserva.Observacion, 500);
            comando.AgregarEntero("@IdUsuario", idUsuario);

            // Todo el detalle en una sola llamada.
            comando.AgregarTabla("@Detalle", TablaDetalleReserva.Construir(reserva.Detalles),
                                 TablaDetalleReserva.NombreTipo);

            var salidaId = comando.AgregarSalidaEntero("@IdReservaOut");
            var salidaCodigo = comando.AgregarSalidaTexto("@CodigoOut", 20);
            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 400);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            var resultado = new ReservaGuardarResultado
            {
                IdReserva = salidaId.ValorEntero(),
                Codigo = salidaCodigo.ValorTexto(),
                Mensaje = salidaMensaje.ValorTexto()
            };

            Registro.Informacion(
                $"Reserva {resultado.Codigo} guardada por el usuario {idUsuario} con {reserva.Detalles.Count} linea(s) de detalle.");

            return resultado;
        }, cancelacion);

    /// <summary>
    /// Comprobacion anticipada de disponibilidad. Devuelve la lista completa de conflictos para
    /// que el usuario pueda corregirlos todos de una vez, en lugar de uno por intento.
    /// </summary>
    public Task<DisponibilidadResultado> ValidarDisponibilidadAsync(DisponibilidadConsultaDto consulta,
                                                                    CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Disponibilidad_Validar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", consulta.IdReserva);
            comando.AgregarEntero("@IdSalon", consulta.IdSalon);
            comando.AgregarFecha("@FechaEvento", consulta.FechaEvento);
            comando.AgregarHora("@HoraInicio", consulta.HoraInicio);
            comando.AgregarHora("@HoraFin", consulta.HoraFin);
            comando.AgregarEntero("@NumeroInvitados", consulta.NumeroInvitados);
            comando.AgregarTabla("@Detalle", TablaDetalleReserva.Construir(consulta.Detalles),
                                 TablaDetalleReserva.NombreTipo);
            comando.AgregarBooleano("@Silencioso", false);

            var salidaValido = comando.AgregarSalidaBooleano("@EsValido");
            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 400);

            var conflictos = new List<ConflictoDisponibilidad>();

            await using (var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await lector.ReadAsync(ct).ConfigureAwait(false))
                {
                    conflictos.Add(new ConflictoDisponibilidad
                    {
                        Tipo = lector.Texto("Tipo"),
                        Referencia = lector.TextoNulo("Referencia"),
                        Detalle = lector.Texto("Detalle")
                    });
                }

                await lector.CloseAsync().ConfigureAwait(false);
            }

            return new DisponibilidadResultado
            {
                EsValido = salidaValido.ValorBooleano(),
                Mensaje = salidaMensaje.ValorTexto(),
                Conflictos = conflictos
            };
        }, cancelacion);

    public Task<PaginaResultado<ReservaResumenDto>> ConsultarAsync(ReservaFiltroDto filtro,
                                                                   CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Reserva_Consultar", async (comando, ct) =>
        {
            comando.AgregarTexto("@Codigo", filtro.Codigo, 20);
            comando.AgregarEntero("@IdCliente", filtro.IdCliente);
            comando.AgregarTexto("@TextoCliente", filtro.TextoCliente, 100);
            comando.AgregarFecha("@FechaDesde", filtro.FechaDesde);
            comando.AgregarFecha("@FechaHasta", filtro.FechaHasta);
            comando.AgregarEntero("@IdSalon", filtro.IdSalon);
            comando.AgregarTextoAscii("@Estado", filtro.Estado?.ASql(), 12);
            comando.AgregarEntero("@Pagina", filtro.Pagina);
            comando.AgregarEntero("@TamanoPagina", filtro.TamanoPagina);

            var salidaTotal = comando.AgregarSalidaEntero("@TotalRegistros");

            var elementos = new List<ReservaResumenDto>();

            await using (var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await lector.ReadAsync(ct).ConfigureAwait(false))
                {
                    elementos.Add(new ReservaResumenDto
                    {
                        IdReserva = lector.Entero("IdReserva"),
                        Codigo = lector.Texto("Codigo"),
                        IdCliente = lector.Entero("IdCliente"),
                        Cliente = lector.Texto("Cliente"),
                        Identificacion = lector.Texto("Identificacion"),
                        EmailCliente = lector.Texto("EmailCliente"),
                        IdSalon = lector.Entero("IdSalon"),
                        Salon = lector.Texto("Salon"),
                        FechaEvento = lector.Fecha("FechaEvento"),
                        HoraInicio = lector.Hora("HoraInicio"),
                        HoraFin = lector.Hora("HoraFin"),
                        NumeroInvitados = lector.Entero("NumeroInvitados"),
                        Estado = ConversionesEnum.AEstadoReserva(lector.Texto("Estado")),
                        Subtotal = lector.Decimal("Subtotal"),
                        Descuento = lector.Decimal("Descuento"),
                        Impuesto = lector.Decimal("Impuesto"),
                        Total = lector.Decimal("Total"),
                        Observacion = lector.TextoNulo("Observacion"),
                        TotalDetalles = lector.Entero("TotalDetalles"),
                        UltimoAnalisis = lector.FechaHoraNula("UltimoAnalisis"),
                        UltimoCorreo = lector.FechaHoraNula("UltimoCorreo"),
                        FechaCreacion = lector.FechaHora("FechaCreacion"),
                        UsuarioCreacion = lector.Texto("UsuarioCreacion")
                    });
                }

                await lector.CloseAsync().ConfigureAwait(false);
            }

            return new PaginaResultado<ReservaResumenDto>
            {
                Elementos = elementos,
                TotalRegistros = salidaTotal.ValorEntero(),
                Pagina = filtro.Pagina,
                TamanoPagina = filtro.TamanoPagina
            };
        }, cancelacion);

    /// <summary>
    /// Lee los DOS conjuntos de resultados de evt.sp_Reserva_ObtenerPorId: primero la cabecera
    /// y luego el detalle, avanzando con NextResultAsync sobre el mismo lector y la misma
    /// conexion. Es una sola ida y vuelta al servidor.
    /// </summary>
    public Task<Reserva?> ObtenerPorIdAsync(int idReserva, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Reserva_ObtenerPorId", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", idReserva);

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            if (!await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                return (Reserva?)null;
            }

            var reserva = new Reserva
            {
                IdReserva = lector.Entero("IdReserva"),
                Codigo = lector.Texto("Codigo"),
                IdCliente = lector.Entero("IdCliente"),
                Cliente = lector.Texto("Cliente"),
                IdentificacionCliente = lector.Texto("Identificacion"),
                EmailCliente = lector.Texto("EmailCliente"),
                TelefonoCliente = lector.TextoNulo("TelefonoCliente"),
                IdSalon = lector.Entero("IdSalon"),
                Salon = lector.Texto("Salon"),
                CapacidadSalon = lector.Entero("CapacidadSalon"),
                TarifaBaseSalon = lector.Decimal("TarifaBaseSalon"),
                FechaEvento = lector.Fecha("FechaEvento"),
                HoraInicio = lector.Hora("HoraInicio"),
                HoraFin = lector.Hora("HoraFin"),
                NumeroInvitados = lector.Entero("NumeroInvitados"),
                Estado = ConversionesEnum.AEstadoReserva(lector.Texto("Estado")),
                Subtotal = lector.Decimal("Subtotal"),
                Descuento = lector.Decimal("Descuento"),
                Impuesto = lector.Decimal("Impuesto"),
                Total = lector.Decimal("Total"),
                Observacion = lector.TextoNulo("Observacion"),
                MotivoCancelacion = lector.TextoNulo("MotivoCancelacion"),
                JustificacionContingencia = lector.TextoNulo("JustificacionContingencia"),
                IdUsuarioCreacion = lector.Entero("IdUsuarioCreacion"),
                UsuarioCreacion = lector.Texto("UsuarioCreacion"),
                FechaCreacion = lector.FechaHora("FechaCreacion"),
                IdUsuarioModificacion = lector.EnteroNulo("IdUsuarioModificacion"),
                FechaModificacion = lector.FechaHoraNula("FechaModificacion")
            };

            // Segundo conjunto: el detalle completo.
            if (await lector.NextResultAsync(ct).ConfigureAwait(false))
            {
                while (await lector.ReadAsync(ct).ConfigureAwait(false))
                {
                    reserva.Detalles.Add(new ReservaDetalle
                    {
                        IdDetalle = lector.Entero("IdDetalle"),
                        IdReserva = lector.Entero("IdReserva"),
                        IdRecurso = lector.Entero("IdRecurso"),
                        Recurso = lector.Texto("Recurso"),
                        TipoRecurso = ConversionesEnum.ATipoRecurso(lector.Texto("TipoRecurso")),
                        StockTotal = lector.Entero("StockTotal"),
                        Cantidad = lector.Entero("Cantidad"),
                        PrecioUnitario = lector.Decimal("PrecioUnitario"),
                        PorcentajeDescuento = lector.Decimal("PorcentajeDescuento")
                    });
                }
            }

            return reserva;
        }, cancelacion);

    public Task<string> CambiarEstadoAsync(CambioEstadoDto cambio, int idUsuario, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Reserva_CambiarEstado", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", cambio.IdReserva);
            comando.AgregarTextoAscii("@EstadoNuevo", cambio.EstadoNuevo.ASql(), 12);
            comando.AgregarTexto("@Motivo", cambio.Motivo, 500);
            comando.AgregarTexto("@JustificacionContingencia", cambio.JustificacionContingencia, 500);
            comando.AgregarEntero("@IdUsuario", idUsuario);

            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 400);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            var mensaje = salidaMensaje.ValorTexto();

            Registro.Informacion(
                $"Reserva {cambio.IdReserva} cambio a {cambio.EstadoNuevo.ASql()} por el usuario {idUsuario}.");

            return mensaje;
        }, cancelacion);

    public Task<IReadOnlyList<ReservaAuditoria>> ObtenerAuditoriaAsync(int idReserva, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Reserva_Auditoria_Consultar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", idReserva);

            var movimientos = new List<ReservaAuditoria>();

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                var estadoAnterior = lector.TextoNulo("EstadoAnterior");

                movimientos.Add(new ReservaAuditoria
                {
                    IdAuditoria = lector.EnteroLargo("IdAuditoria"),
                    IdReserva = lector.Entero("IdReserva"),
                    EstadoAnterior = estadoAnterior is null ? null : ConversionesEnum.AEstadoReserva(estadoAnterior),
                    EstadoNuevo = ConversionesEnum.AEstadoReserva(lector.Texto("EstadoNuevo")),
                    Motivo = lector.TextoNulo("Motivo"),
                    Usuario = lector.Texto("Usuario"),
                    Fecha = lector.FechaHora("Fecha")
                });
            }

            return (IReadOnlyList<ReservaAuditoria>)movimientos;
        }, cancelacion);
}
