using System.Security.Cryptography;
using System.Text;
using SmartEvent.Core.Abstractions;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Exceptions;

namespace SmartEvent.Infrastructure.Data.Seguridad;

/// <summary>
/// Derivacion de claves con PBKDF2-HMAC-SHA256.
///
/// POR QUE PBKDF2 Y NO UN SIMPLE SHA-256:
/// un hash rapido es exactamente lo que necesita quien intenta un ataque por diccionario.
/// PBKDF2 aplica la funcion 120.000 veces con un salt distinto por usuario, de modo que
/// probar contrasenas resulta costoso y dos usuarios con la misma clave producen hashes
/// distintos (no se pueden usar tablas precalculadas).
///
/// POR QUE AQUI Y NO EN SQL SERVER:
/// T-SQL no dispone de PBKDF2. La alternativa seria enviar la contrasena en claro al motor.
/// El diseno elegido es mejor: la contrasena NUNCA sale del proceso de la aplicacion, viaja
/// ya derivada, y la comparacion del hash ocurre dentro del procedimiento almacenado, que
/// tampoco devuelve el hash almacenado.
/// </summary>
public sealed class HasheadorPbkdf2 : IHasheadorContrasena
{
    /// <summary>Longitud de la clave derivada, en bytes. Coincide con VARBINARY(64) de la tabla.</summary>
    private const int LongitudClaveBytes = 32;

    private const string AlgoritmoEsperado = "PBKDF2-SHA256";

    public byte[] Derivar(string contrasena, ParametrosHash parametros)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        if (string.IsNullOrEmpty(contrasena))
        {
            // Aun asi se deriva sobre una cadena vacia para no introducir una diferencia de
            // tiempo observable entre "sin contrasena" y "contrasena incorrecta".
            contrasena = string.Empty;
        }

        if (!string.Equals(parametros.Algoritmo, AlgoritmoEsperado, StringComparison.OrdinalIgnoreCase))
        {
            throw new ErrorTecnicoException(
                $"El usuario tiene configurado el algoritmo '{parametros.Algoritmo}', " +
                $"que esta aplicacion no implementa. Esperado: {AlgoritmoEsperado}.");
        }

        if (parametros.Salt.Length == 0)
        {
            throw new ErrorTecnicoException("No se recibio el salt necesario para derivar la clave.");
        }

        if (parametros.Iteraciones < 10_000)
        {
            throw new ErrorTecnicoException(
                $"El numero de iteraciones configurado ({parametros.Iteraciones}) es inseguro.");
        }

        return Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(contrasena),
            salt: parametros.Salt,
            iterations: parametros.Iteraciones,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: LongitudClaveBytes);
    }
}
