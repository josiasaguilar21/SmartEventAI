namespace SmartEvent.Core.Entities;

/// <summary>
/// Usuario del sistema tal como lo expone la base de datos hacia la aplicacion.
/// IMPORTANTE: esta clase NO tiene PasswordHash ni PasswordSalt. El hash jamas sale de
/// SQL Server: la comparacion ocurre dentro de seg.sp_Usuario_Autenticar. Lo unico que viaja
/// hacia el cliente es el salt (que no es secreto) y solo mientras se deriva la clave.
/// </summary>
public sealed class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int IdRol { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public bool Bloqueado { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; }
}
