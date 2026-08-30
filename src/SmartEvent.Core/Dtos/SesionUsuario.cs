using SmartEvent.Core.Seguridad;

namespace SmartEvent.Core.Dtos;

/// <summary>
/// Identidad del usuario autenticado durante la sesion. Se construye una sola vez tras el
/// login y viaja hacia los servicios; no contiene ni la contrasena ni el hash.
///
/// Las propiedades de permiso existen para que la interfaz oculte o deshabilite opciones.
/// La autorizacion REAL se aplica ademas en SQL Server (por ejemplo, el descuento de linea
/// superior al 10 por ciento lo rechaza evt.sp_Reserva_Guardar segun el rol del usuario que
/// se le envia), de modo que ocultar un boton nunca es la unica defensa.
/// </summary>
public sealed class SesionUsuario
{
    public required int IdUsuario { get; init; }
    public required string NombreUsuario { get; init; }
    public required string NombreCompleto { get; init; }
    public string? Email { get; init; }
    public required string Rol { get; init; }
    public DateTime? UltimoAcceso { get; init; }
    public DateTime InicioSesion { get; init; } = DateTime.Now;

    public bool EsAdministrador => RolesAplicacion.EsAdministrador(Rol);
    public bool EsCoordinador => RolesAplicacion.EsCoordinador(Rol);

    /// <summary>Alta, edicion e inactivacion de clientes, salones y recursos.</summary>
    public bool PuedeEditarCatalogos => EsAdministrador;

    /// <summary>Consulta de catalogos: disponible para ambos roles.</summary>
    public bool PuedeConsultarCatalogos => true;

    /// <summary>Registro y edicion de reservas: disponible para ambos roles.</summary>
    public bool PuedeGestionarReservas => true;

    /// <summary>Descuento de linea superior al 10 por ciento (limite duro: 20 por ciento).</summary>
    public bool PuedeAplicarDescuentoAlto => EsAdministrador;

    /// <summary>Acceso a la pantalla de auditoria de correos y analisis de IA.</summary>
    public bool PuedeVerAuditoriaIntegraciones => true;

    public string TextoBarraEstado => $"{NombreCompleto} ({Rol})";
}
