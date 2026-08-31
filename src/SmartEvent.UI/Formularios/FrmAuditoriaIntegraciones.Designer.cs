namespace SmartEvent.UI.Formularios;

partial class FrmAuditoriaIntegraciones
{
    private System.ComponentModel.IContainer components = null;

    private TabControl tabAuditoria;

    // ---------------------------------------------------------------------------- correos
    private TabPage tabCorreos;
    private Panel pnlFiltroCorreos;
    private Label lblCodigoCorreo;
    private TextBox txtCodigoCorreo;
    private Label lblEstadoCorreo;
    private ComboBox cboEstadoCorreo;
    private CheckBox chkFechasCorreo;
    private DateTimePicker dtpDesdeCorreo;
    private Label lblHastaCorreo;
    private DateTimePicker dtpHastaCorreo;
    private Button btnBuscarCorreos;
    private DataGridView dgvCorreos;
    private GroupBox grpDetalleCorreo;
    private TextBox txtErrorCorreo;
    private Button btnReenviar;

    // --------------------------------------------------------------------------- analisis
    private TabPage tabAnalisis;
    private Panel pnlFiltroAnalisis;
    private Label lblCodigoAnalisis;
    private TextBox txtCodigoAnalisis;
    private Label lblResultadoAnalisis;
    private ComboBox cboResultadoAnalisis;
    private Label lblNivelRiesgo;
    private ComboBox cboNivelRiesgo;
    private Button btnBuscarAnalisis;
    private DataGridView dgvAnalisis;
    private GroupBox grpDetalleAnalisis;
    private TextBox txtDetalleAnalisis;

