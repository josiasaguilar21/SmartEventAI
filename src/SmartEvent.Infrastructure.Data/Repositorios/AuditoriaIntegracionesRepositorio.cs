using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Infrastructure.Data.Comun;
using SmartEvent.Infrastructure.Data.Conexion;

namespace SmartEvent.Infrastructure.Data.Repositorios;

/// <summary>
/// Persistencia de la auditoria de integraciones.
///
/// Se registra SIEMPRE el intento, haya funcionado o no. Es lo que permite demostrar CA-07
/// (dos intentos de correo auditados, sin duplicar la reserva) y CA-08/CA-09 (analisis de IA
/// exitoso y fallido, ambos con rastro). Ninguna de estas operaciones recibe credenciales.
/// </summary>
public sealed class AuditoriaIntegracionesRepositorio : RepositorioBase, IAuditoriaIntegracionesRepositorio
{
    /// <summary>Longitud maxima de la columna Error. Se recorta aqui para no perder el registro.</summary>
    private const int MaximoLongitudError = 500;

    public AuditoriaIntegracionesRepositorio(IFabricaConexiones fabrica, IRegistroEventos registro)
        : base(fabrica, registro) { }

    public Task<int> RegistrarCorreoAsync(int idReserva, ResultadoCorreo resultado, TipoNotificacion tipo,
                                          int? idUsuario, CancellationToken cancelacion) =>
        EjecutarAsync("com.sp_Correo_Registrar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", idReserva);
            comando.AgregarTextoAscii("@TipoNotificacion", tipo.ASql(), 20);
            comando.AgregarTexto("@Destinatario", resultado.Destinatario, 150);
            comando.AgregarTexto("@Asunto", resultado.Asunto, 200);
            comando.AgregarTextoAscii("@Estado", resultado.Estado.ASql(), 10);
            comando.AgregarTexto("@Error", Recortar(resultado.Error), MaximoLongitudError);
            comando.AgregarEntero("@IdUsuario", idUsuario);

