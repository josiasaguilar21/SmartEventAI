using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Infrastructure.Data.Comun;
using SmartEvent.Infrastructure.Data.Conexion;

namespace SmartEvent.Infrastructure.Data.Repositorios;

/// <summary>
/// Mantenimiento de recursos y servicios. <c>StockTotal</c> es el inventario maximo; el
/// disponible real depende de la fecha y la franja horaria y lo calcula
/// evt.sp_Disponibilidad_Validar, no este repositorio.
/// </summary>
public sealed class RecursoRepositorio : RepositorioBase, IRecursoRepositorio
{
    public RecursoRepositorio(IFabricaConexiones fabrica, IRegistroEventos registro)
        : base(fabrica, registro) { }

    public Task<IReadOnlyList<Recurso>> ConsultarAsync(int? idRecurso, string? filtro, TipoRecurso? tipo,
                                                       bool? estado, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Recurso_Consultar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdRecurso", idRecurso);
            comando.AgregarTexto("@Filtro", filtro, 100);
            comando.AgregarTextoAscii("@Tipo", tipo?.ASql(), 20);
            comando.AgregarBooleano("@Estado", estado);

            var recursos = new List<Recurso>();

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                recursos.Add(new Recurso
                {
                    IdRecurso = lector.Entero("IdRecurso"),
                    Nombre = lector.Texto("Nombre"),
                    Tipo = ConversionesEnum.ATipoRecurso(lector.Texto("Tipo")),
                    StockTotal = lector.Entero("StockTotal"),
                    PrecioUnitario = lector.Decimal("PrecioUnitario"),
                    Estado = lector.Booleano("Estado"),
                    FechaCreacion = lector.FechaHora("FechaCreacion"),
                    FechaModificacion = lector.FechaHoraNula("FechaModificacion")
                });
            }

            return (IReadOnlyList<Recurso>)recursos;
        }, cancelacion);

    public async Task<Recurso?> ObtenerPorIdAsync(int idRecurso, CancellationToken cancelacion)
    {
        var recursos = await ConsultarAsync(idRecurso, null, null, null, cancelacion).ConfigureAwait(false);
        return recursos.FirstOrDefault();
    }

    public Task<ResultadoOperacion> GuardarAsync(Recurso recurso, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Recurso_Guardar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdRecurso", recurso.IdRecurso > 0 ? recurso.IdRecurso : null);
            comando.AgregarTexto("@Nombre", recurso.Nombre, 100);
            comando.AgregarTextoAscii("@Tipo", recurso.Tipo.ASql(), 20);
            comando.AgregarEntero("@StockTotal", recurso.StockTotal);
            comando.AgregarDecimal("@PrecioUnitario", recurso.PrecioUnitario);
            comando.AgregarBooleano("@Estado", recurso.Estado);

            var salidaId = comando.AgregarSalidaEntero("@IdRecursoOut");
            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 200);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return new ResultadoOperacion
            {
                Id = salidaId.ValorEntero(),
                Mensaje = salidaMensaje.ValorTexto()
            };
        }, cancelacion);

    public Task<string> CambiarEstadoAsync(int idRecurso, bool estado, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Recurso_CambiarEstado", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdRecurso", idRecurso);
            comando.AgregarBooleano("@Estado", estado);

            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 200);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return salidaMensaje.ValorTexto();
        }, cancelacion);
}
