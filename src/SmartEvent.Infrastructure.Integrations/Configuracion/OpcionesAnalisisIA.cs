namespace SmartEvent.Infrastructure.Integrations.Configuracion;

/// <summary>
/// Configuracion del servicio de analisis con IA.
///
/// La clave se lee de la variable de entorno OPENAI_API_KEY (o de la configuracion local
/// ignorada por Git) y NUNCA se escribe en el codigo, en el log ni en la base de datos.
///
/// POR QUE BaseUrl ES CONFIGURABLE:
/// el contrato que habla este cliente es el de la Responses API con salidas estructuradas.
/// Ese contrato lo implementan varios proveedores de forma compatible, de modo que apuntando
/// BaseUrl a uno u otro se usa el mismo codigo sin cambiar una linea. Eso permite trabajar
/// contra un proveedor con nivel gratuito durante el desarrollo y cambiar de destino sin
/// tocar la aplicacion. Es tambien la razon de que exista el repliegue a /chat/completions:
/// no todos los proveedores compatibles exponen todavia /responses.
/// </summary>
public sealed class OpcionesAnalisisIA
{
    /// <summary>Clave de acceso. Solo vive en memoria durante la ejecucion.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Raiz de la API, sin barra final. Ejemplo: https://api.openai.com/v1</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public string Modelo { get; set; } = "gpt-4o-mini";

    public int TimeoutSegundos { get; set; } = 45;

    /// <summary>Reintentos ante error temporal del proveedor (429 o 5xx). Cero los desactiva.</summary>
    public int MaximoReintentos { get; set; } = 2;

    /// <summary>
    /// Fuerza el uso de /chat/completions sin intentar antes /responses. Util para proveedores
    /// que solo exponen el endpoint clasico y para ahorrarse la llamada de tanteo.
    /// </summary>
    public bool UsarChatCompletions { get; set; }

    public bool EstaCompleta =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        !string.IsNullOrWhiteSpace(Modelo);

    /// <summary>
    /// Lectura desde variables de entorno. OPENAI_API_KEY es el nombre que pide el enunciado;
    /// las demas variables permiten apuntar a un proveedor compatible distinto.
    /// </summary>
    public static OpcionesAnalisisIA DesdeVariablesDeEntorno()
    {
        var opciones = new OpcionesAnalisisIA
        {
            ApiKey = Leer("OPENAI_API_KEY") ?? string.Empty,
            BaseUrl = (Leer("SMARTEVENT_IA_BASEURL") ?? "https://api.openai.com/v1").TrimEnd('/'),
            Modelo = Leer("SMARTEVENT_IA_MODELO") ?? "gpt-4o-mini"
        };

        if (int.TryParse(Leer("SMARTEVENT_IA_TIMEOUT"), out var timeout) && timeout > 0)
        {
            opciones.TimeoutSegundos = timeout;
        }

        if (int.TryParse(Leer("SMARTEVENT_IA_REINTENTOS"), out var reintentos) && reintentos >= 0)
        {
            opciones.MaximoReintentos = reintentos;
        }

        if (bool.TryParse(Leer("SMARTEVENT_IA_CHAT_COMPLETIONS"), out var usarChat))
        {
            opciones.UsarChatCompletions = usarChat;
        }

        return opciones;
    }

    private static string? Leer(string nombre)
    {
        var valor = Environment.GetEnvironmentVariable(nombre);
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
