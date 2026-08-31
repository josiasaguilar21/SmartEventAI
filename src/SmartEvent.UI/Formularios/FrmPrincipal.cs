using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Contenedor MDI de la aplicacion.
///
/// Tres responsabilidades, y ninguna mas:
///   1. Presentar el menu ADAPTADO AL ROL del usuario autenticado.
///   2. Abrir los formularios hijos evitando duplicados.
///   3. Mostrar el estado de la sesion y de la conectividad en la barra inferior.
///
/// No contiene logica de negocio: cada opcion del menu abre un formulario que se ocupa de lo
/// suyo. La adaptacion por rol es de USABILIDAD, no de seguridad: la autorizacion real la
/// aplican los servicios y, en ultima instancia, los procedimientos almacenados.
/// </summary>
public partial class FrmPrincipal : Form
{
    private readonly ContenedorServicios _servicios;
    private readonly CancellationTokenSource _cancelacion = new();

    public FrmPrincipal(ContenedorServicios servicios)
    {
        _servicios = servicios ?? throw new ArgumentNullException(nameof(servicios));

        InitializeComponent();
    }

    private async void FrmPrincipal_Load(object sender, EventArgs e)
    {
        AplicarPermisos();
        MostrarEstadoIntegraciones();
        ActualizarFechaHora();

        tmrEstado.Start();

        await ActualizarEstadoConexionAsync();

        // Se abre directamente la consulta de reservas: es la pantalla de trabajo habitual.
        AbrirConsultaReservas();
    }

    /// <summary>
    /// Adapta el menu al rol. El COORDINADOR puede consultar los catalogos pero no modificarlos,
    /// asi que la opcion sigue disponible y es el propio formulario el que deshabilita la edicion.
    /// </summary>
    private void AplicarPermisos()
    {
        var sesion = _servicios.Contexto.Actual;

        if (sesion is null)
        {
            // No deberia ocurrir: sin sesion no se llega hasta aqui. Si pasara, se cierra.
            Close();
            return;
        }

        lblUsuarioConectado.Text = $"Usuario: {sesion.TextoBarraEstado}";

        mnuCatalogos.Enabled = sesion.PuedeConsultarCatalogos;
        mnuReservas.Enabled = sesion.PuedeGestionarReservas;
        mnuAuditoria.Enabled = sesion.PuedeVerAuditoriaIntegraciones;

        Text = $"SmartEvent AI - {sesion.NombreCompleto} ({sesion.Rol})";
    }

    private void MostrarEstadoIntegraciones()
    {
        var correo = _servicios.CorreoConfigurado ? "Correo: configurado" : "Correo: sin configurar";
        var ia = _servicios.IAConfigurada ? $"IA: {_servicios.ModeloIA}" : "IA: sin configurar";

        lblEstadoIntegraciones.Text = $"{correo}  |  {ia}";

        lblEstadoIntegraciones.ForeColor = _servicios.CorreoConfigurado && _servicios.IAConfigurada
            ? SystemColors.ControlText
            : Color.FromArgb(150, 90, 0);
    }

    private async Task ActualizarEstadoConexionAsync()
    {
        try
        {
            var conectado = await _servicios.FabricaConexiones
                .ProbarConexionAsync(_cancelacion.Token)
                .ConfigureAwait(true);

            if (IsDisposed)
            {
                return;
            }

            lblEstadoConexion.Text = conectado
                ? $"Conectado a {_servicios.FabricaConexiones.DescripcionConexion}"
                : "Sin conexion con la base de datos";

            lblEstadoConexion.ForeColor = conectado
                ? SystemColors.ControlText
                : Color.FromArgb(179, 38, 30);
        }
        catch (OperationCanceledException)
        {
            // La aplicacion se esta cerrando.
        }
    }

    private void ActualizarFechaHora() =>
        lblFechaHora.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

    private async void tmrEstado_Tick(object sender, EventArgs e)
    {
        ActualizarFechaHora();
        await ActualizarEstadoConexionAsync();
    }

    // ---------------------------------------------------------------- apertura de pantallas

