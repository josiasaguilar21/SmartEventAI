using SmartEvent.Application.Servicios;
using SmartEvent.Application.Sesion;
using SmartEvent.Core.Abstractions;
using SmartEvent.Infrastructure.Data.Conexion;
using SmartEvent.Infrastructure.Data.Registro;
using SmartEvent.Infrastructure.Data.Repositorios;
using SmartEvent.Infrastructure.Data.Seguridad;
using SmartEvent.Infrastructure.Integrations.Correo;
using SmartEvent.Infrastructure.Integrations.IA;
using SmartEvent.UI.Configuracion;

namespace SmartEvent.UI.Composicion;

/// <summary>
/// RAIZ DE COMPOSICION de la aplicacion: el unico lugar donde se decide que implementacion
/// concreta recibe cada interfaz.
///
/// La inyeccion de dependencias es MANUAL, que es una de las dos opciones que admite el
/// enunciado. Con cinco proyectos y una docena de servicios, un contenedor anadiria una
/// dependencia mas sin resolver ningun problema real: aqui se ve de un vistazo el grafo
/// completo de la aplicacion, y cualquier ciclo o dependencia indebida salta a la vista.
///
/// Los formularios reciben SOLO servicios de SmartEvent.Application. Nunca ven un repositorio,
/// una conexion ni un cliente HTTP.
/// </summary>
public sealed class ContenedorServicios : IDisposable
{
    private readonly ServicioAnalisisIAHttp _servicioIA;
    private bool _liberado;

    public ContenedorServicios(ConfiguracionAplicacion configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);

        Configuracion = configuracion;

        // --- servicios transversales -------------------------------------------------------
        Registro = new RegistroEventosArchivo(configuracion.CarpetaRegistro);
        Contexto = new ContextoSesion();

        // --- infraestructura de datos ------------------------------------------------------
        FabricaConexiones = new FabricaConexiones(configuracion.CadenaConexion, Registro);

        var hasheador = new HasheadorPbkdf2();
        var usuarios = new UsuarioRepositorio(FabricaConexiones, Registro);
        var clientes = new ClienteRepositorio(FabricaConexiones, Registro);
        var salones = new SalonRepositorio(FabricaConexiones, Registro);
        var recursos = new RecursoRepositorio(FabricaConexiones, Registro);
        var reservas = new ReservaRepositorio(FabricaConexiones, Registro);
        var auditoria = new AuditoriaIntegracionesRepositorio(FabricaConexiones, Registro);

        // --- integraciones externas --------------------------------------------------------
        var correo = new ServicioCorreoMailKit(configuracion.Smtp, Registro);

        // El cliente de IA mantiene un HttpClient reutilizable durante toda la sesion: crear
        // uno por llamada agotaria los sockets del sistema.
        _servicioIA = new ServicioAnalisisIAHttp(configuracion.AnalisisIA, Registro);

        CorreoConfigurado = correo.EstaConfigurado;
        IAConfigurada = _servicioIA.EstaConfigurado;
        ModeloIA = _servicioIA.Modelo;

        // --- servicios de aplicacion -------------------------------------------------------
        Autenticacion = new ServicioAutenticacion(usuarios, hasheador, Contexto, Registro);
        Catalogos = new ServicioCatalogos(clientes, salones, recursos, Contexto);
        Auditoria = new ServicioAuditoria(auditoria, Contexto, Registro);

        Reservas = new ServicioReservas(
            reservas, salones, recursos, auditoria,
            correo, _servicioIA, Contexto, Registro);

        Registro.Informacion(
            $"Aplicacion iniciada. Base: {FabricaConexiones.DescripcionConexion}. " +
            $"Correo configurado: {CorreoConfigurado}. IA configurada: {IAConfigurada} ({ModeloIA}).");
    }

    public ConfiguracionAplicacion Configuracion { get; }

    public IRegistroEventos Registro { get; }

    public IContextoSesion Contexto { get; }

    public IFabricaConexiones FabricaConexiones { get; }

    public IServicioAutenticacion Autenticacion { get; }

    public IServicioCatalogos Catalogos { get; }

    public IServicioReservas Reservas { get; }

    public IServicioAuditoria Auditoria { get; }

    /// <summary>Permite que la interfaz avise si el correo no esta configurado, sin exponer valores.</summary>
    public bool CorreoConfigurado { get; }

    public bool IAConfigurada { get; }

    public string ModeloIA { get; }

    public void Dispose()
    {
        if (_liberado)
        {
            return;
        }

        _liberado = true;

        Registro.Informacion("Aplicacion finalizada.");
        _servicioIA.Dispose();
    }
}
