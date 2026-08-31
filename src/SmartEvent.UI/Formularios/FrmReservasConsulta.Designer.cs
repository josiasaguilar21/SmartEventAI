namespace SmartEvent.UI.Formularios;

partial class FrmReservasConsulta
{
    private System.ComponentModel.IContainer components = null;

    private GroupBox grpFiltros;
    private Label lblCodigo;
    private TextBox txtCodigo;
    private Label lblCliente;
    private TextBox txtCliente;
    private CheckBox chkFechas;
    private DateTimePicker dtpDesde;
    private Label lblHasta;
    private DateTimePicker dtpHasta;
    private Label lblSalon;
    private ComboBox cboSalon;
    private Label lblEstado;
    private ComboBox cboEstado;
    private Button btnBuscar;
    private Button btnLimpiar;
    private Button btnCancelarBusqueda;

    private DataGridView dgvReservas;

    private Panel pnlPie;
    private Label lblResultados;
    private Button btnAnterior;
    private Label lblPagina;
    private Button btnSiguiente;
    private Label lblTamano;
    private ComboBox cboTamanoPagina;
    private Button btnNueva;
    private Button btnAbrir;

    private StatusStrip stbConsulta;
    private ToolStripStatusLabel lblMensajeEstado;
    private ToolStripProgressBar prgBuscando;

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
        this.grpFiltros = new GroupBox();
        this.btnCancelarBusqueda = new Button();
        this.btnLimpiar = new Button();
        this.btnBuscar = new Button();
        this.cboEstado = new ComboBox();
        this.lblEstado = new Label();
        this.cboSalon = new ComboBox();
        this.lblSalon = new Label();
        this.dtpHasta = new DateTimePicker();
        this.lblHasta = new Label();
        this.dtpDesde = new DateTimePicker();
        this.chkFechas = new CheckBox();
        this.txtCliente = new TextBox();
        this.lblCliente = new Label();
        this.txtCodigo = new TextBox();
        this.lblCodigo = new Label();
        this.dgvReservas = new DataGridView();
        this.pnlPie = new Panel();
        this.btnAbrir = new Button();
        this.btnNueva = new Button();
        this.cboTamanoPagina = new ComboBox();
        this.lblTamano = new Label();
        this.btnSiguiente = new Button();
        this.lblPagina = new Label();
        this.btnAnterior = new Button();
        this.lblResultados = new Label();
        this.stbConsulta = new StatusStrip();
        this.lblMensajeEstado = new ToolStripStatusLabel();
        this.prgBuscando = new ToolStripProgressBar();
        this.grpFiltros.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvReservas)).BeginInit();
        this.pnlPie.SuspendLayout();
        this.stbConsulta.SuspendLayout();
        this.SuspendLayout();
        //
        // grpFiltros
        //
        this.grpFiltros.Controls.Add(this.btnCancelarBusqueda);
        this.grpFiltros.Controls.Add(this.btnLimpiar);
        this.grpFiltros.Controls.Add(this.btnBuscar);
        this.grpFiltros.Controls.Add(this.cboEstado);
        this.grpFiltros.Controls.Add(this.lblEstado);
        this.grpFiltros.Controls.Add(this.cboSalon);
        this.grpFiltros.Controls.Add(this.lblSalon);
        this.grpFiltros.Controls.Add(this.dtpHasta);
        this.grpFiltros.Controls.Add(this.lblHasta);
        this.grpFiltros.Controls.Add(this.dtpDesde);
        this.grpFiltros.Controls.Add(this.chkFechas);
        this.grpFiltros.Controls.Add(this.txtCliente);
        this.grpFiltros.Controls.Add(this.lblCliente);
        this.grpFiltros.Controls.Add(this.txtCodigo);
        this.grpFiltros.Controls.Add(this.lblCodigo);
        this.grpFiltros.Dock = DockStyle.Top;
        this.grpFiltros.Location = new System.Drawing.Point(0, 0);
        this.grpFiltros.Name = "grpFiltros";
        this.grpFiltros.Size = new System.Drawing.Size(960, 106);
        this.grpFiltros.TabIndex = 0;
        this.grpFiltros.TabStop = false;
        this.grpFiltros.Text = "Filtros de busqueda";
        //
        this.lblCodigo.AutoSize = true;
        this.lblCodigo.Location = new System.Drawing.Point(16, 30);
        this.lblCodigo.Name = "lblCodigo";
        this.lblCodigo.Size = new System.Drawing.Size(46, 15);
        this.lblCodigo.TabIndex = 0;
        this.lblCodigo.Text = "&Codigo";
        //
        this.txtCodigo.Location = new System.Drawing.Point(70, 27);
        this.txtCodigo.MaxLength = 20;
        this.txtCodigo.Name = "txtCodigo";
        this.txtCodigo.PlaceholderText = "RSV-...";
        this.txtCodigo.Size = new System.Drawing.Size(140, 23);
        this.txtCodigo.TabIndex = 1;
        this.txtCodigo.KeyDown += new KeyEventHandler(this.Filtro_KeyDown);
        //
        this.lblCliente.AutoSize = true;
        this.lblCliente.Location = new System.Drawing.Point(226, 30);
        this.lblCliente.Name = "lblCliente";
        this.lblCliente.Size = new System.Drawing.Size(45, 15);
        this.lblCliente.TabIndex = 2;
        this.lblCliente.Text = "C&liente";
        //
        this.txtCliente.Location = new System.Drawing.Point(278, 27);
        this.txtCliente.MaxLength = 100;
        this.txtCliente.Name = "txtCliente";
        this.txtCliente.PlaceholderText = "Nombre o identificacion";
        this.txtCliente.Size = new System.Drawing.Size(220, 23);
        this.txtCliente.TabIndex = 3;
        this.txtCliente.KeyDown += new KeyEventHandler(this.Filtro_KeyDown);
        //
        this.lblSalon.AutoSize = true;
        this.lblSalon.Location = new System.Drawing.Point(514, 30);
        this.lblSalon.Name = "lblSalon";
        this.lblSalon.Size = new System.Drawing.Size(37, 15);
        this.lblSalon.TabIndex = 4;
        this.lblSalon.Text = "&Salon";
        //
        this.cboSalon.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboSalon.Location = new System.Drawing.Point(558, 27);
        this.cboSalon.Name = "cboSalon";
        this.cboSalon.Size = new System.Drawing.Size(200, 23);
        this.cboSalon.TabIndex = 5;
        //
        this.lblEstado.AutoSize = true;
        this.lblEstado.Location = new System.Drawing.Point(774, 30);
        this.lblEstado.Name = "lblEstado";
        this.lblEstado.Size = new System.Drawing.Size(42, 15);
        this.lblEstado.TabIndex = 6;
        this.lblEstado.Text = "&Estado";
        //
        this.cboEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboEstado.Location = new System.Drawing.Point(822, 27);
        this.cboEstado.Name = "cboEstado";
        this.cboEstado.Size = new System.Drawing.Size(120, 23);
        this.cboEstado.TabIndex = 7;
        //
        this.chkFechas.AutoSize = true;
        this.chkFechas.Location = new System.Drawing.Point(19, 66);
        this.chkFechas.Name = "chkFechas";
        this.chkFechas.Size = new System.Drawing.Size(118, 19);
        this.chkFechas.TabIndex = 8;
        this.chkFechas.Text = "Filtrar por &fechas";
        this.chkFechas.UseVisualStyleBackColor = true;
        this.chkFechas.CheckedChanged += new System.EventHandler(this.chkFechas_CheckedChanged);
        //
        this.dtpDesde.Enabled = false;
        this.dtpDesde.Format = DateTimePickerFormat.Short;
        this.dtpDesde.Location = new System.Drawing.Point(143, 63);
        this.dtpDesde.Name = "dtpDesde";
        this.dtpDesde.Size = new System.Drawing.Size(120, 23);
        this.dtpDesde.TabIndex = 9;
        //
        this.lblHasta.AutoSize = true;
        this.lblHasta.Location = new System.Drawing.Point(272, 66);
        this.lblHasta.Name = "lblHasta";
        this.lblHasta.Size = new System.Drawing.Size(35, 15);
        this.lblHasta.TabIndex = 10;
        this.lblHasta.Text = "hasta";
        //
        this.dtpHasta.Enabled = false;
        this.dtpHasta.Format = DateTimePickerFormat.Short;
        this.dtpHasta.Location = new System.Drawing.Point(313, 63);
        this.dtpHasta.Name = "dtpHasta";
        this.dtpHasta.Size = new System.Drawing.Size(120, 23);
        this.dtpHasta.TabIndex = 11;
        //
        this.btnBuscar.Location = new System.Drawing.Point(558, 62);
        this.btnBuscar.Name = "btnBuscar";
        this.btnBuscar.Size = new System.Drawing.Size(110, 27);
        this.btnBuscar.TabIndex = 12;
        this.btnBuscar.Text = "&Buscar";
        this.btnBuscar.UseVisualStyleBackColor = true;
        this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
        //
        this.btnCancelarBusqueda.Enabled = false;
        this.btnCancelarBusqueda.Location = new System.Drawing.Point(674, 62);
        this.btnCancelarBusqueda.Name = "btnCancelarBusqueda";
        this.btnCancelarBusqueda.Size = new System.Drawing.Size(110, 27);
        this.btnCancelarBusqueda.TabIndex = 13;
        this.btnCancelarBusqueda.Text = "Cancelar busqueda";
        this.btnCancelarBusqueda.UseVisualStyleBackColor = true;
        this.btnCancelarBusqueda.Click += new System.EventHandler(this.btnCancelarBusqueda_Click);
        //
        this.btnLimpiar.Location = new System.Drawing.Point(832, 62);
        this.btnLimpiar.Name = "btnLimpiar";
        this.btnLimpiar.Size = new System.Drawing.Size(110, 27);
        this.btnLimpiar.TabIndex = 14;
        this.btnLimpiar.Text = "&Limpiar filtros";
        this.btnLimpiar.UseVisualStyleBackColor = true;
        this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
        //
        // dgvReservas
        //
        this.dgvReservas.AllowUserToAddRows = false;
        this.dgvReservas.AllowUserToDeleteRows = false;
        this.dgvReservas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvReservas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvReservas.Dock = DockStyle.Fill;
        this.dgvReservas.Location = new System.Drawing.Point(0, 106);
        this.dgvReservas.MultiSelect = false;
        this.dgvReservas.Name = "dgvReservas";
        this.dgvReservas.ReadOnly = true;
        this.dgvReservas.RowHeadersVisible = false;
        this.dgvReservas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvReservas.Size = new System.Drawing.Size(960, 386);
        this.dgvReservas.TabIndex = 1;
        this.dgvReservas.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvReservas_CellDoubleClick);
        this.dgvReservas.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvReservas_CellFormatting);
        //
        // pnlPie
        //
        this.pnlPie.Controls.Add(this.btnAbrir);
        this.pnlPie.Controls.Add(this.btnNueva);
        this.pnlPie.Controls.Add(this.cboTamanoPagina);
        this.pnlPie.Controls.Add(this.lblTamano);
        this.pnlPie.Controls.Add(this.btnSiguiente);
        this.pnlPie.Controls.Add(this.lblPagina);
        this.pnlPie.Controls.Add(this.btnAnterior);
        this.pnlPie.Controls.Add(this.lblResultados);
        this.pnlPie.Dock = DockStyle.Bottom;
        this.pnlPie.Location = new System.Drawing.Point(0, 492);
        this.pnlPie.Name = "pnlPie";
        this.pnlPie.Size = new System.Drawing.Size(960, 44);
        this.pnlPie.TabIndex = 2;
        //
        this.lblResultados.AutoSize = true;
        this.lblResultados.Location = new System.Drawing.Point(16, 14);
        this.lblResultados.Name = "lblResultados";
        this.lblResultados.Size = new System.Drawing.Size(0, 15);
        this.lblResultados.TabIndex = 0;
        //
        this.btnAnterior.Enabled = false;
        this.btnAnterior.Location = new System.Drawing.Point(300, 9);
        this.btnAnterior.Name = "btnAnterior";
        this.btnAnterior.Size = new System.Drawing.Size(90, 26);
        this.btnAnterior.TabIndex = 1;
        this.btnAnterior.Text = "< Anterior";
        this.btnAnterior.UseVisualStyleBackColor = true;
        this.btnAnterior.Click += new System.EventHandler(this.btnAnterior_Click);
        //
        this.lblPagina.Location = new System.Drawing.Point(396, 14);
        this.lblPagina.Name = "lblPagina";
        this.lblPagina.Size = new System.Drawing.Size(120, 17);
        this.lblPagina.TabIndex = 2;
        this.lblPagina.Text = "Pagina 1 de 1";
        this.lblPagina.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        this.btnSiguiente.Enabled = false;
        this.btnSiguiente.Location = new System.Drawing.Point(522, 9);
        this.btnSiguiente.Name = "btnSiguiente";
        this.btnSiguiente.Size = new System.Drawing.Size(90, 26);
        this.btnSiguiente.TabIndex = 3;
        this.btnSiguiente.Text = "Siguiente >";
        this.btnSiguiente.UseVisualStyleBackColor = true;
        this.btnSiguiente.Click += new System.EventHandler(this.btnSiguiente_Click);
        //
        this.lblTamano.AutoSize = true;
        this.lblTamano.Location = new System.Drawing.Point(618, 14);
        this.lblTamano.Name = "lblTamano";
        this.lblTamano.Size = new System.Drawing.Size(66, 15);
        this.lblTamano.TabIndex = 4;
        this.lblTamano.Text = "Por pagina";
        //
        this.cboTamanoPagina.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboTamanoPagina.Location = new System.Drawing.Point(692, 10);
        this.cboTamanoPagina.Name = "cboTamanoPagina";
        this.cboTamanoPagina.Size = new System.Drawing.Size(70, 23);
        this.cboTamanoPagina.TabIndex = 5;
        this.cboTamanoPagina.SelectedIndexChanged += new System.EventHandler(this.cboTamanoPagina_SelectedIndexChanged);
        //
        this.btnNueva.Location = new System.Drawing.Point(766, 9);
        this.btnNueva.Name = "btnNueva";
        this.btnNueva.Size = new System.Drawing.Size(90, 26);
        this.btnNueva.TabIndex = 6;
        this.btnNueva.Text = "&Nueva";
        this.btnNueva.UseVisualStyleBackColor = true;
        this.btnNueva.Click += new System.EventHandler(this.btnNueva_Click);
        //
        this.btnAbrir.Location = new System.Drawing.Point(862, 9);
        this.btnAbrir.Name = "btnAbrir";
        this.btnAbrir.Size = new System.Drawing.Size(90, 26);
        this.btnAbrir.TabIndex = 7;
        this.btnAbrir.Text = "&Abrir";
        this.btnAbrir.UseVisualStyleBackColor = true;
        this.btnAbrir.Click += new System.EventHandler(this.btnAbrir_Click);
        //
        // stbConsulta
        //
        this.stbConsulta.Items.AddRange(new ToolStripItem[] { this.lblMensajeEstado, this.prgBuscando });
        this.stbConsulta.Location = new System.Drawing.Point(0, 536);
        this.stbConsulta.Name = "stbConsulta";
        this.stbConsulta.Size = new System.Drawing.Size(960, 22);
        this.stbConsulta.TabIndex = 3;
        //
        this.lblMensajeEstado.Name = "lblMensajeEstado";
        this.lblMensajeEstado.Size = new System.Drawing.Size(820, 17);
        this.lblMensajeEstado.Spring = true;
        this.lblMensajeEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        this.prgBuscando.Name = "prgBuscando";
        this.prgBuscando.Size = new System.Drawing.Size(120, 16);
        this.prgBuscando.Style = ProgressBarStyle.Marquee;
        this.prgBuscando.Visible = false;
        //
        // FrmReservasConsulta
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(960, 558);
        this.Controls.Add(this.dgvReservas);
        this.Controls.Add(this.pnlPie);
        this.Controls.Add(this.grpFiltros);
        this.Controls.Add(this.stbConsulta);
        this.MinimumSize = new System.Drawing.Size(900, 520);
        this.Name = "FrmReservasConsulta";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Consulta de reservas";
        this.Load += new System.EventHandler(this.FrmReservasConsulta_Load);
        this.FormClosed += new FormClosedEventHandler(this.FrmReservasConsulta_FormClosed);
        this.grpFiltros.ResumeLayout(false);
        this.grpFiltros.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvReservas)).EndInit();
        this.pnlPie.ResumeLayout(false);
        this.pnlPie.PerformLayout();
        this.stbConsulta.ResumeLayout(false);
        this.stbConsulta.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
