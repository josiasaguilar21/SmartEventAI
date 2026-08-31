using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.Core.Exceptions;
using SmartEvent.Infrastructure.Data.Comun;
using SmartEvent.Infrastructure.Data.Conexion;

namespace SmartEvent.Infrastructure.Data.Repositorios;

/// <summary>
/// Repositorio de seguridad. La autenticacion se resuelve en dos llamadas deliberadamente
/// separadas para que el hash almacenado nunca salga de SQL Server:
///
///   1. seg.sp_Usuario_ObtenerParametrosHash -> devuelve salt e iteraciones (datos publicos).
///   2. la aplicacion deriva la clave con PBKDF2 y envia SOLO el resultado.
///   3. seg.sp_Usuario_Autenticar -> compara dentro del motor y aplica el bloqueo por intentos.
/// </summary>
public sealed class UsuarioRepositorio : RepositorioBase, IUsuarioRepositorio
{
    public UsuarioRepositorio(IFabricaConexiones fabrica, IRegistroEventos registro)
        : base(fabrica, registro) { }

    public Task<ParametrosHash> ObtenerParametrosHashAsync(string nombreUsuario, CancellationToken cancelacion) =>
        EjecutarAsync("seg.sp_Usuario_ObtenerParametrosHash", async (comando, ct) =>
        {
            comando.AgregarTexto("@NombreUsuario", nombreUsuario, 50);

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            if (!await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                throw new ErrorTecnicoException(
                    "seg.sp_Usuario_ObtenerParametrosHash no devolvio los parametros de derivacion.");
            }

            return new ParametrosHash
            {
                Salt = lector.Binario("PasswordSalt"),
                Iteraciones = lector.Entero("Iteraciones"),
                Algoritmo = lector.Texto("Algoritmo")
            };
        }, cancelacion);

    public Task<AutenticacionResultado> AutenticarAsync(string nombreUsuario, byte[] hashDerivado,
                                                        CancellationToken cancelacion) =>
        EjecutarAsync("seg.sp_Usuario_Autenticar", async (comando, ct) =>
        {
            comando.AgregarTexto("@NombreUsuario", nombreUsuario, 50);
            comando.AgregarBinario("@PasswordHash", hashDerivado, 64);

            var salidaResultado = comando.AgregarSalidaEntero("@Resultado");
            var salidaMensaje = comando.AgregarSalidaTexto("@Mensaje", 200);
            var salidaBloqueo = comando.AgregarSalidaEntero("@SegundosBloqueo");

            SesionUsuario? sesion = null;

            await using (var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                // El procedimiento solo emite este conjunto cuando las credenciales son validas.
                if (await lector.ReadAsync(ct).ConfigureAwait(false))
                {
                    sesion = new SesionUsuario
                    {
                        IdUsuario = lector.Entero("IdUsuario"),
                        NombreUsuario = lector.Texto("NombreUsuario"),
                        NombreCompleto = lector.Texto("NombreCompleto"),
                        Email = lector.TextoNulo("Email"),
                        Rol = lector.Texto("Rol"),
                        UltimoAcceso = lector.FechaHoraNula("UltimoAcceso")
                    };
                }

                // Los parametros de salida solo estan disponibles cuando el flujo de resultados
                // se ha consumido por completo: por eso se cierra el lector antes de leerlos.
                await lector.CloseAsync().ConfigureAwait(false);
            }

            var codigo = (ResultadoAutenticacion)salidaResultado.ValorEntero();
            var mensaje = salidaMensaje.ValorTexto();
            var segundos = salidaBloqueo.ValorEntero();

            if (codigo == ResultadoAutenticacion.Correcto && sesion is not null)
            {
                Registro.Informacion($"Inicio de sesion correcto del usuario '{sesion.NombreUsuario}' con rol {sesion.Rol}.");
                return AutenticacionResultado.Correcto(sesion, mensaje);
            }

            // Se registra el intento fallido sin escribir jamas la contrasena ni el hash.
            Registro.Advertencia($"Intento de acceso rechazado para el usuario '{nombreUsuario}' (resultado {codigo}).");
            return AutenticacionResultado.Fallido(codigo, mensaje, segundos);
        }, cancelacion);

    public Task<IReadOnlyList<Usuario>> ConsultarAsync(string? filtro, bool? estado, CancellationToken cancelacion) =>
        EjecutarAsync("seg.sp_Usuario_Consultar", async (comando, ct) =>
        {
            comando.AgregarTexto("@Filtro", filtro, 100);
            comando.AgregarBooleano("@Estado", estado);

            var usuarios = new List<Usuario>();

            await using var lector = await comando.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await lector.ReadAsync(ct).ConfigureAwait(false))
            {
                usuarios.Add(new Usuario
                {
                    IdUsuario = lector.Entero("IdUsuario"),
                    NombreUsuario = lector.Texto("NombreUsuario"),
                    NombreCompleto = lector.Texto("NombreCompleto"),
                    Email = lector.TextoNulo("Email"),
                    Rol = lector.Texto("Rol"),
                    Estado = lector.Booleano("Estado"),
                    Bloqueado = lector.Booleano("Bloqueado"),
                    UltimoAcceso = lector.FechaHoraNula("UltimoAcceso"),
                    FechaCreacion = lector.FechaHora("FechaCreacion")
                });
            }

            return (IReadOnlyList<Usuario>)usuarios;
        }, cancelacion);
}
