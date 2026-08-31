namespace SmartEvent.UI.Formularios;

partial class FrmPrincipal
{
    private System.ComponentModel.IContainer components = null;

    private MenuStrip mnuPrincipal;
    private ToolStripMenuItem mnuArchivo;
    private ToolStripMenuItem mnuCerrarSesion;
    private ToolStripSeparator sepArchivo;
    private ToolStripMenuItem mnuSalir;
    private ToolStripMenuItem mnuCatalogos;
    private ToolStripMenuItem mnuClientes;
    private ToolStripMenuItem mnuSalones;
    private ToolStripMenuItem mnuRecursos;
    private ToolStripMenuItem mnuReservas;
    private ToolStripMenuItem mnuNuevaReserva;
    private ToolStripMenuItem mnuConsultarReservas;
    private ToolStripMenuItem mnuAuditoria;
    private ToolStripMenuItem mnuAuditoriaIntegraciones;
    private ToolStripMenuItem mnuVentana;
    private ToolStripMenuItem mnuCascada;
    private ToolStripMenuItem mnuMosaicoHorizontal;
    private ToolStripMenuItem mnuCerrarTodo;
    private ToolStripMenuItem mnuAyuda;
    private ToolStripMenuItem mnuAcercaDe;

    private StatusStrip stbEstado;
    private ToolStripStatusLabel lblUsuarioConectado;
    private ToolStripStatusLabel lblSeparador1;
    private ToolStripStatusLabel lblEstadoConexion;
    private ToolStripStatusLabel lblSeparador2;
    private ToolStripStatusLabel lblEstadoIntegraciones;
    private ToolStripStatusLabel lblRelleno;
    private ToolStripStatusLabel lblFechaHora;

