namespace SmartEvent.UI.Formularios;

partial class FrmLogin
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlEncabezado;
    private Label lblTitulo;
    private Label lblSubtitulo;
    private GroupBox grpCredenciales;
    private Label lblUsuario;
    private TextBox txtUsuario;
    private Label lblContrasena;
    private TextBox txtContrasena;
    private CheckBox chkVerContrasena;
    private Label lblMensaje;
    private ProgressBar prgActividad;
    private Button btnIngresar;
    private Button btnSalir;
    private Label lblConexion;
    private System.Windows.Forms.Timer tmrBloqueo;

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
        this.pnlEncabezado = new Panel();
        this.lblTitulo = new Label();
        this.lblSubtitulo = new Label();
        this.grpCredenciales = new GroupBox();
        this.lblUsuario = new Label();
        this.txtUsuario = new TextBox();
        this.lblContrasena = new Label();
        this.txtContrasena = new TextBox();
        this.chkVerContrasena = new CheckBox();
        this.lblMensaje = new Label();
        this.prgActividad = new ProgressBar();
        this.btnIngresar = new Button();
        this.btnSalir = new Button();
        this.lblConexion = new Label();
        this.tmrBloqueo = new System.Windows.Forms.Timer(this.components);
        this.pnlEncabezado.SuspendLayout();
        this.grpCredenciales.SuspendLayout();
        this.SuspendLayout();
        //
        // pnlEncabezado
        //
        this.pnlEncabezado.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
        this.pnlEncabezado.Controls.Add(this.lblSubtitulo);
        this.pnlEncabezado.Controls.Add(this.lblTitulo);
        this.pnlEncabezado.Dock = DockStyle.Top;
        this.pnlEncabezado.Location = new System.Drawing.Point(0, 0);
        this.pnlEncabezado.Name = "pnlEncabezado";
        this.pnlEncabezado.Size = new System.Drawing.Size(424, 68);
        this.pnlEncabezado.TabIndex = 0;
        //
        // lblTitulo
        //
        this.lblTitulo.AutoSize = true;
        this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
        this.lblTitulo.ForeColor = System.Drawing.Color.White;
        this.lblTitulo.Location = new System.Drawing.Point(18, 12);
        this.lblTitulo.Name = "lblTitulo";
        this.lblTitulo.Size = new System.Drawing.Size(151, 30);
        this.lblTitulo.TabIndex = 0;
        this.lblTitulo.Text = "SmartEvent AI";
        //
        // lblSubtitulo
        //
        this.lblSubtitulo.AutoSize = true;
        this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(200, 211, 232);
        this.lblSubtitulo.Location = new System.Drawing.Point(21, 44);
        this.lblSubtitulo.Name = "lblSubtitulo";
        this.lblSubtitulo.Size = new System.Drawing.Size(268, 15);
        this.lblSubtitulo.TabIndex = 1;
        this.lblSubtitulo.Text = "Reservas de salones, recursos y comunicacion";
        //
        // grpCredenciales
        //
        this.grpCredenciales.Controls.Add(this.chkVerContrasena);
        this.grpCredenciales.Controls.Add(this.txtContrasena);
        this.grpCredenciales.Controls.Add(this.lblContrasena);
        this.grpCredenciales.Controls.Add(this.txtUsuario);
        this.grpCredenciales.Controls.Add(this.lblUsuario);
        this.grpCredenciales.Location = new System.Drawing.Point(18, 82);
        this.grpCredenciales.Name = "grpCredenciales";
        this.grpCredenciales.Size = new System.Drawing.Size(388, 128);
        this.grpCredenciales.TabIndex = 1;
        this.grpCredenciales.TabStop = false;
        this.grpCredenciales.Text = "Inicio de sesion";
        //
        // lblUsuario
        //
        this.lblUsuario.AutoSize = true;
        this.lblUsuario.Location = new System.Drawing.Point(16, 30);
        this.lblUsuario.Name = "lblUsuario";
        this.lblUsuario.Size = new System.Drawing.Size(49, 15);
        this.lblUsuario.TabIndex = 0;
        this.lblUsuario.Text = "&Usuario";
        //
        // txtUsuario
        //
        this.txtUsuario.Location = new System.Drawing.Point(120, 27);
        this.txtUsuario.MaxLength = 50;
        this.txtUsuario.Name = "txtUsuario";
        this.txtUsuario.Size = new System.Drawing.Size(248, 23);
        this.txtUsuario.TabIndex = 1;
        //
        // lblContrasena
        //
        this.lblContrasena.AutoSize = true;
        this.lblContrasena.Location = new System.Drawing.Point(16, 62);
        this.lblContrasena.Name = "lblContrasena";
        this.lblContrasena.Size = new System.Drawing.Size(68, 15);
        this.lblContrasena.TabIndex = 2;
        this.lblContrasena.Text = "&Contrasena";
        //
        // txtContrasena
        //
        this.txtContrasena.Location = new System.Drawing.Point(120, 59);
        this.txtContrasena.MaxLength = 128;
        this.txtContrasena.Name = "txtContrasena";
        this.txtContrasena.Size = new System.Drawing.Size(248, 23);
        this.txtContrasena.TabIndex = 3;
        this.txtContrasena.UseSystemPasswordChar = true;
        //
        // chkVerContrasena
        //
        this.chkVerContrasena.AutoSize = true;
        this.chkVerContrasena.Location = new System.Drawing.Point(120, 92);
        this.chkVerContrasena.Name = "chkVerContrasena";
        this.chkVerContrasena.Size = new System.Drawing.Size(120, 19);
        this.chkVerContrasena.TabIndex = 4;
        this.chkVerContrasena.Text = "&Mostrar contrasena";
        this.chkVerContrasena.UseVisualStyleBackColor = true;
        this.chkVerContrasena.CheckedChanged += new System.EventHandler(this.chkVerContrasena_CheckedChanged);
        //
        // lblMensaje
        //
        this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(179, 38, 30);
        this.lblMensaje.Location = new System.Drawing.Point(18, 216);
        this.lblMensaje.Name = "lblMensaje";
        this.lblMensaje.Size = new System.Drawing.Size(388, 36);
        this.lblMensaje.TabIndex = 2;
        //
        // prgActividad
        //
        this.prgActividad.Location = new System.Drawing.Point(18, 252);
        this.prgActividad.MarqueeAnimationSpeed = 30;
        this.prgActividad.Name = "prgActividad";
        this.prgActividad.Size = new System.Drawing.Size(388, 6);
        this.prgActividad.Style = ProgressBarStyle.Marquee;
        this.prgActividad.TabIndex = 3;
        this.prgActividad.Visible = false;
        //
        // btnIngresar
        //
        this.btnIngresar.Location = new System.Drawing.Point(230, 264);
        this.btnIngresar.Name = "btnIngresar";
        this.btnIngresar.Size = new System.Drawing.Size(88, 30);
        this.btnIngresar.TabIndex = 4;
        this.btnIngresar.Text = "&Ingresar";
        this.btnIngresar.UseVisualStyleBackColor = true;
        this.btnIngresar.Click += new System.EventHandler(this.btnIngresar_Click);
        //
        // btnSalir
        //
        this.btnSalir.DialogResult = DialogResult.Cancel;
        this.btnSalir.Location = new System.Drawing.Point(324, 264);
        this.btnSalir.Name = "btnSalir";
        this.btnSalir.Size = new System.Drawing.Size(82, 30);
        this.btnSalir.TabIndex = 5;
        this.btnSalir.Text = "&Salir";
        this.btnSalir.UseVisualStyleBackColor = true;
        this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
        //
        // lblConexion
        //
        this.lblConexion.AutoEllipsis = true;
        this.lblConexion.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblConexion.Location = new System.Drawing.Point(18, 306);
        this.lblConexion.Name = "lblConexion";
        this.lblConexion.Size = new System.Drawing.Size(388, 20);
        this.lblConexion.TabIndex = 6;
        this.lblConexion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // tmrBloqueo
        //
        this.tmrBloqueo.Interval = 1000;
        this.tmrBloqueo.Tick += new System.EventHandler(this.tmrBloqueo_Tick);
        //
        // FrmLogin
        //
        this.AcceptButton = this.btnIngresar;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.CancelButton = this.btnSalir;
        this.ClientSize = new System.Drawing.Size(424, 336);
        this.Controls.Add(this.lblConexion);
        this.Controls.Add(this.btnSalir);
        this.Controls.Add(this.btnIngresar);
        this.Controls.Add(this.prgActividad);
        this.Controls.Add(this.lblMensaje);
        this.Controls.Add(this.grpCredenciales);
        this.Controls.Add(this.pnlEncabezado);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FrmLogin";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "SmartEvent AI - Iniciar sesion";
        this.Load += new System.EventHandler(this.FrmLogin_Load);
        this.FormClosed += new FormClosedEventHandler(this.FrmLogin_FormClosed);
        this.pnlEncabezado.ResumeLayout(false);
        this.pnlEncabezado.PerformLayout();
        this.grpCredenciales.ResumeLayout(false);
        this.grpCredenciales.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
