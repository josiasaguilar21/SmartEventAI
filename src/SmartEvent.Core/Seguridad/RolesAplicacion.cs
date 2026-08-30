namespace SmartEvent.Core.Seguridad;

/// <summary>
/// Nombres de rol tal como estan sembrados en seg.Rol. Se centralizan como constantes para no
/// repartir literales por el codigo y para que un error de escritura falle al compilar, no en
/// tiempo de ejecucion.
/// </summary>
public static class RolesAplicacion
{
    public const string Administrador = "ADMINISTRADOR";
    public const string Coordinador = "COORDINADOR";

    public static bool EsAdministrador(string? rol) =>
        string.Equals(rol?.Trim(), Administrador, StringComparison.OrdinalIgnoreCase);

    public static bool EsCoordinador(string? rol) =>
        string.Equals(rol?.Trim(), Coordinador, StringComparison.OrdinalIgnoreCase);
}