    private StatusStrip stbAuditoria;
    private ToolStripStatusLabel lblMensajeEstado;
    private ToolStripStatusLabel lblArchivoRegistro;

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
        this.tabAuditoria = new TabControl();
        this.tabCorreos = new TabPage();
        this.dgvCorreos = new DataGridView();
        this.grpDetalleCorreo = new GroupBox();
        this.btnReenviar = new Button();
        this.txtErrorCorreo = new TextBox();
        this.pnlFiltroCorreos = new Panel();
        this.btnBuscarCorreos = new Button();
        this.dtpHastaCorreo = new DateTimePicker();
        this.lblHastaCorreo = new Label();
        this.dtpDesdeCorreo = new DateTimePicker();
        this.chkFechasCorreo = new CheckBox();
        this.cboEstadoCorreo = new ComboBox();
        this.lblEstadoCorreo = new Label();
        this.txtCodigoCorreo = new TextBox();
        this.lblCodigoCorreo = new Label();
        this.tabAnalisis = new TabPage();
        this.dgvAnalisis = new DataGridView();
        this.grpDetalleAnalisis = new GroupBox();
        this.txtDetalleAnalisis = new TextBox();
        this.pnlFiltroAnalisis = new Panel();
        this.btnBuscarAnalisis = new Button();
        this.cboNivelRiesgo = new ComboBox();
        this.lblNivelRiesgo = new Label();
        this.cboResultadoAnalisis = new ComboBox();
        this.lblResultadoAnalisis = new Label();
        this.txtCodigoAnalisis = new TextBox();
        this.lblCodigoAnalisis = new Label();
        this.stbAuditoria = new StatusStrip();
        this.lblMensajeEstado = new ToolStripStatusLabel();
        this.lblArchivoRegistro = new ToolStripStatusLabel();
        this.tabAuditoria.SuspendLayout();
        this.tabCorreos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvCorreos)).BeginInit();
        this.grpDetalleCorreo.SuspendLayout();
        this.pnlFiltroCorreos.SuspendLayout();
        this.tabAnalisis.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvAnalisis)).BeginInit();
        this.grpDetalleAnalisis.SuspendLayout();
        this.pnlFiltroAnalisis.SuspendLayout();
        this.stbAuditoria.SuspendLayout();
        this.SuspendLayout();
        //
        // tabAuditoria
        //
        this.tabAuditoria.Controls.Add(this.tabCorreos);
        this.tabAuditoria.Controls.Add(this.tabAnalisis);
        this.tabAuditoria.Dock = DockStyle.Fill;
        this.tabAuditoria.Location = new System.Drawing.Point(0, 0);
        this.tabAuditoria.Name = "tabAuditoria";
        this.tabAuditoria.SelectedIndex = 0;
        this.tabAuditoria.Size = new System.Drawing.Size(940, 538);
        this.tabAuditoria.TabIndex = 0;
        //
        // ====================================================================== CORREOS
        //
        this.tabCorreos.Controls.Add(this.dgvCorreos);
        this.tabCorreos.Controls.Add(this.grpDetalleCorreo);
        this.tabCorreos.Controls.Add(this.pnlFiltroCorreos);
        this.tabCorreos.Location = new System.Drawing.Point(4, 24);
        this.tabCorreos.Name = "tabCorreos";
        this.tabCorreos.Padding = new Padding(3);
        this.tabCorreos.Size = new System.Drawing.Size(932, 510);
        this.tabCorreos.TabIndex = 0;
        this.tabCorreos.Text = "Correos enviados";
        this.tabCorreos.UseVisualStyleBackColor = true;
        //
        this.pnlFiltroCorreos.Controls.Add(this.btnBuscarCorreos);
        this.pnlFiltroCorreos.Controls.Add(this.dtpHastaCorreo);
        this.pnlFiltroCorreos.Controls.Add(this.lblHastaCorreo);
        this.pnlFiltroCorreos.Controls.Add(this.dtpDesdeCorreo);
        this.pnlFiltroCorreos.Controls.Add(this.chkFechasCorreo);
        this.pnlFiltroCorreos.Controls.Add(this.cboEstadoCorreo);
        this.pnlFiltroCorreos.Controls.Add(this.lblEstadoCorreo);
        this.pnlFiltroCorreos.Controls.Add(this.txtCodigoCorreo);
        this.pnlFiltroCorreos.Controls.Add(this.lblCodigoCorreo);
        this.pnlFiltroCorreos.Dock = DockStyle.Top;
        this.pnlFiltroCorreos.Location = new System.Drawing.Point(3, 3);
        this.pnlFiltroCorreos.Name = "pnlFiltroCorreos";
        this.pnlFiltroCorreos.Size = new System.Drawing.Size(926, 44);
        this.pnlFiltroCorreos.TabIndex = 0;
        //
        this.lblCodigoCorreo.AutoSize = true;
        this.lblCodigoCorreo.Location = new System.Drawing.Point(6, 14);
        this.lblCodigoCorreo.Name = "lblCodigoCorreo";
        this.lblCodigoCorreo.Size = new System.Drawing.Size(46, 15);
        this.lblCodigoCorreo.TabIndex = 0;
        this.lblCodigoCorreo.Text = "Codigo";
        //
        this.txtCodigoCorreo.Location = new System.Drawing.Point(58, 11);
        this.txtCodigoCorreo.MaxLength = 20;
        this.txtCodigoCorreo.Name = "txtCodigoCorreo";
        this.txtCodigoCorreo.PlaceholderText = "RSV-...";
        this.txtCodigoCorreo.Size = new System.Drawing.Size(140, 23);
        this.txtCodigoCorreo.TabIndex = 1;
        this.txtCodigoCorreo.KeyDown += new KeyEventHandler(this.FiltroCorreo_KeyDown);
        //
        this.lblEstadoCorreo.AutoSize = true;
        this.lblEstadoCorreo.Location = new System.Drawing.Point(212, 14);
        this.lblEstadoCorreo.Name = "lblEstadoCorreo";
        this.lblEstadoCorreo.Size = new System.Drawing.Size(42, 15);
        this.lblEstadoCorreo.TabIndex = 2;
        this.lblEstadoCorreo.Text = "Estado";
        //
        this.cboEstadoCorreo.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboEstadoCorreo.Location = new System.Drawing.Point(260, 11);
        this.cboEstadoCorreo.Name = "cboEstadoCorreo";
        this.cboEstadoCorreo.Size = new System.Drawing.Size(130, 23);
        this.cboEstadoCorreo.TabIndex = 3;
        //
        this.chkFechasCorreo.AutoSize = true;
        this.chkFechasCorreo.Location = new System.Drawing.Point(404, 13);
        this.chkFechasCorreo.Name = "chkFechasCorreo";
        this.chkFechasCorreo.Size = new System.Drawing.Size(63, 19);
        this.chkFechasCorreo.TabIndex = 4;
        this.chkFechasCorreo.Text = "Fechas";
        this.chkFechasCorreo.UseVisualStyleBackColor = true;
        this.chkFechasCorreo.CheckedChanged += new System.EventHandler(this.chkFechasCorreo_CheckedChanged);
        //
        this.dtpDesdeCorreo.Enabled = false;
        this.dtpDesdeCorreo.Format = DateTimePickerFormat.Short;
        this.dtpDesdeCorreo.Location = new System.Drawing.Point(473, 11);
        this.dtpDesdeCorreo.Name = "dtpDesdeCorreo";
        this.dtpDesdeCorreo.Size = new System.Drawing.Size(115, 23);
        this.dtpDesdeCorreo.TabIndex = 5;
        //
        this.lblHastaCorreo.AutoSize = true;
        this.lblHastaCorreo.Location = new System.Drawing.Point(594, 14);
        this.lblHastaCorreo.Name = "lblHastaCorreo";
        this.lblHastaCorreo.Size = new System.Drawing.Size(35, 15);
        this.lblHastaCorreo.TabIndex = 6;
        this.lblHastaCorreo.Text = "hasta";
        //
        this.dtpHastaCorreo.Enabled = false;
        this.dtpHastaCorreo.Format = DateTimePickerFormat.Short;
        this.dtpHastaCorreo.Location = new System.Drawing.Point(635, 11);
        this.dtpHastaCorreo.Name = "dtpHastaCorreo";
        this.dtpHastaCorreo.Size = new System.Drawing.Size(115, 23);
        this.dtpHastaCorreo.TabIndex = 7;
        //
        this.btnBuscarCorreos.Location = new System.Drawing.Point(800, 10);
        this.btnBuscarCorreos.Name = "btnBuscarCorreos";
        this.btnBuscarCorreos.Size = new System.Drawing.Size(110, 26);
        this.btnBuscarCorreos.TabIndex = 8;
        this.btnBuscarCorreos.Text = "Buscar";
        this.btnBuscarCorreos.UseVisualStyleBackColor = true;
        this.btnBuscarCorreos.Click += new System.EventHandler(this.btnBuscarCorreos_Click);
        //
        this.dgvCorreos.AllowUserToAddRows = false;
        this.dgvCorreos.AllowUserToDeleteRows = false;
        this.dgvCorreos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvCorreos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvCorreos.Dock = DockStyle.Fill;
        this.dgvCorreos.Location = new System.Drawing.Point(3, 47);
        this.dgvCorreos.MultiSelect = false;
        this.dgvCorreos.Name = "dgvCorreos";
        this.dgvCorreos.ReadOnly = true;
        this.dgvCorreos.RowHeadersVisible = false;
        this.dgvCorreos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvCorreos.Size = new System.Drawing.Size(926, 320);
        this.dgvCorreos.TabIndex = 1;
        this.dgvCorreos.SelectionChanged += new System.EventHandler(this.dgvCorreos_SelectionChanged);
        this.dgvCorreos.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvCorreos_CellFormatting);
        //
        this.grpDetalleCorreo.Controls.Add(this.btnReenviar);
        this.grpDetalleCorreo.Controls.Add(this.txtErrorCorreo);
        this.grpDetalleCorreo.Dock = DockStyle.Bottom;
        this.grpDetalleCorreo.Location = new System.Drawing.Point(3, 367);
        this.grpDetalleCorreo.Name = "grpDetalleCorreo";
        this.grpDetalleCorreo.Size = new System.Drawing.Size(926, 140);
        this.grpDetalleCorreo.TabIndex = 2;
        this.grpDetalleCorreo.TabStop = false;
        this.grpDetalleCorreo.Text = "Detalle tecnico del intento";
        //
        this.txtErrorCorreo.BackColor = System.Drawing.SystemColors.Window;
        this.txtErrorCorreo.Location = new System.Drawing.Point(12, 24);
        this.txtErrorCorreo.Multiline = true;
        this.txtErrorCorreo.Name = "txtErrorCorreo";
        this.txtErrorCorreo.ReadOnly = true;
        this.txtErrorCorreo.ScrollBars = ScrollBars.Vertical;
        this.txtErrorCorreo.Size = new System.Drawing.Size(730, 104);
        this.txtErrorCorreo.TabIndex = 0;
        //
        this.btnReenviar.Enabled = false;
        this.btnReenviar.Location = new System.Drawing.Point(756, 24);
        this.btnReenviar.Name = "btnReenviar";
        this.btnReenviar.Size = new System.Drawing.Size(154, 36);
        this.btnReenviar.TabIndex = 1;
        this.btnReenviar.Text = "&Reenviar notificacion";
        this.btnReenviar.UseVisualStyleBackColor = true;
        this.btnReenviar.Click += new System.EventHandler(this.btnReenviar_Click);
        //
        // ===================================================================== ANALISIS
        //
        this.tabAnalisis.Controls.Add(this.dgvAnalisis);
        this.tabAnalisis.Controls.Add(this.grpDetalleAnalisis);
        this.tabAnalisis.Controls.Add(this.pnlFiltroAnalisis);
        this.tabAnalisis.Location = new System.Drawing.Point(4, 24);
        this.tabAnalisis.Name = "tabAnalisis";
        this.tabAnalisis.Padding = new Padding(3);
        this.tabAnalisis.Size = new System.Drawing.Size(932, 510);
        this.tabAnalisis.TabIndex = 1;
        this.tabAnalisis.Text = "Analisis de IA";
        this.tabAnalisis.UseVisualStyleBackColor = true;
        //
        this.pnlFiltroAnalisis.Controls.Add(this.btnBuscarAnalisis);
        this.pnlFiltroAnalisis.Controls.Add(this.cboNivelRiesgo);
        this.pnlFiltroAnalisis.Controls.Add(this.lblNivelRiesgo);
        this.pnlFiltroAnalisis.Controls.Add(this.cboResultadoAnalisis);
        this.pnlFiltroAnalisis.Controls.Add(this.lblResultadoAnalisis);
        this.pnlFiltroAnalisis.Controls.Add(this.txtCodigoAnalisis);
        this.pnlFiltroAnalisis.Controls.Add(this.lblCodigoAnalisis);
        this.pnlFiltroAnalisis.Dock = DockStyle.Top;
        this.pnlFiltroAnalisis.Location = new System.Drawing.Point(3, 3);
        this.pnlFiltroAnalisis.Name = "pnlFiltroAnalisis";
        this.pnlFiltroAnalisis.Size = new System.Drawing.Size(926, 44);
        this.pnlFiltroAnalisis.TabIndex = 0;
        //
        this.lblCodigoAnalisis.AutoSize = true;
        this.lblCodigoAnalisis.Location = new System.Drawing.Point(6, 14);
        this.lblCodigoAnalisis.Name = "lblCodigoAnalisis";
        this.lblCodigoAnalisis.Size = new System.Drawing.Size(46, 15);
        this.lblCodigoAnalisis.TabIndex = 0;
        this.lblCodigoAnalisis.Text = "Codigo";
        //
        this.txtCodigoAnalisis.Location = new System.Drawing.Point(58, 11);
        this.txtCodigoAnalisis.MaxLength = 20;
        this.txtCodigoAnalisis.Name = "txtCodigoAnalisis";
        this.txtCodigoAnalisis.PlaceholderText = "RSV-...";
        this.txtCodigoAnalisis.Size = new System.Drawing.Size(140, 23);
        this.txtCodigoAnalisis.TabIndex = 1;
        this.txtCodigoAnalisis.KeyDown += new KeyEventHandler(this.FiltroAnalisis_KeyDown);
        //
        this.lblResultadoAnalisis.AutoSize = true;
        this.lblResultadoAnalisis.Location = new System.Drawing.Point(212, 14);
        this.lblResultadoAnalisis.Name = "lblResultadoAnalisis";
        this.lblResultadoAnalisis.Size = new System.Drawing.Size(60, 15);
        this.lblResultadoAnalisis.TabIndex = 2;
        this.lblResultadoAnalisis.Text = "Resultado";
        //
        this.cboResultadoAnalisis.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboResultadoAnalisis.Location = new System.Drawing.Point(278, 11);
        this.cboResultadoAnalisis.Name = "cboResultadoAnalisis";
        this.cboResultadoAnalisis.Size = new System.Drawing.Size(130, 23);
        this.cboResultadoAnalisis.TabIndex = 3;
        //
        this.lblNivelRiesgo.AutoSize = true;
        this.lblNivelRiesgo.Location = new System.Drawing.Point(424, 14);
        this.lblNivelRiesgo.Name = "lblNivelRiesgo";
        this.lblNivelRiesgo.Size = new System.Drawing.Size(83, 15);
        this.lblNivelRiesgo.TabIndex = 4;
        this.lblNivelRiesgo.Text = "Nivel de riesgo";
        //
        this.cboNivelRiesgo.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboNivelRiesgo.Location = new System.Drawing.Point(513, 11);
        this.cboNivelRiesgo.Name = "cboNivelRiesgo";
        this.cboNivelRiesgo.Size = new System.Drawing.Size(130, 23);
        this.cboNivelRiesgo.TabIndex = 5;
        //
        this.btnBuscarAnalisis.Location = new System.Drawing.Point(800, 10);
        this.btnBuscarAnalisis.Name = "btnBuscarAnalisis";
        this.btnBuscarAnalisis.Size = new System.Drawing.Size(110, 26);
        this.btnBuscarAnalisis.TabIndex = 6;
        this.btnBuscarAnalisis.Text = "Buscar";
        this.btnBuscarAnalisis.UseVisualStyleBackColor = true;
        this.btnBuscarAnalisis.Click += new System.EventHandler(this.btnBuscarAnalisis_Click);
        //
        this.dgvAnalisis.AllowUserToAddRows = false;
        this.dgvAnalisis.AllowUserToDeleteRows = false;
        this.dgvAnalisis.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvAnalisis.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvAnalisis.Dock = DockStyle.Fill;
        this.dgvAnalisis.Location = new System.Drawing.Point(3, 47);
        this.dgvAnalisis.MultiSelect = false;
        this.dgvAnalisis.Name = "dgvAnalisis";
        this.dgvAnalisis.ReadOnly = true;
        this.dgvAnalisis.RowHeadersVisible = false;
        this.dgvAnalisis.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvAnalisis.Size = new System.Drawing.Size(926, 280);
        this.dgvAnalisis.TabIndex = 1;
        this.dgvAnalisis.SelectionChanged += new System.EventHandler(this.dgvAnalisis_SelectionChanged);
        this.dgvAnalisis.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvAnalisis_CellFormatting);
        //
        this.grpDetalleAnalisis.Controls.Add(this.txtDetalleAnalisis);
        this.grpDetalleAnalisis.Dock = DockStyle.Bottom;
        this.grpDetalleAnalisis.Location = new System.Drawing.Point(3, 327);
        this.grpDetalleAnalisis.Name = "grpDetalleAnalisis";
        this.grpDetalleAnalisis.Padding = new Padding(6);
        this.grpDetalleAnalisis.Size = new System.Drawing.Size(926, 180);
        this.grpDetalleAnalisis.TabIndex = 2;
        this.grpDetalleAnalisis.TabStop = false;
        this.grpDetalleAnalisis.Text = "Respuesta estructurada / detalle del error";
        //
        this.txtDetalleAnalisis.BackColor = System.Drawing.SystemColors.Window;
        this.txtDetalleAnalisis.Dock = DockStyle.Fill;
        this.txtDetalleAnalisis.Font = new System.Drawing.Font("Consolas", 9F);
        this.txtDetalleAnalisis.Location = new System.Drawing.Point(6, 22);
        this.txtDetalleAnalisis.Multiline = true;
        this.txtDetalleAnalisis.Name = "txtDetalleAnalisis";
        this.txtDetalleAnalisis.ReadOnly = true;
        this.txtDetalleAnalisis.ScrollBars = ScrollBars.Both;
        this.txtDetalleAnalisis.Size = new System.Drawing.Size(914, 152);
        this.txtDetalleAnalisis.TabIndex = 0;
        this.txtDetalleAnalisis.WordWrap = false;
        //
        // stbAuditoria
        //
        this.stbAuditoria.Items.AddRange(new ToolStripItem[] { this.lblMensajeEstado, this.lblArchivoRegistro });
        this.stbAuditoria.Location = new System.Drawing.Point(0, 538);
        this.stbAuditoria.Name = "stbAuditoria";
        this.stbAuditoria.Size = new System.Drawing.Size(940, 22);
        this.stbAuditoria.TabIndex = 1;
        //
        this.lblMensajeEstado.Name = "lblMensajeEstado";
        this.lblMensajeEstado.Size = new System.Drawing.Size(500, 17);
        this.lblMensajeEstado.Spring = true;
        this.lblMensajeEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        this.lblArchivoRegistro.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblArchivoRegistro.Name = "lblArchivoRegistro";
        this.lblArchivoRegistro.Size = new System.Drawing.Size(400, 17);
        //
        // FrmAuditoriaIntegraciones
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(940, 560);
        this.Controls.Add(this.tabAuditoria);
        this.Controls.Add(this.stbAuditoria);
        this.MinimumSize = new System.Drawing.Size(900, 540);
        this.Name = "FrmAuditoriaIntegraciones";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Auditoria de integraciones";
        this.Load += new System.EventHandler(this.FrmAuditoriaIntegraciones_Load);
        this.FormClosed += new FormClosedEventHandler(this.FrmAuditoriaIntegraciones_FormClosed);
        this.tabAuditoria.ResumeLayout(false);
        this.tabCorreos.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvCorreos)).EndInit();
        this.grpDetalleCorreo.ResumeLayout(false);
        this.grpDetalleCorreo.PerformLayout();
        this.pnlFiltroCorreos.ResumeLayout(false);
        this.pnlFiltroCorreos.PerformLayout();
        this.tabAnalisis.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvAnalisis)).EndInit();
        this.grpDetalleAnalisis.ResumeLayout(false);
        this.grpDetalleAnalisis.PerformLayout();
        this.pnlFiltroAnalisis.ResumeLayout(false);
        this.pnlFiltroAnalisis.PerformLayout();
        this.stbAuditoria.ResumeLayout(false);
        this.stbAuditoria.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