    /// <summary>
    /// Abre un formulario hijo reutilizando el que ya este abierto.
    ///
    /// Sin esta comprobacion, pulsar dos veces la misma opcion abriria dos copias de la misma
    /// pantalla, cada una con su propia consulta a la base y su propio estado. Es un defecto
    /// habitual en aplicaciones MDI y aqui esta resuelto en un solo lugar.
    /// </summary>
    private T AbrirHijo<T>(Func<T> fabrica) where T : Form
    {
        var existente = MdiChildren.OfType<T>().FirstOrDefault();

        if (existente is not null && !existente.IsDisposed)
        {
            if (existente.WindowState == FormWindowState.Minimized)
            {
                existente.WindowState = FormWindowState.Normal;
            }

            existente.Activate();
            return existente;
        }

        var formulario = fabrica();
        formulario.MdiParent = this;

        // Los hijos se abren maximizados: en un contenedor MDI a pantalla completa, una ventana
        // pequena desperdicia el espacio util de las grillas.
        formulario.WindowState = FormWindowState.Maximized;
        formulario.Show();

        return formulario;
    }

    private void mnuClientes_Click(object sender, EventArgs e) => AbrirCatalogos(FrmCatalogos.Pestana.Clientes);

    private void mnuSalones_Click(object sender, EventArgs e) => AbrirCatalogos(FrmCatalogos.Pestana.Salones);

    private void mnuRecursos_Click(object sender, EventArgs e) => AbrirCatalogos(FrmCatalogos.Pestana.Recursos);

    private void AbrirCatalogos(FrmCatalogos.Pestana pestana)
    {
        var formulario = AbrirHijo(() => new FrmCatalogos(_servicios));
        formulario.MostrarPestana(pestana);
    }

    private void mnuNuevaReserva_Click(object sender, EventArgs e)
    {
        // La edicion de reservas SI admite varias ventanas: es razonable trabajar en dos
        // reservas a la vez. Por eso no pasa por AbrirHijo.
        var formulario = new FrmReservaEdicion(_servicios, idReserva: null)
        {
            MdiParent = this,
            WindowState = FormWindowState.Maximized
        };

        formulario.Show();
    }

    private void mnuConsultarReservas_Click(object sender, EventArgs e) => AbrirConsultaReservas();

    private void AbrirConsultaReservas() => AbrirHijo(() => new FrmReservasConsulta(_servicios));

    private void mnuAuditoriaIntegraciones_Click(object sender, EventArgs e) =>
        AbrirHijo(() => new FrmAuditoriaIntegraciones(_servicios));

    // ------------------------------------------------------------------ ventana y sesion

    private void mnuCascada_Click(object sender, EventArgs e) => LayoutMdi(MdiLayout.Cascade);

    private void mnuMosaicoHorizontal_Click(object sender, EventArgs e) => LayoutMdi(MdiLayout.TileHorizontal);

    private void mnuCerrarTodo_Click(object sender, EventArgs e)
    {
        // Se recorre una copia: cerrar un hijo modifica la coleccion original.
        foreach (var hijo in MdiChildren.ToArray())
        {
            hijo.Close();
        }
    }

    private void mnuCerrarSesion_Click(object sender, EventArgs e)
    {
        if (!ManejadorErrores.Confirmar(this,
                "Se cerraran todas las ventanas abiertas.\n\nDesea cerrar la sesion?",
                "Cerrar sesion"))
        {
            return;
        }

        foreach (var hijo in MdiChildren.ToArray())
        {
            hijo.Close();
        }

        // Si alguna ventana cancelo su cierre (por ejemplo, una reserva con cambios sin
        // guardar), la sesion no se cierra.
        if (MdiChildren.Length > 0)
        {
            return;
        }

        _servicios.Autenticacion.CerrarSesion();

        using var login = new FrmLogin(_servicios);

        if (login.ShowDialog(this) != DialogResult.OK)
        {
            Close();
            return;
        }

        AplicarPermisos();
        AbrirConsultaReservas();
    }

    private void mnuSalir_Click(object sender, EventArgs e) => Close();

    private void mnuAcercaDe_Click(object sender, EventArgs e)
    {
        var version = typeof(FrmPrincipal).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        MessageBox.Show(this,
            "SmartEvent AI\n" +
            "Sistema de reservas de salones, recursos y comunicacion para eventos\n\n" +
            $"Version {version}\n" +
            "Autor: Josias Aguilar\n\n" +
            "Arquitectura en capas sobre .NET 8 y SQL Server.\n" +
            "Integraciones: SMTP con MailKit y analisis con IA mediante salidas estructuradas.",
            "Acerca de SmartEvent AI", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void FrmPrincipal_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing &&
            !ManejadorErrores.Confirmar(this, "Desea salir de SmartEvent AI?", "Salir"))
        {
            e.Cancel = true;
            return;
        }

        tmrEstado.Stop();
        _cancelacion.Cancel();
        _cancelacion.Dispose();
    }
}
