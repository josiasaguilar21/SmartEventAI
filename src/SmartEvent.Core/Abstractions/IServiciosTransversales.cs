using SmartEvent.Core.Dtos;

namespace SmartEvent.Core.Abstractions;

/// <summary>
/// Derivacion de claves. La implementacion usa PBKDF2-HMAC-SHA256 con el salt y las
/// iteraciones que entrega la base. Se abstrae para poder cambiar de algoritmo sin tocar
/// la logica de autenticacion.
/// </summary>
public interface IHasheadorContrasena
{
    /// <summary>Deriva la clave a partir de la contrasena en claro y los parametros publicos.</summary>
    byte[] Derivar(string contrasena, ParametrosHash parametros);
}

/// <summary>
/// Registro de eventos local. La implementacion escribe a archivo con rotacion diaria.
///
/// REGLA: aqui nunca se escriben contrasenas, hashes, cadenas de conexion, claves de API ni
/// cuerpos completos de correo. Los metodos reciben mensajes ya redactados por quien llama.
/// </summary>
public interface IRegistroEventos
{
    void Informacion(string mensaje);
    void Advertencia(string mensaje, Exception? excepcion = null);
    void Error(string mensaje, Exception? excepcion = null);
}
