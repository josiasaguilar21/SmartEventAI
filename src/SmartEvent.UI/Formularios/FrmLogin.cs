using SmartEvent.Core.Enums;
using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Pantalla de autenticacion.
///
/// El formulario NO sabe como se verifica una contrasena: entrega usuario y clave al servicio
/// de aplicacion y reacciona al resultado. No hay hash, ni SQL, ni cadena de conexion aqui.
///
/// El bloqueo temporal se muestra con una cuenta atras basada en un Timer de Windows Forms.
/// Es importante entender que ese contador es solo la parte visible: el bloqueo REAL esta
/// persistido en la columna BloqueadoHasta de seg.Usuario, de modo que cerrar la aplicacion y
/// volver a abrirla no lo evita.
/// </summary>
public partial class FrmLogin : Form
{
    private readonly ContenedorServicios _servicios;
    private readonly CancellationTokenSource _cancelacion = new();

    private int _segundosRestantesBloqueo;

    public FrmLogin(ContenedorServicios servicios)
    {
        _servicios = servicios ?? throw new ArgumentNullException(nameof(servicios));

        InitializeComponent();
    }

    private async void FrmLogin_Load(object sender, EventArgs e)
    {
        lblMensaje.Text = string.Empty;
        txtUsuario.Focus();

        // Comprobacion de conectividad en segundo plano: informa al usuario antes de que
        // intente entrar, en lugar de dejar que descubra el problema al pulsar Ingresar.
        await ComprobarConexionAsync();
    }

    private async Task ComprobarConexionAsync()
    {
        lblConexion.ForeColor = SystemColors.GrayText;
        lblConexion.Text = "Verificando conexion...";

        try
        {
            var hayConexion = await _servicios.FabricaConexiones
                .ProbarConexionAsync(_cancelacion.Token)
                .ConfigureAwait(true);

            if (IsDisposed)
            {
                return;
            }

            if (hayConexion)
            {
                lblConexion.ForeColor = SystemColors.GrayText;
                lblConexion.Text = $"Conectado a {_servicios.FabricaConexiones.DescripcionConexion}";
            }
            else
            {
                lblConexion.ForeColor = Color.FromArgb(179, 38, 30);
                lblConexion.Text = "Sin conexion con la base de datos";
            }
        }
        catch (OperationCanceledException)
        {
            // El formulario se cerro mientras se comprobaba: no hay nada que mostrar.
        }
    }

    private async void btnIngresar_Click(object sender, EventArgs e)
    {
        await IniciarSesionAsync();
    }

    private async Task IniciarSesionAsync()
    {
        var usuario = txtUsuario.Text.Trim();
        var contrasena = txtContrasena.Text;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(contrasena))
        {
            MostrarMensaje("Escriba su usuario y su contrasena para continuar.");
            (string.IsNullOrWhiteSpace(usuario) ? txtUsuario : txtContrasena).Focus();
            return;
        }

        lblMensaje.Text = string.Empty;
        prgActividad.Visible = true;

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Autenticacion.IniciarSesionAsync(usuario, contrasena, ct),
            _cancelacion.Token,
            "inicio de sesion",
            btnIngresar, txtUsuario, txtContrasena, chkVerContrasena);

        prgActividad.Visible = false;

        if (resultado is null)
        {
            // Hubo un error tecnico o de conexion; ManejadorErrores ya informo al usuario.
            return;
        }

        if (resultado.EsCorrecto)
        {
            // La contrasena se limpia en cuanto deja de ser necesaria.
            txtContrasena.Clear();

            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        MostrarMensaje(resultado.Mensaje);
        txtContrasena.Clear();

        if (resultado.Resultado == ResultadoAutenticacion.CuentaBloqueada && resultado.SegundosBloqueo > 0)
        {
            IniciarCuentaAtras(resultado.SegundosBloqueo);
        }
        else
        {
            txtContrasena.Focus();
        }
    }

    /// <summary>
    /// Deshabilita el acceso durante el bloqueo y muestra el tiempo restante.
    /// Se usa un Timer y no una espera: bloquear el hilo congelaria toda la ventana.
    /// </summary>
    private void IniciarCuentaAtras(int segundos)
    {
        _segundosRestantesBloqueo = segundos;

        btnIngresar.Enabled = false;
        txtUsuario.Enabled = false;
        txtContrasena.Enabled = false;

        ActualizarTextoBloqueo();
        tmrBloqueo.Start();
    }

    private void tmrBloqueo_Tick(object sender, EventArgs e)
    {
        _segundosRestantesBloqueo--;

        if (_segundosRestantesBloqueo <= 0)
        {
            tmrBloqueo.Stop();

            btnIngresar.Enabled = true;
            txtUsuario.Enabled = true;
            txtContrasena.Enabled = true;

            lblMensaje.Text = "Ya puede intentar iniciar sesion nuevamente.";
            txtContrasena.Focus();
            return;
        }

        ActualizarTextoBloqueo();
    }

    private void ActualizarTextoBloqueo()
    {
        var minutos = _segundosRestantesBloqueo / 60;
        var segundos = _segundosRestantesBloqueo % 60;

        lblMensaje.Text = "Cuenta bloqueada temporalmente por intentos fallidos. " +
                          $"Podra reintentar en {minutos:00}:{segundos:00}.";
    }

    private void MostrarMensaje(string mensaje) => lblMensaje.Text = mensaje;

    private void chkVerContrasena_CheckedChanged(object sender, EventArgs e)
    {
        txtContrasena.UseSystemPasswordChar = !chkVerContrasena.Checked;
    }

    private void btnSalir_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void FrmLogin_FormClosed(object sender, FormClosedEventArgs e)
    {
        // Cancela cualquier operacion pendiente para que ninguna tarea sobreviva al formulario.
        _cancelacion.Cancel();
        _cancelacion.Dispose();
    }
}