            var salidaId = comando.AgregarSalidaEntero("@IdCorreoOut");

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return salidaId.ValorEntero();
        }, cancelacion);

    public Task<IReadOnlyList<CorreoEnviado>> ConsultarCorreosAsync(FiltroCorreoDto filtro,
                                                                    CancellationToken cancelacion) =>
        EjecutarAsync("com.sp_Correo_Consultar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", filtro.IdReserva);
            comando.AgregarTexto("@Codigo", filtro.Codigo, 20);
            comando.AgregarTextoAscii("@Estado", filtro.Estado?.ASql(), 10);
            comando.AgregarFecha("@FechaDesde", filtro.FechaDesde);
            comando.AgregarFecha("@FechaHasta", filtro.FechaHasta);

            var correos = new List<CorreoEnviado>();

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                correos.Add(new CorreoEnviado
                {
                    IdCorreo = lector.Entero("IdCorreo"),
                    IdReserva = lector.Entero("IdReserva"),
                    CodigoReserva = lector.Texto("CodigoReserva"),
                    ClienteReserva = lector.Texto("Cliente"),
                    TipoNotificacion = ConversionesEnum.ATipoNotificacion(lector.Texto("TipoNotificacion")),
                    Destinatario = lector.Texto("Destinatario"),
                    Asunto = lector.Texto("Asunto"),
                    FechaIntento = lector.FechaHora("FechaIntento"),
                    Estado = ConversionesEnum.AEstadoCorreo(lector.Texto("Estado")),
                    Error = lector.TextoNulo("Error"),
                    Usuario = lector.TextoNulo("Usuario")
                });
            }

            return (IReadOnlyList<CorreoEnviado>)correos;
        }, cancelacion);

    public Task<int> RegistrarAnalisisAsync(int idReserva, AnalisisIAResultado resultado, int? idUsuario,
                                            CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_AnalisisIA_Registrar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", idReserva);
            comando.AgregarTexto("@Modelo", resultado.Modelo, 100);
            comando.AgregarTexto("@PromptVersion", resultado.PromptVersion, 20);

            // El JSON se guarda completo: es la evidencia del analisis. La columna es NVARCHAR(MAX),
            // por eso el parametro se declara con tamano -1.
            comando.AgregarTexto("@RespuestaJson", resultado.RespuestaJson, -1);

            comando.AgregarTextoAscii("@NivelRiesgo", resultado.Respuesta?.NivelRiesgoEnum.ASql(), 5);
            comando.AgregarEntero("@TokensEntrada", resultado.TokensEntrada);
            comando.AgregarEntero("@TokensSalida", resultado.TokensSalida);
            comando.AgregarBooleano("@Exitoso", resultado.Exitoso);
            comando.AgregarTexto("@Error", Recortar(resultado.Error), MaximoLongitudError);
            comando.AgregarEntero("@IdUsuario", idUsuario);

            var salidaId = comando.AgregarSalidaEntero("@IdAnalisisOut");

            await comando.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return salidaId.ValorEntero();
        }, cancelacion);

    public Task<IReadOnlyList<AnalisisIA>> ConsultarAnalisisAsync(FiltroAnalisisDto filtro,
                                                                  CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_AnalisisIA_Consultar", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", filtro.IdReserva);
            comando.AgregarTexto("@Codigo", filtro.Codigo, 20);
            comando.AgregarBooleano("@Exitoso", filtro.Exitoso);
            comando.AgregarTextoAscii("@NivelRiesgo", filtro.NivelRiesgo?.ASql(), 5);
            comando.AgregarFecha("@FechaDesde", filtro.FechaDesde);
            comando.AgregarFecha("@FechaHasta", filtro.FechaHasta);

            var analisis = new List<AnalisisIA>();

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                analisis.Add(MapearAnalisis(lector, incluirDatosReserva: true));
            }

            return (IReadOnlyList<AnalisisIA>)analisis;
        }, cancelacion);

    public Task<AnalisisIA?> ObtenerUltimoAnalisisAsync(int idReserva, CancellationToken cancelacion) =>
        EjecutarAsync("evt.sp_AnalisisIA_ObtenerUltimo", async (comando, ct) =>
        {
            comando.AgregarEntero("@IdReserva", idReserva);

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            return await lector.ReadAsync(ct).ConfigureAwait(false)
                ? MapearAnalisis(lector, incluirDatosReserva: false)
                : null;
        }, cancelacion);

    private static AnalisisIA MapearAnalisis(Microsoft.Data.SqlClient.SqlDataReader lector, bool incluirDatosReserva)
    {
        var nivel = lector.TextoNulo("NivelRiesgo");

        var entidad = new AnalisisIA
        {
            IdAnalisis = lector.Entero("IdAnalisis"),
            IdReserva = lector.Entero("IdReserva"),
            Modelo = lector.Texto("Modelo"),
            PromptVersion = lector.Texto("PromptVersion"),
            RespuestaJson = lector.TextoNulo("RespuestaJson"),
            TokensEntrada = lector.EnteroNulo("TokensEntrada"),
            TokensSalida = lector.EnteroNulo("TokensSalida"),
            Fecha = lector.FechaHora("Fecha"),
            Exitoso = lector.Booleano("Exitoso"),
            Error = lector.TextoNulo("Error")
        };

        if (nivel is not null && ConversionesEnum.TryANivelRiesgo(nivel, out var nivelRiesgo))
        {
            entidad.NivelRiesgo = nivelRiesgo;
        }

        // El procedimiento que devuelve el ultimo analisis no proyecta las columnas de la
        // reserva porque quien lo llama ya la tiene cargada.
        if (incluirDatosReserva)
        {
            entidad.CodigoReserva = lector.Texto("CodigoReserva");
            entidad.ClienteReserva = lector.Texto("Cliente");
            entidad.Usuario = lector.TextoNulo("Usuario");
        }

        return entidad;
    }

    private static string? Recortar(string? texto) =>
        string.IsNullOrWhiteSpace(texto)
            ? null
            : texto.Length <= MaximoLongitudError ? texto : texto[..MaximoLongitudError];
}
