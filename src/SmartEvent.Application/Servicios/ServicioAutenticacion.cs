using SmartEvent.Application.Sesion;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Enums;

namespace SmartEvent.Application.Servicios;

public interface IServicioAutenticacion
{
    /// <summary>
    /// Ejecuta el flujo completo de acceso. Si es correcto, deja la sesion establecida en el
    /// contexto de la aplicacion.
    /// </summary>
    Task<AutenticacionResultado> IniciarSesionAsync(string nombreUsuario, string contrasena,
                                                    CancellationToken cancelacion);

    void CerrarSesion();
}

/// <summary>
/// Orquesta la autenticacion en tres pasos, sin que la contrasena salga nunca del proceso:
///
///   1. Pide a la base el salt y las iteraciones del usuario (datos publicos).
///   2. Deriva la clave localmente con PBKDF2-HMAC-SHA256.
///   3. Envia SOLO el hash resultante. La comparacion, el conteo de intentos fallidos y el
///      bloqueo temporal ocurren dentro de seg.sp_Usuario_Autenticar.
///
/// El servicio no decide si las credenciales son correctas: eso lo resuelve el motor. Aqui
/// solo se coordina el flujo y se traduce el resultado.
/// </summary>
public sealed class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IHasheadorContrasena _hasheador;
    private readonly IContextoSesion _contexto;
    private readonly IRegistroEventos _registro;

    public ServicioAutenticacion(IUsuarioRepositorio usuarios, IHasheadorContrasena hasheador,
                                 IContextoSesion contexto, IRegistroEventos registro)
    {
        _usuarios = usuarios;
        _hasheador = hasheador;
        _contexto = contexto;
        _registro = registro;
    }

    public async Task<AutenticacionResultado> IniciarSesionAsync(string nombreUsuario, string contrasena,
                                                                 CancellationToken cancelacion)
    {
        // Validacion de forma. Se responde con el MISMO mensaje generico que usaria la base
        // para no dar ninguna pista sobre que parte de las credenciales esta mal.
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contrasena))
        {
            return AutenticacionResultado.Fallido(
                ResultadoAutenticacion.CredencialesInvalidas,
                "Escriba su usuario y su contrasena para continuar.");
        }

        nombreUsuario = nombreUsuario.Trim();

        var parametros = await _usuarios.ObtenerParametrosHashAsync(nombreUsuario, cancelacion)
                                        .ConfigureAwait(false);

        // La derivacion siempre se ejecuta, incluso si el usuario no existe: en ese caso la base
        // devuelve un salt senuelo con el mismo coste, de modo que el tiempo de respuesta no
        // permita deducir que cuentas existen.
        var hash = _hasheador.Derivar(contrasena, parametros);

        var resultado = await _usuarios.AutenticarAsync(nombreUsuario, hash, cancelacion)
                                       .ConfigureAwait(false);

        if (resultado.EsCorrecto && resultado.Sesion is not null)
        {
            _contexto.Iniciar(resultado.Sesion);
        }

        return resultado;
    }

    public void CerrarSesion()
    {
        var usuario = _contexto.Actual?.NombreUsuario;

        _contexto.Cerrar();

        if (usuario is not null)
        {
            _registro.Informacion($"Cierre de sesion del usuario '{usuario}'.");
        }
    }
}
