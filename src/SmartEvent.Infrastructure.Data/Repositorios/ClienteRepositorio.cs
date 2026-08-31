using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Infrastructure.Data.Comun;
using SmartEvent.Infrastructure.Data.Conexion;

namespace SmartEvent.Infrastructure.Data.Repositorios;

/// <summary>
/// Mantenimiento de clientes sobre evt.sp_Cliente_Guardar / _Consultar / _CambiarEstado.
/// La deteccion de duplicados por identificacion y el bloqueo de la inactivacion cuando hay
/// reservas vigentes ocurren en el procedimiento, no aqui: son reglas de negocio y deben
/// cumplirse aunque alguien invoque la base por otro medio.
/// </summary>
public sealed class ClienteRepositorio : RepositorioBase, IClienteRepositorio
{
    public ClienteRepositorio(IFabricaConexiones fabrica, IRegistroEventos registro)
        : base(fabrica, registro) { }

    public Task<IReadOnlyList<Cliente>> ConsultarAsync(int? idCliente, string? filtro, bool? estado,
                                                       CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Cliente_Consultar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdCliente", idCliente);
            comando.AgregarTexto("@Filtro", filtro, 100);
            comando.AgregarBooleano("@Estado", estado);

            var clientes = new List<Cliente>();

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                clientes.Add(Mapear(lector));
            }

            return (IReadOnlyList<Cliente>)clientes;
        }, cancelacion);

    public async Task<Cliente?> ObtenerPorIdAsync(int idCliente, CancellationToken cancelacion)
    {
        var clientes = await ConsultarAsync(idCliente, null, null, cancelacion).ConfigureAwait(false);
        return clientes.FirstOrDefault();
    }

    public Task<ResultadoOperacion> GuardarAsync(Cliente cliente, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Cliente_Guardar", async (comando, ct) =>
        {
            // IdCliente nulo = alta; con valor = edicion. Lo decide el procedimiento.
            comando.AgregarEntero("@IdCliente", cliente.IdCliente > 0 ? cliente.IdCliente : null);
            comando.AgregarTexto("@Identificacion", cliente.Identificacion, 20);
            comando.AgregarTexto("@Nombres", cliente.Nombres, 150);
            comando.AgregarTexto("@Email", cliente.Email, 150);
            comando.AgregarTexto("@Telefono", cliente.Telefono, 20);
            comando.AgregarBooleano("@Estado", cliente.Estado);

            var salidaId = comando.AgregarSalidaEntero("@IdClienteOut");
            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 200);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return new ResultadoOperacion
            {
                Id = salidaId.ValorEntero(),
                Mensaje = salidaMensaje.ValorTexto()
            };
        }, cancelacion);

    public Task<string> CambiarEstadoAsync(int idCliente, bool estado, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_Cliente_CambiarEstado", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdCliente", idCliente);
            comando.AgregarBooleano("@Estado", estado);

            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 200);

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return salidaMensaje.ValorTexto();
        }, cancelacion);

    private static Cliente Mapear(Microsoft.Data.SqlClient.SqlDataReader lector) => new()
    {
        IdCliente = lector.Entero("IdCliente"),
        Identificacion = lector.Texto("Identificacion"),
        Nombres = lector.Texto("Nombres"),
        Email = lector.Texto("Email"),
        Telefono = lector.TextoNulo("Telefono"),
        Estado = lector.Booleano("Estado"),
        FechaCreacion = lector.FechaHora("FechaCreacion"),
        FechaModificacion = lector.FechaHoraNula("FechaModificacion")
    };
}
