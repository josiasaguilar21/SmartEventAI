namespace SmartEvent.UI.Formularios;

partial class FrmReservaEdicion
{
    private System.ComponentModel.IContainer components = null;

    private GroupBox grpCabecera;
    private Label lblCodigoTitulo;
    private Label lblCodigo;
    private Label lblEstadoTitulo;
    private Label lblEstado;
    private Label lblCliente;
    private ComboBox cboCliente;
    private Label lblSalon;
    private ComboBox cboSalon;
    private Label lblCapacidad;
    private Label lblFecha;
    private DateTimePicker dtpFecha;
    private Label lblHoraInicio;
    private DateTimePicker dtpHoraInicio;
    private Label lblHoraFin;
    private DateTimePicker dtpHoraFin;
    private Label lblDuracion;
    private Label lblInvitados;
    private NumericUpDown numInvitados;
    private Label lblObservacion;
    private TextBox txtObservacion;

    private GroupBox grpDetalle;
    private Panel pnlAgregar;
    private Label lblRecurso;
    private ComboBox cboRecurso;
    private Label lblCantidad;
    private NumericUpDown numCantidad;
    private Label lblPrecio;
    private NumericUpDown numPrecio;
    private Label lblDescuentoLinea;
    private NumericUpDown numDescuentoLinea;
    private Button btnAgregarDetalle;
    private Button btnQuitarDetalle;
    private DataGridView dgvDetalle;

    private Panel pnlInferior;
    private GroupBox grpTotales;
    private Label lblSubtotalTitulo;
    private Label lblSubtotal;
    private Label lblDescuentoGlobalTitulo;
    private NumericUpDown numDescuentoGlobal;
    private Label lblImpuestoTitulo;
    private Label lblImpuesto;
    private Label lblTotalTitulo;
    private Label lblTotal;
    private Panel pnlAcciones;
    private Button btnValidar;
    private Button btnGuardar;
    private Button btnAnalizarIA;
    private Button btnConfirmar;
    private Button btnCancelarReserva;
    private Button btnFinalizar;
    private Button btnHistorial;
    private Button btnCerrar;

    private StatusStrip stbReserva;
    private ToolStripStatusLabel lblMensajeEstado;
    private ToolStripProgressBar prgOcupado;

