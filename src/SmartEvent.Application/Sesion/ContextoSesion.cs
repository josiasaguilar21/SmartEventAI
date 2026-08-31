using SmartEvent.Core.Dtos;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.Application.Sesion;

/// <summary>
/// Sesion activa de la aplicacion. Se establece al autenticarse y la consultan los servicios
/// que necesitan saber QUIEN esta operando, en lugar de arrastrar el usuario como parametro
/// por todas las firmas.
/// </summary>
public interface IContextoSesion
{
    SesionUsuario? Actual { get; }

    bool HaySesion { get; }

    /// <summary>Devuelve la sesion o lanza si no hay ninguna. Usado por los servicios de negocio.</summary>
    SesionUsuario Requerida { get; }

    void Iniciar(SesionUsuario sesion);

    void Cerrar();
}

/// <summary>
/// Implementacion en memoria del contexto. Vive mientras la aplicacion esta abierta y se
/// limpia al cerrar sesion, de modo que un formulario que quede abierto por error no pueda
/// seguir operando con la identidad anterior.
/// </summary>
public sealed class ContextoSesion : IContextoSesion
{
    public SesionUsuario? Actual { get; private set; }

    public bool HaySesion => Actual is not null;

    public SesionUsuario Requerida =>
        Actual ?? throw new ReglaNegocioException(
            "La sesion ha finalizado. Vuelva a iniciar sesion para continuar.");

    public void Iniciar(SesionUsuario sesion)
    {
        ArgumentNullException.ThrowIfNull(sesion);
        Actual = sesion;
    }

    public void Cerrar() => Actual = null;
}
