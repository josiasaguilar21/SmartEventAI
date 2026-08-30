using SmartEvent.Core.Enums;

namespace SmartEvent.Core.Dtos;

/// <summary>
/// Resultado del intento de autenticacion. El mensaje viene redactado desde el procedimiento
/// almacenado y es deliberadamente generico ante credenciales incorrectas o usuario
/// inexistente, para no permitir enumerar cuentas validas.
/// </summary>
public sealed class AutenticacionResultado
{
    public required ResultadoAutenticacion Resultado { get; init; }
    public required string Mensaje { get; init; }

    /// <summary>Segundos que restan de bloqueo cuando <see cref="Resultado"/> es CuentaBloqueada.</summary>
    public int SegundosBloqueo { get; init; }

    /// <summary>Datos de sesion; solo viene informado cuando la autenticacion fue correcta.</summary>
    public SesionUsuario? Sesion { get; init; }

    public bool EsCorrecto => Resultado == ResultadoAutenticacion.Correcto && Sesion is not null;

    public static AutenticacionResultado Fallido(ResultadoAutenticacion resultado, string mensaje, int segundosBloqueo = 0) =>
        new() { Resultado = resultado, Mensaje = mensaje, SegundosBloqueo = segundosBloqueo };

    public static AutenticacionResultado Correcto(SesionUsuario sesion, string mensaje) =>
        new() { Resultado = ResultadoAutenticacion.Correcto, Mensaje = mensaje, Sesion = sesion };
}

/// <summary>
/// Parametros publicos necesarios para derivar la clave con PBKDF2. El salt no es secreto;
/// se transporta para que el hash se calcule en el cliente y solo el resultado viaje a SQL.
/// </summary>
public sealed class ParametrosHash
{
    public required byte[] Salt { get; init; }
    public required int Iteraciones { get; init; }
    public required string Algoritmo { get; init; }
}
