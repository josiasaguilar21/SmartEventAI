using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;

namespace SmartEvent.Core.Abstractions;

/// <summary>
/// Acceso a los datos de seguridad. Ninguna de estas operaciones devuelve el hash almacenado:
/// la comparacion de credenciales ocurre dentro de SQL Server.
/// </summary>
public interface IUsuarioRepositorio
{
    /// <summary>
    /// Obtiene el salt y el numero de iteraciones necesarios para derivar la clave.
    /// Si el usuario no existe o esta inactivo devuelve parametros senuelo con el mismo coste,
    /// de modo que el tiempo de respuesta no revele si la cuenta existe.
    /// </summary>
    Task<ParametrosHash> ObtenerParametrosHashAsync(string nombreUsuario, CancellationToken cancelacion);

    /// <summary>
    /// Envia el hash ya derivado para que el procedimiento lo compare y aplique el control de
    /// intentos fallidos y el bloqueo temporal.
    /// </summary>
    Task<AutenticacionResultado> AutenticarAsync(string nombreUsuario, byte[] hashDerivado, CancellationToken cancelacion);

    Task<IReadOnlyList<Usuario>> ConsultarAsync(string? filtro, bool? estado, CancellationToken cancelacion);
}