    private System.Windows.Forms.Timer tmrEstado;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.mnuPrincipal = new MenuStrip();
        this.mnuArchivo = new ToolStripMenuItem();
        this.mnuCerrarSesion = new ToolStripMenuItem();
        this.sepArchivo = new ToolStripSeparator();
        this.mnuSalir = new ToolStripMenuItem();
        this.mnuCatalogos = new ToolStripMenuItem();
        this.mnuClientes = new ToolStripMenuItem();
        this.mnuSalones = new ToolStripMenuItem();
        this.mnuRecursos = new ToolStripMenuItem();
        this.mnuReservas = new ToolStripMenuItem();
        this.mnuNuevaReserva = new ToolStripMenuItem();
        this.mnuConsultarReservas = new ToolStripMenuItem();
        this.mnuAuditoria = new ToolStripMenuItem();
        this.mnuAuditoriaIntegraciones = new ToolStripMenuItem();
        this.mnuVentana = new ToolStripMenuItem();
        this.mnuCascada = new ToolStripMenuItem();
        this.mnuMosaicoHorizontal = new ToolStripMenuItem();
        this.mnuCerrarTodo = new ToolStripMenuItem();
        this.mnuAyuda = new ToolStripMenuItem();
        this.mnuAcercaDe = new ToolStripMenuItem();
        this.stbEstado = new StatusStrip();
        this.lblUsuarioConectado = new ToolStripStatusLabel();
        this.lblSeparador1 = new ToolStripStatusLabel();
        this.lblEstadoConexion = new ToolStripStatusLabel();
        this.lblSeparador2 = new ToolStripStatusLabel();
        this.lblEstadoIntegraciones = new ToolStripStatusLabel();
        this.lblRelleno = new ToolStripStatusLabel();
        this.lblFechaHora = new ToolStripStatusLabel();
        this.tmrEstado = new System.Windows.Forms.Timer(this.components);
        this.mnuPrincipal.SuspendLayout();
        this.stbEstado.SuspendLayout();
        this.SuspendLayout();
        //
        // mnuPrincipal
        //
        this.mnuPrincipal.Items.AddRange(new ToolStripItem[] {
            this.mnuArchivo, this.mnuCatalogos, this.mnuReservas,
            this.mnuAuditoria, this.mnuVentana, this.mnuAyuda});
        this.mnuPrincipal.Location = new System.Drawing.Point(0, 0);
        this.mnuPrincipal.MdiWindowListItem = this.mnuVentana;
        this.mnuPrincipal.Name = "mnuPrincipal";
        this.mnuPrincipal.Size = new System.Drawing.Size(1024, 24);
        this.mnuPrincipal.TabIndex = 0;
        //
        // mnuArchivo
        //
        this.mnuArchivo.DropDownItems.AddRange(new ToolStripItem[] {
            this.mnuCerrarSesion, this.sepArchivo, this.mnuSalir});
        this.mnuArchivo.Name = "mnuArchivo";
        this.mnuArchivo.Size = new System.Drawing.Size(60, 20);
        this.mnuArchivo.Text = "&Archivo";
        //
        // mnuCerrarSesion
        //
        this.mnuCerrarSesion.Name = "mnuCerrarSesion";
        this.mnuCerrarSesion.Size = new System.Drawing.Size(180, 22);
        this.mnuCerrarSesion.Text = "&Cerrar sesion";
        this.mnuCerrarSesion.Click += new System.EventHandler(this.mnuCerrarSesion_Click);
        //
        // sepArchivo
        //
        this.sepArchivo.Name = "sepArchivo";
        this.sepArchivo.Size = new System.Drawing.Size(177, 6);
        //
        // mnuSalir
        //
        this.mnuSalir.Name = "mnuSalir";
        this.mnuSalir.ShortcutKeys = Keys.Alt | Keys.F4;
        this.mnuSalir.Size = new System.Drawing.Size(180, 22);
        this.mnuSalir.Text = "&Salir";
        this.mnuSalir.Click += new System.EventHandler(this.mnuSalir_Click);
        //
        // mnuCatalogos
        //
        this.mnuCatalogos.DropDownItems.AddRange(new ToolStripItem[] {
            this.mnuClientes, this.mnuSalones, this.mnuRecursos});
        this.mnuCatalogos.Name = "mnuCatalogos";
        this.mnuCatalogos.Size = new System.Drawing.Size(75, 20);
        this.mnuCatalogos.Text = "C&atalogos";
        //
        // mnuClientes
        //
        this.mnuClientes.Name = "mnuClientes";
        this.mnuClientes.ShortcutKeys = Keys.Control | Keys.D1;
        this.mnuClientes.Size = new System.Drawing.Size(180, 22);
        this.mnuClientes.Text = "&Clientes";
        this.mnuClientes.Click += new System.EventHandler(this.mnuClientes_Click);
        //
        // mnuSalones
        //
        this.mnuSalones.Name = "mnuSalones";
        this.mnuSalones.ShortcutKeys = Keys.Control | Keys.D2;
        this.mnuSalones.Size = new System.Drawing.Size(180, 22);
        this.mnuSalones.Text = "&Salones";
        this.mnuSalones.Click += new System.EventHandler(this.mnuSalones_Click);
        //
        // mnuRecursos
        //
        this.mnuRecursos.Name = "mnuRecursos";
        this.mnuRecursos.ShortcutKeys = Keys.Control | Keys.D3;
        this.mnuRecursos.Size = new System.Drawing.Size(180, 22);
        this.mnuRecursos.Text = "&Recursos y servicios";
        this.mnuRecursos.Click += new System.EventHandler(this.mnuRecursos_Click);
        //
        // mnuReservas
        //
        this.mnuReservas.DropDownItems.AddRange(new ToolStripItem[] {
            this.mnuNuevaReserva, this.mnuConsultarReservas});
        this.mnuReservas.Name = "mnuReservas";
        this.mnuReservas.Size = new System.Drawing.Size(66, 20);
        this.mnuReservas.Text = "&Reservas";
        //
        // mnuNuevaReserva
        //
        this.mnuNuevaReserva.Name = "mnuNuevaReserva";
        this.mnuNuevaReserva.ShortcutKeys = Keys.Control | Keys.N;
        this.mnuNuevaReserva.Size = new System.Drawing.Size(200, 22);
        this.mnuNuevaReserva.Text = "&Nueva reserva";
        this.mnuNuevaReserva.Click += new System.EventHandler(this.mnuNuevaReserva_Click);
        //
        // mnuConsultarReservas
        //
        this.mnuConsultarReservas.Name = "mnuConsultarReservas";
        this.mnuConsultarReservas.ShortcutKeys = Keys.Control | Keys.B;
        this.mnuConsultarReservas.Size = new System.Drawing.Size(200, 22);
        this.mnuConsultarReservas.Text = "&Consultar reservas";
        this.mnuConsultarReservas.Click += new System.EventHandler(this.mnuConsultarReservas_Click);
        //
        // mnuAuditoria
        //
        this.mnuAuditoria.DropDownItems.AddRange(new ToolStripItem[] {
            this.mnuAuditoriaIntegraciones});
        this.mnuAuditoria.Name = "mnuAuditoria";
        this.mnuAuditoria.Size = new System.Drawing.Size(67, 20);
        this.mnuAuditoria.Text = "A&uditoria";
        //
        // mnuAuditoriaIntegraciones
        //
        this.mnuAuditoriaIntegraciones.Name = "mnuAuditoriaIntegraciones";
        this.mnuAuditoriaIntegraciones.ShortcutKeys = Keys.Control | Keys.U;
        this.mnuAuditoriaIntegraciones.Size = new System.Drawing.Size(220, 22);
        this.mnuAuditoriaIntegraciones.Text = "Correos y analisis de &IA";
        this.mnuAuditoriaIntegraciones.Click += new System.EventHandler(this.mnuAuditoriaIntegraciones_Click);
        //
        // mnuVentana
        //
        this.mnuVentana.DropDownItems.AddRange(new ToolStripItem[] {
            this.mnuCascada, this.mnuMosaicoHorizontal, this.mnuCerrarTodo});
        this.mnuVentana.Name = "mnuVentana";
        this.mnuVentana.Size = new System.Drawing.Size(63, 20);
        this.mnuVentana.Text = "&Ventana";
        //
        // mnuCascada
        //
        this.mnuCascada.Name = "mnuCascada";
        this.mnuCascada.Size = new System.Drawing.Size(180, 22);
        this.mnuCascada.Text = "&Cascada";
        this.mnuCascada.Click += new System.EventHandler(this.mnuCascada_Click);
        //
        // mnuMosaicoHorizontal
        //
        this.mnuMosaicoHorizontal.Name = "mnuMosaicoHorizontal";
        this.mnuMosaicoHorizontal.Size = new System.Drawing.Size(180, 22);
        this.mnuMosaicoHorizontal.Text = "&Mosaico horizontal";
        this.mnuMosaicoHorizontal.Click += new System.EventHandler(this.mnuMosaicoHorizontal_Click);
        //
        // mnuCerrarTodo
        //
        this.mnuCerrarTodo.Name = "mnuCerrarTodo";
        this.mnuCerrarTodo.Size = new System.Drawing.Size(180, 22);
        this.mnuCerrarTodo.Text = "Cerrar &todo";
        this.mnuCerrarTodo.Click += new System.EventHandler(this.mnuCerrarTodo_Click);
        //
        // mnuAyuda
        //
        this.mnuAyuda.DropDownItems.AddRange(new ToolStripItem[] { this.mnuAcercaDe });
        this.mnuAyuda.Name = "mnuAyuda";
        this.mnuAyuda.Size = new System.Drawing.Size(53, 20);
        this.mnuAyuda.Text = "A&yuda";
        //
        // mnuAcercaDe
        //
        this.mnuAcercaDe.Name = "mnuAcercaDe";
        this.mnuAcercaDe.Size = new System.Drawing.Size(180, 22);
        this.mnuAcercaDe.Text = "&Acerca de...";
        this.mnuAcercaDe.Click += new System.EventHandler(this.mnuAcercaDe_Click);
        //
        // stbEstado
        //
        this.stbEstado.Items.AddRange(new ToolStripItem[] {
            this.lblUsuarioConectado, this.lblSeparador1, this.lblEstadoConexion,
            this.lblSeparador2, this.lblEstadoIntegraciones, this.lblRelleno, this.lblFechaHora});
        this.stbEstado.Location = new System.Drawing.Point(0, 618);
        this.stbEstado.Name = "stbEstado";
        this.stbEstado.Size = new System.Drawing.Size(1024, 22);
        this.stbEstado.TabIndex = 1;
        //
        // lblUsuarioConectado
        //
        this.lblUsuarioConectado.Name = "lblUsuarioConectado";
        this.lblUsuarioConectado.Size = new System.Drawing.Size(60, 17);
        this.lblUsuarioConectado.Text = "Usuario";
        //
        // lblSeparador1
        //
        this.lblSeparador1.Name = "lblSeparador1";
        this.lblSeparador1.Size = new System.Drawing.Size(10, 17);
        this.lblSeparador1.Text = "|";
        //
        // lblEstadoConexion
        //
        this.lblEstadoConexion.Name = "lblEstadoConexion";
        this.lblEstadoConexion.Size = new System.Drawing.Size(70, 17);
        this.lblEstadoConexion.Text = "Conexion";
        //
        // lblSeparador2
        //
        this.lblSeparador2.Name = "lblSeparador2";
        this.lblSeparador2.Size = new System.Drawing.Size(10, 17);
        this.lblSeparador2.Text = "|";
        //
        // lblEstadoIntegraciones
        //
        this.lblEstadoIntegraciones.Name = "lblEstadoIntegraciones";
        this.lblEstadoIntegraciones.Size = new System.Drawing.Size(120, 17);
        this.lblEstadoIntegraciones.Text = "Integraciones";
        //
        // lblRelleno
        //
        this.lblRelleno.Name = "lblRelleno";
        this.lblRelleno.Size = new System.Drawing.Size(700, 17);
        this.lblRelleno.Spring = true;
        //
        // lblFechaHora
        //
        this.lblFechaHora.Name = "lblFechaHora";
        this.lblFechaHora.Size = new System.Drawing.Size(120, 17);
        //
        // tmrEstado
        //
        this.tmrEstado.Interval = 30000;
        this.tmrEstado.Tick += new System.EventHandler(this.tmrEstado_Tick);
        //
        // FrmPrincipal
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1024, 640);
        this.Controls.Add(this.stbEstado);
        this.Controls.Add(this.mnuPrincipal);
        this.IsMdiContainer = true;
        this.MainMenuStrip = this.mnuPrincipal;
        this.Name = "FrmPrincipal";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "SmartEvent AI";
        this.WindowState = FormWindowState.Maximized;
        this.Load += new System.EventHandler(this.FrmPrincipal_Load);
        this.FormClosing += new FormClosingEventHandler(this.FrmPrincipal_FormClosing);
        this.mnuPrincipal.ResumeLayout(false);
        this.mnuPrincipal.PerformLayout();
        this.stbEstado.ResumeLayout(false);
        this.stbEstado.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
