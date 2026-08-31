using Microsoft.Extensions.Configuration;
using SmartEvent.Core.Exceptions;
using SmartEvent.Infrastructure.Integrations.Configuracion;

namespace SmartEvent.UI.Configuracion;

/// <summary>Configuracion completa de la aplicacion, ya resuelta al arrancar.</summary>
public sealed class ConfiguracionAplicacion
{
    public required string CadenaConexion { get; init; }
    public required OpcionesSmtp Smtp { get; init; }
    public required OpcionesAnalisisIA AnalisisIA { get; init; }
    public string? CarpetaRegistro { get; init; }
}

/// <summary>
/// Resuelve de donde sale cada valor de configuracion.
///
/// ORDEN DE PRIORIDAD, de menor a mayor:
///   1. appsettings.json           (archivo local, excluido del repositorio)
///   2. User Secrets               (fuera del arbol del proyecto, imposible de subir por error)
///   3. Variables de entorno       (la via recomendada; no deja rastro en ningun archivo)
///
/// Las variables de entorno ganan siempre. Eso permite que el repositorio contenga un
/// appsettings.example.json con valores ficticios y que cada equipo defina los reales por
/// fuera, sin tocar el codigo ni arriesgarse a subir una clave.
///
/// Ningun valor sensible se escribe en el log ni se muestra en pantalla en ningun momento.
/// </summary>
public static class CargadorConfiguracion
{
    /// <summary>Nombre de la variable de entorno con la cadena de conexion.</summary>
    public const string VariableCadenaConexion = "SMARTEVENT_CONNECTION";

    public static ConfiguracionAplicacion Cargar()
    {
        var constructor = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(typeof(CargadorConfiguracion).Assembly, optional: true)
            .AddEnvironmentVariables();

        var configuracion = constructor.Build();

        return new ConfiguracionAplicacion
        {
            CadenaConexion = ResolverCadenaConexion(configuracion),
            Smtp = ResolverSmtp(configuracion),
            AnalisisIA = ResolverAnalisisIA(configuracion),
            CarpetaRegistro = configuracion["Registro:Carpeta"]
        };
    }

    private static string ResolverCadenaConexion(IConfiguration configuracion)
    {
        // La variable de entorno tiene prioridad sobre el archivo.
        var cadena = Environment.GetEnvironmentVariable(VariableCadenaConexion);

        if (string.IsNullOrWhiteSpace(cadena))
        {
            cadena = configuracion.GetConnectionString("SmartEvent");
        }

        if (string.IsNullOrWhiteSpace(cadena))
        {
            throw new ConfiguracionException(
                "No se encontro la cadena de conexion a SQL Server.\n\n" +
                "Configure una de estas dos opciones y vuelva a iniciar la aplicacion:\n\n" +
                $"  1. La variable de entorno {VariableCadenaConexion}\n" +
                "  2. La seccion ConnectionStrings:SmartEvent de appsettings.json\n\n" +
                "Puede partir del archivo appsettings.example.json incluido junto al ejecutable.");
        }

        return cadena.Trim();
    }

    private static OpcionesSmtp ResolverSmtp(IConfiguration configuracion)
    {
        var archivo = configuracion.GetSection("Smtp").Get<OpcionesSmtp>() ?? new OpcionesSmtp();
        var entorno = OpcionesSmtp.DesdeVariablesDeEntorno();

        return new OpcionesSmtp
        {
            Host = Preferir(entorno.Host, archivo.Host),
            Puerto = entorno.Puerto != 587 ? entorno.Puerto : (archivo.Puerto > 0 ? archivo.Puerto : 587),
            Usuario = Preferir(entorno.Usuario, archivo.Usuario),
            Contrasena = Preferir(entorno.Contrasena, archivo.Contrasena),
            RemitenteNombre = Preferir(entorno.RemitenteNombre, archivo.RemitenteNombre, "SmartEvent AI"),
            RemitenteCorreo = Preferir(entorno.RemitenteCorreo, archivo.RemitenteCorreo),
            UsarSslImplicito = entorno.UsarSslImplicito || archivo.UsarSslImplicito,
            TimeoutSegundos = entorno.TimeoutSegundos != 20 ? entorno.TimeoutSegundos
                            : (archivo.TimeoutSegundos > 0 ? archivo.TimeoutSegundos : 20),
            RedireccionPruebas = Preferir(entorno.RedireccionPruebas, archivo.RedireccionPruebas)
        };
    }

    private static OpcionesAnalisisIA ResolverAnalisisIA(IConfiguration configuracion)
    {
        var archivo = configuracion.GetSection("AnalisisIA").Get<OpcionesAnalisisIA>() ?? new OpcionesAnalisisIA();
        var entorno = OpcionesAnalisisIA.DesdeVariablesDeEntorno();

        return new OpcionesAnalisisIA
        {
            ApiKey = Preferir(entorno.ApiKey, archivo.ApiKey),
            BaseUrl = Preferir(
                entorno.BaseUrl == "https://api.openai.com/v1" ? null : entorno.BaseUrl,
                archivo.BaseUrl,
                "https://api.openai.com/v1").TrimEnd('/'),
            Modelo = Preferir(
                entorno.Modelo == "gpt-4o-mini" ? null : entorno.Modelo,
                archivo.Modelo,
                "gpt-4o-mini"),
            TimeoutSegundos = entorno.TimeoutSegundos != 45 ? entorno.TimeoutSegundos
                            : (archivo.TimeoutSegundos > 0 ? archivo.TimeoutSegundos : 45),
            MaximoReintentos = entorno.MaximoReintentos != 2 ? entorno.MaximoReintentos : archivo.MaximoReintentos,
            UsarChatCompletions = entorno.UsarChatCompletions || archivo.UsarChatCompletions
        };
    }

    /// <summary>Devuelve el primer valor con contenido, en el orden en que se reciben.</summary>
    private static string Preferir(string? preferido, string? alternativo, string valorPorDefecto = "")
    {
        if (!string.IsNullOrWhiteSpace(preferido)) return preferido.Trim();
        if (!string.IsNullOrWhiteSpace(alternativo)) return alternativo.Trim();
        return valorPorDefecto;
    }
}
