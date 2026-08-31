using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Infrastructure.Data.Comun;
using SmartEvent.Infrastructure.Data.Conexion;

namespace SmartEvent.Infrastructure.Data.Repositorios;

/// <summary>
/// Mantenimiento de salones. El filtro <c>capacidadMinima</c> permite que el formulario de
/// reserva ofrezca solo los salones capaces de albergar el numero de invitados indicado,
/// evitando que el usuario elija una combinacion que el motor va a rechazar.
/// </summary>
public sealed class SalonRepositorio : RepositorioBase, ISalonRepositorio
{
    public SalonRepositorio(IFabricaConexiones fabrica, IRegistroEventos registro)
        : base(fabrica, registro) { }

    public Task<IReadOnlyList<Salon>> ConsultarAsync(int? idSalon, string? filtro, bool? estado,
                                                     int? capacidadMinima, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Salon_Consultar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdSalon", idSalon);
            comando.AgregarTexto("@Filtro", filtro, 100);
            comando.AgregarBooleano("@Estado", estado);
            comando.AgregarEntero("@CapacidadMinima", capacidadMinima);

            var salones = new List<Salon>();

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                salones.Add(new Salon
                {
                    IdSalon = lector.Entero("IdSalon"),
                    Nombre = lector.Texto("Nombre"),
                    Ubicacion = lector.TextoNulo("Ubicacion"),
                    Capacidad = lector.Entero("Capacidad"),
                    TarifaBase = lector.Decimal("TarifaBase"),
                    Estado = lector.Booleano("Estado"),
                    FechaCreacion = lector.FechaHora("FechaCreacion"),
                    FechaModificacion = lector.FechaHoraNula("FechaModificacion")
                });
            }

            return (IReadOnlyList<Salon>)salones;
        }, cancelacion);

    public async Task<Salon?> ObtenerPorIdAsync(int idSalon, CancellationToken cancelacion)
    {
        var salones = await ConsultarAsync(idSalon, null, null, null, cancelacion).ConfigureAwait(false);
        return salones.FirstOrDefault();
    }

    public Task<ResultadoOperacion> GuardarAsync(Salon salon, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Salon_Guardar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdSalon", salon.IdSalon > 0 ? salon.IdSalon : null);
            comando.AgregarTexto("@Nombre", salon.Nombre, 100);
            comando.AgregarTexto("@Ubicacion", salon.Ubicacion, 150);
            comando.AgregarEntero("@Capacidad", salon.Capacidad);
            comando.AgregarDecimal("@TarifaBase", salon.TarifaBase);
            comando.AgregarBooleano("@Estado", salon.Estado);

            var salidaId = comando.AgregarSalidaEntero("@IdSalonOut");
            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 200);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return new ResultadoOperacion
            {
                Id = salidaId.ValorEntero(),
                Mensaje = salidaMensaje.ValorTexto()
            };
        }, cancelacion);

    public Task<string> CambiarEstadoAsync(int idSalon, bool estado, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Salon_CambiarEstado", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdSalon", idSalon);
            comando.AgregarBooleano("@Estado", estado);

            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 200);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return salidaMensaje.ValorTexto();
        }, cancelacion);
}