    private DataGridViewTextBoxColumn colIdRecurso;
    private DataGridViewTextBoxColumn colRecurso;
    private DataGridViewTextBoxColumn colTipo;
    private DataGridViewTextBoxColumn colCantidad;
    private DataGridViewTextBoxColumn colPrecio;
    private DataGridViewTextBoxColumn colDescuento;
    private DataGridViewTextBoxColumn colSubtotalLinea;
    private DataGridViewTextBoxColumn colStock;

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
        this.grpCabecera = new GroupBox();
        this.lblCodigoTitulo = new Label();
        this.lblCodigo = new Label();
        this.lblEstadoTitulo = new Label();
        this.lblEstado = new Label();
        this.lblCliente = new Label();
        this.cboCliente = new ComboBox();
        this.lblSalon = new Label();
        this.cboSalon = new ComboBox();
        this.lblCapacidad = new Label();
        this.lblFecha = new Label();
        this.dtpFecha = new DateTimePicker();
        this.lblHoraInicio = new Label();
        this.dtpHoraInicio = new DateTimePicker();
        this.lblHoraFin = new Label();
        this.dtpHoraFin = new DateTimePicker();
        this.lblDuracion = new Label();
        this.lblInvitados = new Label();
        this.numInvitados = new NumericUpDown();
        this.lblObservacion = new Label();
        this.txtObservacion = new TextBox();
        this.grpDetalle = new GroupBox();
        this.dgvDetalle = new DataGridView();
        this.colIdRecurso = new DataGridViewTextBoxColumn();
        this.colRecurso = new DataGridViewTextBoxColumn();
        this.colTipo = new DataGridViewTextBoxColumn();
        this.colCantidad = new DataGridViewTextBoxColumn();
        this.colPrecio = new DataGridViewTextBoxColumn();
        this.colDescuento = new DataGridViewTextBoxColumn();
        this.colSubtotalLinea = new DataGridViewTextBoxColumn();
        this.colStock = new DataGridViewTextBoxColumn();
        this.pnlAgregar = new Panel();
        this.lblRecurso = new Label();
        this.cboRecurso = new ComboBox();
        this.lblCantidad = new Label();
        this.numCantidad = new NumericUpDown();
        this.lblPrecio = new Label();
        this.numPrecio = new NumericUpDown();
        this.lblDescuentoLinea = new Label();
        this.numDescuentoLinea = new NumericUpDown();
        this.btnAgregarDetalle = new Button();
        this.btnQuitarDetalle = new Button();
        this.pnlInferior = new Panel();
        this.pnlAcciones = new Panel();
        this.btnValidar = new Button();
        this.btnGuardar = new Button();
        this.btnAnalizarIA = new Button();
        this.btnConfirmar = new Button();
        this.btnCancelarReserva = new Button();
        this.btnFinalizar = new Button();
        this.btnHistorial = new Button();
        this.btnCerrar = new Button();
        this.grpTotales = new GroupBox();
        this.lblSubtotalTitulo = new Label();
        this.lblSubtotal = new Label();
        this.lblDescuentoGlobalTitulo = new Label();
        this.numDescuentoGlobal = new NumericUpDown();
        this.lblImpuestoTitulo = new Label();
        this.lblImpuesto = new Label();
        this.lblTotalTitulo = new Label();
        this.lblTotal = new Label();
        this.stbReserva = new StatusStrip();
        this.lblMensajeEstado = new ToolStripStatusLabel();
        this.prgOcupado = new ToolStripProgressBar();
        this.grpCabecera.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numInvitados)).BeginInit();
        this.grpDetalle.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).BeginInit();
        this.pnlAgregar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPrecio)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDescuentoLinea)).BeginInit();
        this.pnlInferior.SuspendLayout();
        this.pnlAcciones.SuspendLayout();
        this.grpTotales.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numDescuentoGlobal)).BeginInit();
        this.stbReserva.SuspendLayout();
        this.SuspendLayout();
        //
        // ==================================================================== CABECERA
        //
        this.grpCabecera.Controls.Add(this.txtObservacion);
        this.grpCabecera.Controls.Add(this.lblObservacion);
        this.grpCabecera.Controls.Add(this.numInvitados);
        this.grpCabecera.Controls.Add(this.lblInvitados);
        this.grpCabecera.Controls.Add(this.lblDuracion);
        this.grpCabecera.Controls.Add(this.dtpHoraFin);
        this.grpCabecera.Controls.Add(this.lblHoraFin);
        this.grpCabecera.Controls.Add(this.dtpHoraInicio);
        this.grpCabecera.Controls.Add(this.lblHoraInicio);
        this.grpCabecera.Controls.Add(this.dtpFecha);
        this.grpCabecera.Controls.Add(this.lblFecha);
        this.grpCabecera.Controls.Add(this.lblCapacidad);
        this.grpCabecera.Controls.Add(this.cboSalon);
        this.grpCabecera.Controls.Add(this.lblSalon);
        this.grpCabecera.Controls.Add(this.cboCliente);
        this.grpCabecera.Controls.Add(this.lblCliente);
        this.grpCabecera.Controls.Add(this.lblEstado);
        this.grpCabecera.Controls.Add(this.lblEstadoTitulo);
        this.grpCabecera.Controls.Add(this.lblCodigo);
        this.grpCabecera.Controls.Add(this.lblCodigoTitulo);
        this.grpCabecera.Dock = DockStyle.Top;
        this.grpCabecera.Location = new System.Drawing.Point(0, 0);
        this.grpCabecera.Name = "grpCabecera";
        this.grpCabecera.Size = new System.Drawing.Size(940, 176);
        this.grpCabecera.TabIndex = 0;
        this.grpCabecera.TabStop = false;
        this.grpCabecera.Text = "Datos de la reserva";
        //
        this.lblCodigoTitulo.AutoSize = true;
        this.lblCodigoTitulo.Location = new System.Drawing.Point(16, 24);
        this.lblCodigoTitulo.Name = "lblCodigoTitulo";
        this.lblCodigoTitulo.Size = new System.Drawing.Size(46, 15);
        this.lblCodigoTitulo.TabIndex = 0;
        this.lblCodigoTitulo.Text = "Codigo";
        //
        this.lblCodigo.AutoSize = true;
        this.lblCodigo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblCodigo.Location = new System.Drawing.Point(110, 24);
        this.lblCodigo.Name = "lblCodigo";
        this.lblCodigo.Size = new System.Drawing.Size(69, 15);
        this.lblCodigo.TabIndex = 1;
        this.lblCodigo.Text = "(nueva)";
        //
        this.lblEstadoTitulo.AutoSize = true;
        this.lblEstadoTitulo.Location = new System.Drawing.Point(320, 24);
        this.lblEstadoTitulo.Name = "lblEstadoTitulo";
        this.lblEstadoTitulo.Size = new System.Drawing.Size(42, 15);
        this.lblEstadoTitulo.TabIndex = 2;
        this.lblEstadoTitulo.Text = "Estado";
        //
        this.lblEstado.AutoSize = true;
        this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblEstado.Location = new System.Drawing.Point(380, 24);
        this.lblEstado.Name = "lblEstado";
        this.lblEstado.Size = new System.Drawing.Size(60, 15);
        this.lblEstado.TabIndex = 3;
        this.lblEstado.Text = "Borrador";
        //
        this.lblCliente.AutoSize = true;
        this.lblCliente.Location = new System.Drawing.Point(16, 56);
        this.lblCliente.Name = "lblCliente";
        this.lblCliente.Size = new System.Drawing.Size(45, 15);
        this.lblCliente.TabIndex = 4;
        this.lblCliente.Text = "&Cliente";
        //
        this.cboCliente.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        this.cboCliente.AutoCompleteSource = AutoCompleteSource.ListItems;
        this.cboCliente.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboCliente.Location = new System.Drawing.Point(110, 53);
        this.cboCliente.Name = "cboCliente";
        this.cboCliente.Size = new System.Drawing.Size(380, 23);
        this.cboCliente.TabIndex = 5;
        //
        this.lblSalon.AutoSize = true;
        this.lblSalon.Location = new System.Drawing.Point(510, 56);
        this.lblSalon.Name = "lblSalon";
        this.lblSalon.Size = new System.Drawing.Size(37, 15);
        this.lblSalon.TabIndex = 6;
        this.lblSalon.Text = "&Salon";
        //
        this.cboSalon.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        this.cboSalon.AutoCompleteSource = AutoCompleteSource.ListItems;
        this.cboSalon.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboSalon.Location = new System.Drawing.Point(570, 53);
        this.cboSalon.Name = "cboSalon";
        this.cboSalon.Size = new System.Drawing.Size(250, 23);
        this.cboSalon.TabIndex = 7;
        this.cboSalon.SelectedIndexChanged += new System.EventHandler(this.cboSalon_SelectedIndexChanged);
        //
        this.lblCapacidad.AutoSize = true;
        this.lblCapacidad.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblCapacidad.Location = new System.Drawing.Point(828, 56);
        this.lblCapacidad.Name = "lblCapacidad";
        this.lblCapacidad.Size = new System.Drawing.Size(0, 15);
        this.lblCapacidad.TabIndex = 8;
        //
        this.lblFecha.AutoSize = true;
        this.lblFecha.Location = new System.Drawing.Point(16, 88);
        this.lblFecha.Name = "lblFecha";
        this.lblFecha.Size = new System.Drawing.Size(38, 15);
        this.lblFecha.TabIndex = 9;
        this.lblFecha.Text = "&Fecha";
        //
        this.dtpFecha.Format = DateTimePickerFormat.Short;
        this.dtpFecha.Location = new System.Drawing.Point(110, 85);
        this.dtpFecha.Name = "dtpFecha";
        this.dtpFecha.Size = new System.Drawing.Size(130, 23);
        this.dtpFecha.TabIndex = 10;
        //
        this.lblHoraInicio.AutoSize = true;
        this.lblHoraInicio.Location = new System.Drawing.Point(260, 88);
        this.lblHoraInicio.Name = "lblHoraInicio";
        this.lblHoraInicio.Size = new System.Drawing.Size(64, 15);
        this.lblHoraInicio.TabIndex = 11;
        this.lblHoraInicio.Text = "Hora inicio";
        //
        this.dtpHoraInicio.Format = DateTimePickerFormat.Time;
        this.dtpHoraInicio.Location = new System.Drawing.Point(334, 85);
        this.dtpHoraInicio.Name = "dtpHoraInicio";
        this.dtpHoraInicio.ShowUpDown = true;
        this.dtpHoraInicio.Size = new System.Drawing.Size(100, 23);
        this.dtpHoraInicio.TabIndex = 12;
        this.dtpHoraInicio.ValueChanged += new System.EventHandler(this.Horario_ValueChanged);
        //
        this.lblHoraFin.AutoSize = true;
        this.lblHoraFin.Location = new System.Drawing.Point(450, 88);
        this.lblHoraFin.Name = "lblHoraFin";
        this.lblHoraFin.Size = new System.Drawing.Size(51, 15);
        this.lblHoraFin.TabIndex = 13;
        this.lblHoraFin.Text = "Hora fin";
        //
        this.dtpHoraFin.Format = DateTimePickerFormat.Time;
        this.dtpHoraFin.Location = new System.Drawing.Point(510, 85);
        this.dtpHoraFin.Name = "dtpHoraFin";
        this.dtpHoraFin.ShowUpDown = true;
        this.dtpHoraFin.Size = new System.Drawing.Size(100, 23);
        this.dtpHoraFin.TabIndex = 14;
        this.dtpHoraFin.ValueChanged += new System.EventHandler(this.Horario_ValueChanged);
        //
        this.lblDuracion.AutoSize = true;
        this.lblDuracion.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblDuracion.Location = new System.Drawing.Point(620, 88);
        this.lblDuracion.Name = "lblDuracion";
        this.lblDuracion.Size = new System.Drawing.Size(0, 15);
        this.lblDuracion.TabIndex = 15;
        //
        this.lblInvitados.AutoSize = true;
        this.lblInvitados.Location = new System.Drawing.Point(740, 88);
        this.lblInvitados.Name = "lblInvitados";
        this.lblInvitados.Size = new System.Drawing.Size(56, 15);
        this.lblInvitados.TabIndex = 16;
        this.lblInvitados.Text = "&Invitados";
        //
        this.numInvitados.Location = new System.Drawing.Point(806, 86);
        this.numInvitados.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numInvitados.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numInvitados.Name = "numInvitados";
        this.numInvitados.Size = new System.Drawing.Size(90, 23);
        this.numInvitados.TabIndex = 17;
        this.numInvitados.TextAlign = HorizontalAlignment.Right;
        this.numInvitados.Value = new decimal(new int[] { 20, 0, 0, 0 });
        this.numInvitados.ValueChanged += new System.EventHandler(this.numInvitados_ValueChanged);
        //
        this.lblObservacion.AutoSize = true;
        this.lblObservacion.Location = new System.Drawing.Point(16, 120);
        this.lblObservacion.Name = "lblObservacion";
        this.lblObservacion.Size = new System.Drawing.Size(72, 15);
        this.lblObservacion.TabIndex = 18;
        this.lblObservacion.Text = "&Observacion";
        //
        this.txtObservacion.Location = new System.Drawing.Point(110, 117);
        this.txtObservacion.MaxLength = 500;
        this.txtObservacion.Multiline = true;
        this.txtObservacion.Name = "txtObservacion";
        this.txtObservacion.ScrollBars = ScrollBars.Vertical;
        this.txtObservacion.Size = new System.Drawing.Size(786, 46);
        this.txtObservacion.TabIndex = 19;
        //
        // ===================================================================== DETALLE
        //
        this.grpDetalle.Controls.Add(this.dgvDetalle);
        this.grpDetalle.Controls.Add(this.pnlAgregar);
        this.grpDetalle.Dock = DockStyle.Fill;
        this.grpDetalle.Location = new System.Drawing.Point(0, 176);
        this.grpDetalle.Name = "grpDetalle";
        this.grpDetalle.Padding = new Padding(6);
        this.grpDetalle.Size = new System.Drawing.Size(940, 306);
        this.grpDetalle.TabIndex = 1;
        this.grpDetalle.TabStop = false;
        this.grpDetalle.Text = "Recursos y servicios";
        //
        this.pnlAgregar.Controls.Add(this.btnQuitarDetalle);
        this.pnlAgregar.Controls.Add(this.btnAgregarDetalle);
        this.pnlAgregar.Controls.Add(this.numDescuentoLinea);
        this.pnlAgregar.Controls.Add(this.lblDescuentoLinea);
        this.pnlAgregar.Controls.Add(this.numPrecio);
        this.pnlAgregar.Controls.Add(this.lblPrecio);
        this.pnlAgregar.Controls.Add(this.numCantidad);
        this.pnlAgregar.Controls.Add(this.lblCantidad);
        this.pnlAgregar.Controls.Add(this.cboRecurso);
        this.pnlAgregar.Controls.Add(this.lblRecurso);
        this.pnlAgregar.Dock = DockStyle.Top;
        this.pnlAgregar.Location = new System.Drawing.Point(6, 22);
        this.pnlAgregar.Name = "pnlAgregar";
        this.pnlAgregar.Size = new System.Drawing.Size(928, 42);
        this.pnlAgregar.TabIndex = 0;
        //
        this.lblRecurso.AutoSize = true;
        this.lblRecurso.Location = new System.Drawing.Point(4, 13);
        this.lblRecurso.Name = "lblRecurso";
        this.lblRecurso.Size = new System.Drawing.Size(52, 15);
        this.lblRecurso.TabIndex = 0;
        this.lblRecurso.Text = "&Recurso";
        //
        this.cboRecurso.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        this.cboRecurso.AutoCompleteSource = AutoCompleteSource.ListItems;
        this.cboRecurso.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboRecurso.Location = new System.Drawing.Point(62, 10);
        this.cboRecurso.Name = "cboRecurso";
        this.cboRecurso.Size = new System.Drawing.Size(290, 23);
        this.cboRecurso.TabIndex = 1;
        this.cboRecurso.SelectedIndexChanged += new System.EventHandler(this.cboRecurso_SelectedIndexChanged);
        //
        this.lblCantidad.AutoSize = true;
        this.lblCantidad.Location = new System.Drawing.Point(366, 13);
        this.lblCantidad.Name = "lblCantidad";
        this.lblCantidad.Size = new System.Drawing.Size(56, 15);
        this.lblCantidad.TabIndex = 2;
        this.lblCantidad.Text = "Cantidad";
        //
        this.numCantidad.Location = new System.Drawing.Point(428, 10);
        this.numCantidad.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numCantidad.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numCantidad.Name = "numCantidad";
        this.numCantidad.Size = new System.Drawing.Size(70, 23);
        this.numCantidad.TabIndex = 3;
        this.numCantidad.TextAlign = HorizontalAlignment.Right;
        this.numCantidad.Value = new decimal(new int[] { 1, 0, 0, 0 });
        //
        this.lblPrecio.AutoSize = true;
        this.lblPrecio.Location = new System.Drawing.Point(510, 13);
        this.lblPrecio.Name = "lblPrecio";
        this.lblPrecio.Size = new System.Drawing.Size(40, 15);
        this.lblPrecio.TabIndex = 4;
        this.lblPrecio.Text = "Precio";
        //
        this.numPrecio.DecimalPlaces = 2;
        this.numPrecio.Location = new System.Drawing.Point(556, 10);
        this.numPrecio.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numPrecio.Name = "numPrecio";
        this.numPrecio.Size = new System.Drawing.Size(90, 23);
        this.numPrecio.TabIndex = 5;
        this.numPrecio.TextAlign = HorizontalAlignment.Right;
        //
        this.lblDescuentoLinea.AutoSize = true;
        this.lblDescuentoLinea.Location = new System.Drawing.Point(658, 13);
        this.lblDescuentoLinea.Name = "lblDescuentoLinea";
        this.lblDescuentoLinea.Size = new System.Drawing.Size(62, 15);
        this.lblDescuentoLinea.TabIndex = 6;
        this.lblDescuentoLinea.Text = "Dscto. (%)";
        //
        this.numDescuentoLinea.DecimalPlaces = 2;
        this.numDescuentoLinea.Location = new System.Drawing.Point(726, 10);
        this.numDescuentoLinea.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        this.numDescuentoLinea.Name = "numDescuentoLinea";
        this.numDescuentoLinea.Size = new System.Drawing.Size(70, 23);
        this.numDescuentoLinea.TabIndex = 7;
        this.numDescuentoLinea.TextAlign = HorizontalAlignment.Right;
        //
        this.btnAgregarDetalle.Location = new System.Drawing.Point(806, 9);
        this.btnAgregarDetalle.Name = "btnAgregarDetalle";
        this.btnAgregarDetalle.Size = new System.Drawing.Size(58, 25);
        this.btnAgregarDetalle.TabIndex = 8;
        this.btnAgregarDetalle.Text = "&Agregar";
        this.btnAgregarDetalle.UseVisualStyleBackColor = true;
        this.btnAgregarDetalle.Click += new System.EventHandler(this.btnAgregarDetalle_Click);
        //
        this.btnQuitarDetalle.Location = new System.Drawing.Point(870, 9);
        this.btnQuitarDetalle.Name = "btnQuitarDetalle";
        this.btnQuitarDetalle.Size = new System.Drawing.Size(54, 25);
        this.btnQuitarDetalle.TabIndex = 9;
        this.btnQuitarDetalle.Text = "&Quitar";
        this.btnQuitarDetalle.UseVisualStyleBackColor = true;
        this.btnQuitarDetalle.Click += new System.EventHandler(this.btnQuitarDetalle_Click);
        //
        // dgvDetalle
        //
        this.dgvDetalle.AllowUserToAddRows = false;
        this.dgvDetalle.AllowUserToDeleteRows = false;
        this.dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvDetalle.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvDetalle.Columns.AddRange(new DataGridViewColumn[] {
            this.colIdRecurso, this.colRecurso, this.colTipo, this.colCantidad,
            this.colPrecio, this.colDescuento, this.colSubtotalLinea, this.colStock});
        this.dgvDetalle.Dock = DockStyle.Fill;
        this.dgvDetalle.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        this.dgvDetalle.Location = new System.Drawing.Point(6, 64);
        this.dgvDetalle.MultiSelect = false;
        this.dgvDetalle.Name = "dgvDetalle";
        // El encabezado de fila queda visible a proposito: es donde el DataGridView dibuja el
        // icono de error cuando una celda no supera la validacion.
        this.dgvDetalle.RowHeadersVisible = true;
        this.dgvDetalle.RowHeadersWidth = 28;
        this.dgvDetalle.SelectionMode = DataGridViewSelectionMode.CellSelect;
        this.dgvDetalle.Size = new System.Drawing.Size(928, 236);
        this.dgvDetalle.TabIndex = 1;
        this.dgvDetalle.CellEndEdit += new DataGridViewCellEventHandler(this.dgvDetalle_CellEndEdit);
        this.dgvDetalle.CellValidating += new DataGridViewCellValidatingEventHandler(this.dgvDetalle_CellValidating);
        this.dgvDetalle.DataError += new DataGridViewDataErrorEventHandler(this.dgvDetalle_DataError);
        //
        this.colIdRecurso.HeaderText = "IdRecurso";
        this.colIdRecurso.Name = "colIdRecurso";
        this.colIdRecurso.Visible = false;
        //
        this.colRecurso.HeaderText = "Recurso";
        this.colRecurso.Name = "colRecurso";
        this.colRecurso.ReadOnly = true;
        this.colRecurso.FillWeight = 160F;
        //
        this.colTipo.HeaderText = "Tipo";
        this.colTipo.Name = "colTipo";
        this.colTipo.ReadOnly = true;
        this.colTipo.FillWeight = 70F;
        //
        this.colCantidad.HeaderText = "Cantidad";
        this.colCantidad.Name = "colCantidad";
        this.colCantidad.FillWeight = 60F;
        this.colCantidad.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        //
        this.colPrecio.HeaderText = "Precio unitario";
        this.colPrecio.Name = "colPrecio";
        this.colPrecio.FillWeight = 75F;
        this.colPrecio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        this.colPrecio.DefaultCellStyle.Format = "N2";
        //
        this.colDescuento.HeaderText = "Dscto. (%)";
        this.colDescuento.Name = "colDescuento";
        this.colDescuento.FillWeight = 60F;
        this.colDescuento.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        //
        this.colSubtotalLinea.HeaderText = "Subtotal";
        this.colSubtotalLinea.Name = "colSubtotalLinea";
        this.colSubtotalLinea.ReadOnly = true;
        this.colSubtotalLinea.FillWeight = 75F;
        this.colSubtotalLinea.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        this.colSubtotalLinea.DefaultCellStyle.Format = "N2";
        this.colSubtotalLinea.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        //
        this.colStock.HeaderText = "Stock";
        this.colStock.Name = "colStock";
        this.colStock.Visible = false;
        //
        // =============================================================== PANEL INFERIOR
        //
        this.pnlInferior.Controls.Add(this.pnlAcciones);
        this.pnlInferior.Controls.Add(this.grpTotales);
        this.pnlInferior.Dock = DockStyle.Bottom;
        this.pnlInferior.Location = new System.Drawing.Point(0, 482);
        this.pnlInferior.Name = "pnlInferior";
        this.pnlInferior.Size = new System.Drawing.Size(940, 140);
        this.pnlInferior.TabIndex = 2;
        //
        this.grpTotales.Controls.Add(this.lblTotal);
        this.grpTotales.Controls.Add(this.lblTotalTitulo);
        this.grpTotales.Controls.Add(this.lblImpuesto);
        this.grpTotales.Controls.Add(this.lblImpuestoTitulo);
        this.grpTotales.Controls.Add(this.numDescuentoGlobal);
        this.grpTotales.Controls.Add(this.lblDescuentoGlobalTitulo);
        this.grpTotales.Controls.Add(this.lblSubtotal);
        this.grpTotales.Controls.Add(this.lblSubtotalTitulo);
        this.grpTotales.Dock = DockStyle.Left;
        this.grpTotales.Location = new System.Drawing.Point(0, 0);
        this.grpTotales.Name = "grpTotales";
        this.grpTotales.Size = new System.Drawing.Size(330, 140);
        this.grpTotales.TabIndex = 0;
        this.grpTotales.TabStop = false;
        this.grpTotales.Text = "Totales";
        //
        this.lblSubtotalTitulo.AutoSize = true;
        this.lblSubtotalTitulo.Location = new System.Drawing.Point(16, 26);
        this.lblSubtotalTitulo.Name = "lblSubtotalTitulo";
        this.lblSubtotalTitulo.Size = new System.Drawing.Size(52, 15);
        this.lblSubtotalTitulo.TabIndex = 0;
        this.lblSubtotalTitulo.Text = "Subtotal";
        //
        this.lblSubtotal.Location = new System.Drawing.Point(150, 26);
        this.lblSubtotal.Name = "lblSubtotal";
        this.lblSubtotal.Size = new System.Drawing.Size(160, 18);
        this.lblSubtotal.TabIndex = 1;
        this.lblSubtotal.Text = "0,00";
        this.lblSubtotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        this.lblDescuentoGlobalTitulo.AutoSize = true;
        this.lblDescuentoGlobalTitulo.Location = new System.Drawing.Point(16, 52);
        this.lblDescuentoGlobalTitulo.Name = "lblDescuentoGlobalTitulo";
        this.lblDescuentoGlobalTitulo.Size = new System.Drawing.Size(102, 15);
        this.lblDescuentoGlobalTitulo.TabIndex = 2;
        this.lblDescuentoGlobalTitulo.Text = "&Descuento global";
        //
        this.numDescuentoGlobal.DecimalPlaces = 2;
        this.numDescuentoGlobal.Location = new System.Drawing.Point(190, 50);
        this.numDescuentoGlobal.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numDescuentoGlobal.Name = "numDescuentoGlobal";
        this.numDescuentoGlobal.Size = new System.Drawing.Size(120, 23);
        this.numDescuentoGlobal.TabIndex = 3;
        this.numDescuentoGlobal.TextAlign = HorizontalAlignment.Right;
        this.numDescuentoGlobal.ValueChanged += new System.EventHandler(this.numDescuentoGlobal_ValueChanged);
        //
        this.lblImpuestoTitulo.AutoSize = true;
        this.lblImpuestoTitulo.Location = new System.Drawing.Point(16, 82);
        this.lblImpuestoTitulo.Name = "lblImpuestoTitulo";
        this.lblImpuestoTitulo.Size = new System.Drawing.Size(96, 15);
        this.lblImpuestoTitulo.TabIndex = 4;
        this.lblImpuestoTitulo.Text = "Impuesto (15 %)";
        //
        this.lblImpuesto.Location = new System.Drawing.Point(150, 82);
        this.lblImpuesto.Name = "lblImpuesto";
        this.lblImpuesto.Size = new System.Drawing.Size(160, 18);
        this.lblImpuesto.TabIndex = 5;
        this.lblImpuesto.Text = "0,00";
        this.lblImpuesto.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        this.lblTotalTitulo.AutoSize = true;
        this.lblTotalTitulo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.lblTotalTitulo.Location = new System.Drawing.Point(16, 108);
        this.lblTotalTitulo.Name = "lblTotalTitulo";
        this.lblTotalTitulo.Size = new System.Drawing.Size(44, 19);
        this.lblTotalTitulo.TabIndex = 6;
        this.lblTotalTitulo.Text = "TOTAL";
        //
        this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.lblTotal.Location = new System.Drawing.Point(140, 108);
        this.lblTotal.Name = "lblTotal";
        this.lblTotal.Size = new System.Drawing.Size(170, 22);
        this.lblTotal.TabIndex = 7;
        this.lblTotal.Text = "0,00";
        this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        //
        this.pnlAcciones.Controls.Add(this.btnCerrar);
        this.pnlAcciones.Controls.Add(this.btnHistorial);
        this.pnlAcciones.Controls.Add(this.btnFinalizar);
        this.pnlAcciones.Controls.Add(this.btnCancelarReserva);
        this.pnlAcciones.Controls.Add(this.btnConfirmar);
        this.pnlAcciones.Controls.Add(this.btnAnalizarIA);
        this.pnlAcciones.Controls.Add(this.btnGuardar);
        this.pnlAcciones.Controls.Add(this.btnValidar);
        this.pnlAcciones.Dock = DockStyle.Fill;
        this.pnlAcciones.Location = new System.Drawing.Point(330, 0);
        this.pnlAcciones.Name = "pnlAcciones";
        this.pnlAcciones.Size = new System.Drawing.Size(610, 140);
        this.pnlAcciones.TabIndex = 1;
        //
        this.btnValidar.Location = new System.Drawing.Point(16, 24);
        this.btnValidar.Name = "btnValidar";
        this.btnValidar.Size = new System.Drawing.Size(180, 32);
        this.btnValidar.TabIndex = 0;
        this.btnValidar.Text = "&Validar disponibilidad";
        this.btnValidar.UseVisualStyleBackColor = true;
        this.btnValidar.Click += new System.EventHandler(this.btnValidar_Click);
        //
        this.btnGuardar.Location = new System.Drawing.Point(202, 24);
        this.btnGuardar.Name = "btnGuardar";
        this.btnGuardar.Size = new System.Drawing.Size(180, 32);
        this.btnGuardar.TabIndex = 1;
        this.btnGuardar.Text = "&Guardar reserva";
        this.btnGuardar.UseVisualStyleBackColor = true;
        this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
        //
        this.btnAnalizarIA.Location = new System.Drawing.Point(388, 24);
        this.btnAnalizarIA.Name = "btnAnalizarIA";
        this.btnAnalizarIA.Size = new System.Drawing.Size(180, 32);
        this.btnAnalizarIA.TabIndex = 2;
        this.btnAnalizarIA.Text = "Analizar reserva con &IA";
        this.btnAnalizarIA.UseVisualStyleBackColor = true;
        this.btnAnalizarIA.Click += new System.EventHandler(this.btnAnalizarIA_Click);
        //
        this.btnConfirmar.Location = new System.Drawing.Point(16, 66);
        this.btnConfirmar.Name = "btnConfirmar";
        this.btnConfirmar.Size = new System.Drawing.Size(180, 32);
        this.btnConfirmar.TabIndex = 3;
        this.btnConfirmar.Text = "&Confirmar reserva";
        this.btnConfirmar.UseVisualStyleBackColor = true;
        this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);
        //
        this.btnCancelarReserva.Location = new System.Drawing.Point(202, 66);
        this.btnCancelarReserva.Name = "btnCancelarReserva";
        this.btnCancelarReserva.Size = new System.Drawing.Size(180, 32);
        this.btnCancelarReserva.TabIndex = 4;
        this.btnCancelarReserva.Text = "Cance&lar reserva";
        this.btnCancelarReserva.UseVisualStyleBackColor = true;
        this.btnCancelarReserva.Click += new System.EventHandler(this.btnCancelarReserva_Click);
        //
        this.btnFinalizar.Location = new System.Drawing.Point(388, 66);
        this.btnFinalizar.Name = "btnFinalizar";
        this.btnFinalizar.Size = new System.Drawing.Size(180, 32);
        this.btnFinalizar.TabIndex = 5;
        this.btnFinalizar.Text = "&Finalizar evento";
        this.btnFinalizar.UseVisualStyleBackColor = true;
        this.btnFinalizar.Click += new System.EventHandler(this.btnFinalizar_Click);
        //
        this.btnHistorial.Location = new System.Drawing.Point(16, 104);
        this.btnHistorial.Name = "btnHistorial";
        this.btnHistorial.Size = new System.Drawing.Size(180, 28);
        this.btnHistorial.TabIndex = 6;
        this.btnHistorial.Text = "&Historial de estados";
        this.btnHistorial.UseVisualStyleBackColor = true;
        this.btnHistorial.Click += new System.EventHandler(this.btnHistorial_Click);
        //
        // btnCerrar
        //
        this.btnCerrar.Location = new System.Drawing.Point(478, 104);
        this.btnCerrar.Name = "btnCerrar";
        this.btnCerrar.Size = new System.Drawing.Size(90, 28);
        this.btnCerrar.TabIndex = 7;
        this.btnCerrar.Text = "Cerrar";
        this.btnCerrar.UseVisualStyleBackColor = true;
        this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
        //
        // stbReserva
        //
        this.stbReserva.Items.AddRange(new ToolStripItem[] { this.lblMensajeEstado, this.prgOcupado });
        this.stbReserva.Location = new System.Drawing.Point(0, 622);
        this.stbReserva.Name = "stbReserva";
        this.stbReserva.Size = new System.Drawing.Size(940, 22);
        this.stbReserva.TabIndex = 3;
        //
        this.lblMensajeEstado.Name = "lblMensajeEstado";
        this.lblMensajeEstado.Size = new System.Drawing.Size(800, 17);
        this.lblMensajeEstado.Spring = true;
        this.lblMensajeEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        this.prgOcupado.Name = "prgOcupado";
        this.prgOcupado.Size = new System.Drawing.Size(120, 16);
        this.prgOcupado.Style = ProgressBarStyle.Marquee;
        this.prgOcupado.Visible = false;
        //
        // FrmReservaEdicion
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(940, 644);
        this.Controls.Add(this.grpDetalle);
        this.Controls.Add(this.pnlInferior);
        this.Controls.Add(this.grpCabecera);
        this.Controls.Add(this.stbReserva);
        this.MinimumSize = new System.Drawing.Size(960, 620);
        this.Name = "FrmReservaEdicion";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Reserva";
        this.Load += new System.EventHandler(this.FrmReservaEdicion_Load);
        this.FormClosing += new FormClosingEventHandler(this.FrmReservaEdicion_FormClosing);
        this.grpCabecera.ResumeLayout(false);
        this.grpCabecera.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numInvitados)).EndInit();
        this.grpDetalle.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle)).EndInit();
        this.pnlAgregar.ResumeLayout(false);
        this.pnlAgregar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPrecio)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDescuentoLinea)).EndInit();
        this.pnlInferior.ResumeLayout(false);
        this.pnlAcciones.ResumeLayout(false);
        this.grpTotales.ResumeLayout(false);
        this.grpTotales.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numDescuentoGlobal)).EndInit();
        this.stbReserva.ResumeLayout(false);
        this.stbReserva.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
