namespace SmartEvent.Infrastructure.Integrations.Configuracion;

/// <summary>
/// Configuracion del servidor de correo.
///
/// NINGUN valor de esta clase esta escrito en el codigo ni se versiona. Se rellena en tiempo
/// de ejecucion desde variables de entorno o desde el appsettings.json local, que el
/// .gitignore excluye. El repositorio solo contiene appsettings.example.json con valores
/// ficticios.
///
/// La contrasena vive aqui y en ningun otro sitio: no se escribe en el log, no se pasa a la
/// base de datos y no aparece en los mensajes de error, que se redactan aparte.
/// </summary>
public sealed class OpcionesSmtp
{
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;

    /// <summary>Nombre visible del remitente.</summary>
    public string RemitenteNombre { get; set; } = "SmartEvent AI";

    /// <summary>Direccion del remitente. Si queda vacia se usa <see cref="Usuario"/>.</summary>
    public string RemitenteCorreo { get; set; } = string.Empty;

    /// <summary>
    /// Verdadero para SSL implicito (normalmente puerto 465); falso para STARTTLS (puerto 587).
    /// En ambos casos la conexion viaja cifrada: nunca se usa texto plano.
    /// </summary>
    public bool UsarSslImplicito { get; set; }

    public int TimeoutSegundos { get; set; } = 20;

    /// <summary>
    /// Direccion a la que se redirigen TODOS los correos durante las pruebas. Cuando tiene
    /// valor, ningun mensaje llega a un cliente real: es la forma segura de demostrar el
    /// envio con datos ficticios sin escribir a terceros por error.
    /// </summary>
    public string? RedireccionPruebas { get; set; }

    /// <summary>Hay configuracion suficiente para intentar un envio.</summary>
    public bool EstaCompleta =>
        !string.IsNullOrWhiteSpace(Host) &&
        Puerto is > 0 and <= 65535 &&
        !string.IsNullOrWhiteSpace(Usuario) &&
        !string.IsNullOrWhiteSpace(Contrasena);

    public string CorreoRemitenteEfectivo =>
        string.IsNullOrWhiteSpace(RemitenteCorreo) ? Usuario : RemitenteCorreo;

    /// <summary>
    /// Lectura desde variables de entorno. Es la via recomendada en el README porque no deja
    /// rastro en ningun archivo del proyecto.
    /// </summary>
    public static OpcionesSmtp DesdeVariablesDeEntorno()
    {
        var opciones = new OpcionesSmtp
        {
            Host = Leer("SMARTEVENT_SMTP_HOST") ?? string.Empty,
            Usuario = Leer("SMARTEVENT_SMTP_USUARIO") ?? string.Empty,
            Contrasena = Leer("SMARTEVENT_SMTP_CLAVE") ?? string.Empty,
            RemitenteNombre = Leer("SMARTEVENT_SMTP_REMITENTE_NOMBRE") ?? "SmartEvent AI",
            RemitenteCorreo = Leer("SMARTEVENT_SMTP_REMITENTE") ?? string.Empty,
            RedireccionPruebas = Leer("SMARTEVENT_SMTP_REDIRECCION_PRUEBAS")
        };

        if (int.TryParse(Leer("SMARTEVENT_SMTP_PUERTO"), out var puerto))
        {
            opciones.Puerto = puerto;
        }

        if (bool.TryParse(Leer("SMARTEVENT_SMTP_SSL"), out var ssl))
        {
            opciones.UsarSslImplicito = ssl;
        }

        if (int.TryParse(Leer("SMARTEVENT_SMTP_TIMEOUT"), out var timeout) && timeout > 0)
        {
            opciones.TimeoutSegundos = timeout;
        }

        return opciones;
    }

    private static string? Leer(string nombre)
    {
        var valor = Environment.GetEnvironmentVariable(nombre);
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
