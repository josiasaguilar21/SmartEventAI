namespace SmartEvent.UI.Formularios;

partial class FrmCatalogos
{
    private System.ComponentModel.IContainer components = null;

    private TabControl tabCatalogos;

    // --------------------------------------------------------------------------- clientes
    private TabPage tabClientes;
    private DataGridView dgvClientes;
    private Panel pnlFiltroClientes;
    private Label lblBuscarCliente;
    private TextBox txtFiltroCliente;
    private Label lblEstadoCliente;
    private ComboBox cboEstadoCliente;
    private Button btnBuscarClientes;
    private Button btnNuevoCliente;
    private GroupBox grpCliente;
    private Label lblIdentificacion;
    private TextBox txtIdentificacion;
    private Label lblNombresCliente;
    private TextBox txtNombresCliente;
    private Label lblEmailCliente;
    private TextBox txtEmailCliente;
    private Label lblTelefonoCliente;
    private TextBox txtTelefonoCliente;
    private CheckBox chkClienteActivo;
    private Button btnGuardarCliente;
    private Button btnCancelarCliente;
    private Button btnEstadoCliente;

    // ---------------------------------------------------------------------------- salones
    private TabPage tabSalones;
    private DataGridView dgvSalones;
    private Panel pnlFiltroSalones;
    private Label lblBuscarSalon;
    private TextBox txtFiltroSalon;
    private Label lblEstadoSalon;
    private ComboBox cboEstadoSalon;
    private Button btnBuscarSalones;
    private Button btnNuevoSalon;
    private GroupBox grpSalon;
    private Label lblNombreSalon;
    private TextBox txtNombreSalon;
    private Label lblUbicacionSalon;
    private TextBox txtUbicacionSalon;
    private Label lblCapacidadSalon;
    private NumericUpDown numCapacidadSalon;
    private Label lblTarifaSalon;
    private NumericUpDown numTarifaSalon;
    private CheckBox chkSalonActivo;
    private Button btnGuardarSalon;
    private Button btnCancelarSalon;
    private Button btnEstadoSalon;

    // --------------------------------------------------------------------------- recursos
    private TabPage tabRecursos;
    private DataGridView dgvRecursos;
    private Panel pnlFiltroRecursos;
    private Label lblBuscarRecurso;
    private TextBox txtFiltroRecurso;
    private Label lblTipoFiltroRecurso;
    private ComboBox cboTipoFiltroRecurso;
    private Label lblEstadoRecurso;
    private ComboBox cboEstadoRecurso;
    private Button btnBuscarRecursos;
    private Button btnNuevoRecurso;
    private GroupBox grpRecurso;
    private Label lblNombreRecurso;
    private TextBox txtNombreRecurso;
    private Label lblTipoRecurso;
    private ComboBox cboTipoRecurso;
    private Label lblStockRecurso;
    private NumericUpDown numStockRecurso;
    private Label lblPrecioRecurso;
    private NumericUpDown numPrecioRecurso;
    private CheckBox chkRecursoActivo;
    private Button btnGuardarRecurso;
    private Button btnCancelarRecurso;
    private Button btnEstadoRecurso;

    private StatusStrip stbCatalogos;
    private ToolStripStatusLabel lblEstadoOperacion;

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
        this.tabCatalogos = new TabControl();
        this.tabClientes = new TabPage();
        this.dgvClientes = new DataGridView();
        this.grpCliente = new GroupBox();
        this.pnlFiltroClientes = new Panel();
        this.lblBuscarCliente = new Label();
        this.txtFiltroCliente = new TextBox();
        this.lblEstadoCliente = new Label();
        this.cboEstadoCliente = new ComboBox();
        this.btnBuscarClientes = new Button();
        this.btnNuevoCliente = new Button();
        this.lblIdentificacion = new Label();
        this.txtIdentificacion = new TextBox();
        this.lblNombresCliente = new Label();
        this.txtNombresCliente = new TextBox();
        this.lblEmailCliente = new Label();
        this.txtEmailCliente = new TextBox();
        this.lblTelefonoCliente = new Label();
        this.txtTelefonoCliente = new TextBox();
        this.chkClienteActivo = new CheckBox();
        this.btnGuardarCliente = new Button();
        this.btnCancelarCliente = new Button();
        this.btnEstadoCliente = new Button();
        this.tabSalones = new TabPage();
        this.dgvSalones = new DataGridView();
        this.grpSalon = new GroupBox();
        this.pnlFiltroSalones = new Panel();
        this.lblBuscarSalon = new Label();
        this.txtFiltroSalon = new TextBox();
        this.lblEstadoSalon = new Label();
        this.cboEstadoSalon = new ComboBox();
        this.btnBuscarSalones = new Button();
        this.btnNuevoSalon = new Button();
        this.lblNombreSalon = new Label();
        this.txtNombreSalon = new TextBox();
        this.lblUbicacionSalon = new Label();
        this.txtUbicacionSalon = new TextBox();
        this.lblCapacidadSalon = new Label();
        this.numCapacidadSalon = new NumericUpDown();
        this.lblTarifaSalon = new Label();
        this.numTarifaSalon = new NumericUpDown();
        this.chkSalonActivo = new CheckBox();
        this.btnGuardarSalon = new Button();
        this.btnCancelarSalon = new Button();
        this.btnEstadoSalon = new Button();
        this.tabRecursos = new TabPage();
        this.dgvRecursos = new DataGridView();
        this.grpRecurso = new GroupBox();
        this.pnlFiltroRecursos = new Panel();
        this.lblBuscarRecurso = new Label();
        this.txtFiltroRecurso = new TextBox();
        this.lblTipoFiltroRecurso = new Label();
        this.cboTipoFiltroRecurso = new ComboBox();
        this.lblEstadoRecurso = new Label();
        this.cboEstadoRecurso = new ComboBox();
        this.btnBuscarRecursos = new Button();
        this.btnNuevoRecurso = new Button();
        this.lblNombreRecurso = new Label();
        this.txtNombreRecurso = new TextBox();
        this.lblTipoRecurso = new Label();
        this.cboTipoRecurso = new ComboBox();
        this.lblStockRecurso = new Label();
        this.numStockRecurso = new NumericUpDown();
        this.lblPrecioRecurso = new Label();
        this.numPrecioRecurso = new NumericUpDown();
        this.chkRecursoActivo = new CheckBox();
        this.btnGuardarRecurso = new Button();
        this.btnCancelarRecurso = new Button();
        this.btnEstadoRecurso = new Button();
        this.stbCatalogos = new StatusStrip();
        this.lblEstadoOperacion = new ToolStripStatusLabel();
        this.tabCatalogos.SuspendLayout();
        this.tabClientes.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).BeginInit();
        this.grpCliente.SuspendLayout();
        this.pnlFiltroClientes.SuspendLayout();
        this.tabSalones.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvSalones)).BeginInit();
        this.grpSalon.SuspendLayout();
        this.pnlFiltroSalones.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numCapacidadSalon)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numTarifaSalon)).BeginInit();
        this.tabRecursos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRecursos)).BeginInit();
        this.grpRecurso.SuspendLayout();
        this.pnlFiltroRecursos.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numStockRecurso)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPrecioRecurso)).BeginInit();
        this.stbCatalogos.SuspendLayout();
        this.SuspendLayout();
        //
        // tabCatalogos
        //
        this.tabCatalogos.Controls.Add(this.tabClientes);
        this.tabCatalogos.Controls.Add(this.tabSalones);
        this.tabCatalogos.Controls.Add(this.tabRecursos);
        this.tabCatalogos.Dock = DockStyle.Fill;
        this.tabCatalogos.Location = new System.Drawing.Point(0, 0);
        this.tabCatalogos.Name = "tabCatalogos";
        this.tabCatalogos.SelectedIndex = 0;
        this.tabCatalogos.Size = new System.Drawing.Size(900, 538);
        this.tabCatalogos.TabIndex = 0;
        this.tabCatalogos.SelectedIndexChanged += new System.EventHandler(this.tabCatalogos_SelectedIndexChanged);
        //
        // ===================================================================== CLIENTES
        //
        this.tabClientes.Controls.Add(this.dgvClientes);
        this.tabClientes.Controls.Add(this.grpCliente);
        this.tabClientes.Controls.Add(this.pnlFiltroClientes);
        this.tabClientes.Location = new System.Drawing.Point(4, 24);
        this.tabClientes.Name = "tabClientes";
        this.tabClientes.Padding = new Padding(3);
        this.tabClientes.Size = new System.Drawing.Size(892, 510);
        this.tabClientes.TabIndex = 0;
        this.tabClientes.Text = "Clientes";
        this.tabClientes.UseVisualStyleBackColor = true;
        //
        // pnlFiltroClientes
        //
        this.pnlFiltroClientes.Controls.Add(this.btnNuevoCliente);
        this.pnlFiltroClientes.Controls.Add(this.btnBuscarClientes);
        this.pnlFiltroClientes.Controls.Add(this.cboEstadoCliente);
        this.pnlFiltroClientes.Controls.Add(this.lblEstadoCliente);
        this.pnlFiltroClientes.Controls.Add(this.txtFiltroCliente);
        this.pnlFiltroClientes.Controls.Add(this.lblBuscarCliente);
        this.pnlFiltroClientes.Dock = DockStyle.Top;
        this.pnlFiltroClientes.Location = new System.Drawing.Point(3, 3);
        this.pnlFiltroClientes.Name = "pnlFiltroClientes";
        this.pnlFiltroClientes.Size = new System.Drawing.Size(886, 42);
        this.pnlFiltroClientes.TabIndex = 0;
        //
        this.lblBuscarCliente.AutoSize = true;
        this.lblBuscarCliente.Location = new System.Drawing.Point(6, 13);
        this.lblBuscarCliente.Name = "lblBuscarCliente";
        this.lblBuscarCliente.Size = new System.Drawing.Size(43, 15);
        this.lblBuscarCliente.TabIndex = 0;
        this.lblBuscarCliente.Text = "&Buscar";
        //
        this.txtFiltroCliente.Location = new System.Drawing.Point(55, 10);
        this.txtFiltroCliente.MaxLength = 100;
        this.txtFiltroCliente.Name = "txtFiltroCliente";
        this.txtFiltroCliente.PlaceholderText = "Identificacion, nombre o correo";
        this.txtFiltroCliente.Size = new System.Drawing.Size(260, 23);
        this.txtFiltroCliente.TabIndex = 1;
        this.txtFiltroCliente.KeyDown += new KeyEventHandler(this.txtFiltroCliente_KeyDown);
        //
        this.lblEstadoCliente.AutoSize = true;
        this.lblEstadoCliente.Location = new System.Drawing.Point(330, 13);
        this.lblEstadoCliente.Name = "lblEstadoCliente";
        this.lblEstadoCliente.Size = new System.Drawing.Size(42, 15);
        this.lblEstadoCliente.TabIndex = 2;
        this.lblEstadoCliente.Text = "&Estado";
        //
        this.cboEstadoCliente.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboEstadoCliente.Location = new System.Drawing.Point(378, 10);
        this.cboEstadoCliente.Name = "cboEstadoCliente";
        this.cboEstadoCliente.Size = new System.Drawing.Size(130, 23);
        this.cboEstadoCliente.TabIndex = 3;
        //
        this.btnBuscarClientes.Location = new System.Drawing.Point(520, 9);
        this.btnBuscarClientes.Name = "btnBuscarClientes";
        this.btnBuscarClientes.Size = new System.Drawing.Size(90, 25);
        this.btnBuscarClientes.TabIndex = 4;
        this.btnBuscarClientes.Text = "Buscar";
        this.btnBuscarClientes.UseVisualStyleBackColor = true;
        this.btnBuscarClientes.Click += new System.EventHandler(this.btnBuscarClientes_Click);
        //
        this.btnNuevoCliente.Location = new System.Drawing.Point(616, 9);
        this.btnNuevoCliente.Name = "btnNuevoCliente";
        this.btnNuevoCliente.Size = new System.Drawing.Size(90, 25);
        this.btnNuevoCliente.TabIndex = 5;
        this.btnNuevoCliente.Text = "Nuevo";
        this.btnNuevoCliente.UseVisualStyleBackColor = true;
        this.btnNuevoCliente.Click += new System.EventHandler(this.btnNuevoCliente_Click);
        //
        // dgvClientes
        //
        this.dgvClientes.AllowUserToAddRows = false;
        this.dgvClientes.AllowUserToDeleteRows = false;
        this.dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvClientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvClientes.Dock = DockStyle.Fill;
        this.dgvClientes.Location = new System.Drawing.Point(3, 45);
        this.dgvClientes.MultiSelect = false;
        this.dgvClientes.Name = "dgvClientes";
        this.dgvClientes.ReadOnly = true;
        this.dgvClientes.RowHeadersVisible = false;
        this.dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvClientes.Size = new System.Drawing.Size(886, 288);
        this.dgvClientes.TabIndex = 1;
        this.dgvClientes.SelectionChanged += new System.EventHandler(this.dgvClientes_SelectionChanged);
        //
        // grpCliente
        //
        this.grpCliente.Controls.Add(this.btnEstadoCliente);
        this.grpCliente.Controls.Add(this.btnCancelarCliente);
        this.grpCliente.Controls.Add(this.btnGuardarCliente);
        this.grpCliente.Controls.Add(this.chkClienteActivo);
        this.grpCliente.Controls.Add(this.txtTelefonoCliente);
        this.grpCliente.Controls.Add(this.lblTelefonoCliente);
        this.grpCliente.Controls.Add(this.txtEmailCliente);
        this.grpCliente.Controls.Add(this.lblEmailCliente);
        this.grpCliente.Controls.Add(this.txtNombresCliente);
        this.grpCliente.Controls.Add(this.lblNombresCliente);
        this.grpCliente.Controls.Add(this.txtIdentificacion);
        this.grpCliente.Controls.Add(this.lblIdentificacion);
        this.grpCliente.Dock = DockStyle.Bottom;
        this.grpCliente.Location = new System.Drawing.Point(3, 333);
        this.grpCliente.Name = "grpCliente";
        this.grpCliente.Size = new System.Drawing.Size(886, 174);
        this.grpCliente.TabIndex = 2;
        this.grpCliente.TabStop = false;
        this.grpCliente.Text = "Datos del cliente";
        //
        this.lblIdentificacion.AutoSize = true;
        this.lblIdentificacion.Location = new System.Drawing.Point(16, 30);
        this.lblIdentificacion.Name = "lblIdentificacion";
        this.lblIdentificacion.Size = new System.Drawing.Size(80, 15);
        this.lblIdentificacion.TabIndex = 0;
        this.lblIdentificacion.Text = "Identificacion";
        //
        this.txtIdentificacion.Location = new System.Drawing.Point(120, 27);
        this.txtIdentificacion.MaxLength = 20;
        this.txtIdentificacion.Name = "txtIdentificacion";
        this.txtIdentificacion.Size = new System.Drawing.Size(200, 23);
        this.txtIdentificacion.TabIndex = 1;
        //
        this.lblNombresCliente.AutoSize = true;
        this.lblNombresCliente.Location = new System.Drawing.Point(16, 62);
        this.lblNombresCliente.Name = "lblNombresCliente";
        this.lblNombresCliente.Size = new System.Drawing.Size(78, 15);
        this.lblNombresCliente.TabIndex = 2;
        this.lblNombresCliente.Text = "Razon social";
        //
        this.txtNombresCliente.Location = new System.Drawing.Point(120, 59);
        this.txtNombresCliente.MaxLength = 150;
        this.txtNombresCliente.Name = "txtNombresCliente";
        this.txtNombresCliente.Size = new System.Drawing.Size(400, 23);
        this.txtNombresCliente.TabIndex = 3;
        //
        this.lblEmailCliente.AutoSize = true;
        this.lblEmailCliente.Location = new System.Drawing.Point(16, 94);
        this.lblEmailCliente.Name = "lblEmailCliente";
        this.lblEmailCliente.Size = new System.Drawing.Size(101, 15);
        this.lblEmailCliente.TabIndex = 4;
        this.lblEmailCliente.Text = "Correo electronico";
        //
        this.txtEmailCliente.Location = new System.Drawing.Point(120, 91);
        this.txtEmailCliente.MaxLength = 150;
        this.txtEmailCliente.Name = "txtEmailCliente";
        this.txtEmailCliente.Size = new System.Drawing.Size(300, 23);
        this.txtEmailCliente.TabIndex = 5;
        //
        this.lblTelefonoCliente.AutoSize = true;
        this.lblTelefonoCliente.Location = new System.Drawing.Point(450, 94);
        this.lblTelefonoCliente.Name = "lblTelefonoCliente";
        this.lblTelefonoCliente.Size = new System.Drawing.Size(52, 15);
        this.lblTelefonoCliente.TabIndex = 6;
        this.lblTelefonoCliente.Text = "Telefono";
        //
        this.txtTelefonoCliente.Location = new System.Drawing.Point(520, 91);
        this.txtTelefonoCliente.MaxLength = 20;
        this.txtTelefonoCliente.Name = "txtTelefonoCliente";
        this.txtTelefonoCliente.Size = new System.Drawing.Size(160, 23);
        this.txtTelefonoCliente.TabIndex = 7;
        //
        this.chkClienteActivo.AutoSize = true;
        this.chkClienteActivo.Checked = true;
        this.chkClienteActivo.CheckState = CheckState.Checked;
        this.chkClienteActivo.Location = new System.Drawing.Point(120, 126);
        this.chkClienteActivo.Name = "chkClienteActivo";
        this.chkClienteActivo.Size = new System.Drawing.Size(60, 19);
        this.chkClienteActivo.TabIndex = 8;
        this.chkClienteActivo.Text = "Activo";
        this.chkClienteActivo.UseVisualStyleBackColor = true;
        //
        this.btnGuardarCliente.Location = new System.Drawing.Point(560, 124);
        this.btnGuardarCliente.Name = "btnGuardarCliente";
        this.btnGuardarCliente.Size = new System.Drawing.Size(100, 28);
        this.btnGuardarCliente.TabIndex = 9;
        this.btnGuardarCliente.Text = "Guardar";
        this.btnGuardarCliente.UseVisualStyleBackColor = true;
        this.btnGuardarCliente.Click += new System.EventHandler(this.btnGuardarCliente_Click);
        //
        this.btnCancelarCliente.Location = new System.Drawing.Point(666, 124);
        this.btnCancelarCliente.Name = "btnCancelarCliente";
        this.btnCancelarCliente.Size = new System.Drawing.Size(100, 28);
        this.btnCancelarCliente.TabIndex = 10;
        this.btnCancelarCliente.Text = "Cancelar";
        this.btnCancelarCliente.UseVisualStyleBackColor = true;
        this.btnCancelarCliente.Click += new System.EventHandler(this.btnCancelarCliente_Click);
        //
        this.btnEstadoCliente.Location = new System.Drawing.Point(772, 124);
        this.btnEstadoCliente.Name = "btnEstadoCliente";
        this.btnEstadoCliente.Size = new System.Drawing.Size(100, 28);
        this.btnEstadoCliente.TabIndex = 11;
        this.btnEstadoCliente.Text = "Inactivar";
        this.btnEstadoCliente.UseVisualStyleBackColor = true;
        this.btnEstadoCliente.Click += new System.EventHandler(this.btnEstadoCliente_Click);
        //
        // ====================================================================== SALONES
        //
        this.tabSalones.Controls.Add(this.dgvSalones);
        this.tabSalones.Controls.Add(this.grpSalon);
        this.tabSalones.Controls.Add(this.pnlFiltroSalones);
        this.tabSalones.Location = new System.Drawing.Point(4, 24);
        this.tabSalones.Name = "tabSalones";
        this.tabSalones.Padding = new Padding(3);
        this.tabSalones.Size = new System.Drawing.Size(892, 510);
        this.tabSalones.TabIndex = 1;
        this.tabSalones.Text = "Salones";
        this.tabSalones.UseVisualStyleBackColor = true;
        //
        this.pnlFiltroSalones.Controls.Add(this.btnNuevoSalon);
        this.pnlFiltroSalones.Controls.Add(this.btnBuscarSalones);
        this.pnlFiltroSalones.Controls.Add(this.cboEstadoSalon);
        this.pnlFiltroSalones.Controls.Add(this.lblEstadoSalon);
        this.pnlFiltroSalones.Controls.Add(this.txtFiltroSalon);
        this.pnlFiltroSalones.Controls.Add(this.lblBuscarSalon);
        this.pnlFiltroSalones.Dock = DockStyle.Top;
        this.pnlFiltroSalones.Location = new System.Drawing.Point(3, 3);
        this.pnlFiltroSalones.Name = "pnlFiltroSalones";
        this.pnlFiltroSalones.Size = new System.Drawing.Size(886, 42);
        this.pnlFiltroSalones.TabIndex = 0;
        //
        this.lblBuscarSalon.AutoSize = true;
        this.lblBuscarSalon.Location = new System.Drawing.Point(6, 13);
        this.lblBuscarSalon.Name = "lblBuscarSalon";
        this.lblBuscarSalon.Size = new System.Drawing.Size(43, 15);
        this.lblBuscarSalon.TabIndex = 0;
        this.lblBuscarSalon.Text = "Buscar";
        //
        this.txtFiltroSalon.Location = new System.Drawing.Point(55, 10);
        this.txtFiltroSalon.MaxLength = 100;
        this.txtFiltroSalon.Name = "txtFiltroSalon";
        this.txtFiltroSalon.PlaceholderText = "Nombre o ubicacion";
        this.txtFiltroSalon.Size = new System.Drawing.Size(260, 23);
        this.txtFiltroSalon.TabIndex = 1;
        this.txtFiltroSalon.KeyDown += new KeyEventHandler(this.txtFiltroSalon_KeyDown);
        //
        this.lblEstadoSalon.AutoSize = true;
        this.lblEstadoSalon.Location = new System.Drawing.Point(330, 13);
        this.lblEstadoSalon.Name = "lblEstadoSalon";
        this.lblEstadoSalon.Size = new System.Drawing.Size(42, 15);
        this.lblEstadoSalon.TabIndex = 2;
        this.lblEstadoSalon.Text = "Estado";
        //
        this.cboEstadoSalon.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboEstadoSalon.Location = new System.Drawing.Point(378, 10);
        this.cboEstadoSalon.Name = "cboEstadoSalon";
        this.cboEstadoSalon.Size = new System.Drawing.Size(130, 23);
        this.cboEstadoSalon.TabIndex = 3;
        //
        this.btnBuscarSalones.Location = new System.Drawing.Point(520, 9);
        this.btnBuscarSalones.Name = "btnBuscarSalones";
        this.btnBuscarSalones.Size = new System.Drawing.Size(90, 25);
        this.btnBuscarSalones.TabIndex = 4;
        this.btnBuscarSalones.Text = "Buscar";
        this.btnBuscarSalones.UseVisualStyleBackColor = true;
        this.btnBuscarSalones.Click += new System.EventHandler(this.btnBuscarSalones_Click);
        //
        this.btnNuevoSalon.Location = new System.Drawing.Point(616, 9);
        this.btnNuevoSalon.Name = "btnNuevoSalon";
        this.btnNuevoSalon.Size = new System.Drawing.Size(90, 25);
        this.btnNuevoSalon.TabIndex = 5;
        this.btnNuevoSalon.Text = "Nuevo";
        this.btnNuevoSalon.UseVisualStyleBackColor = true;
        this.btnNuevoSalon.Click += new System.EventHandler(this.btnNuevoSalon_Click);
        //
        this.dgvSalones.AllowUserToAddRows = false;
        this.dgvSalones.AllowUserToDeleteRows = false;
        this.dgvSalones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvSalones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvSalones.Dock = DockStyle.Fill;
        this.dgvSalones.Location = new System.Drawing.Point(3, 45);
        this.dgvSalones.MultiSelect = false;
        this.dgvSalones.Name = "dgvSalones";
        this.dgvSalones.ReadOnly = true;
        this.dgvSalones.RowHeadersVisible = false;
        this.dgvSalones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvSalones.Size = new System.Drawing.Size(886, 288);
        this.dgvSalones.TabIndex = 1;
        this.dgvSalones.SelectionChanged += new System.EventHandler(this.dgvSalones_SelectionChanged);
        //
        this.grpSalon.Controls.Add(this.btnEstadoSalon);
        this.grpSalon.Controls.Add(this.btnCancelarSalon);
        this.grpSalon.Controls.Add(this.btnGuardarSalon);
        this.grpSalon.Controls.Add(this.chkSalonActivo);
        this.grpSalon.Controls.Add(this.numTarifaSalon);
        this.grpSalon.Controls.Add(this.lblTarifaSalon);
        this.grpSalon.Controls.Add(this.numCapacidadSalon);
        this.grpSalon.Controls.Add(this.lblCapacidadSalon);
        this.grpSalon.Controls.Add(this.txtUbicacionSalon);
        this.grpSalon.Controls.Add(this.lblUbicacionSalon);
        this.grpSalon.Controls.Add(this.txtNombreSalon);
        this.grpSalon.Controls.Add(this.lblNombreSalon);
        this.grpSalon.Dock = DockStyle.Bottom;
        this.grpSalon.Location = new System.Drawing.Point(3, 333);
        this.grpSalon.Name = "grpSalon";
        this.grpSalon.Size = new System.Drawing.Size(886, 174);
        this.grpSalon.TabIndex = 2;
        this.grpSalon.TabStop = false;
        this.grpSalon.Text = "Datos del salon";
        //
        this.lblNombreSalon.AutoSize = true;
        this.lblNombreSalon.Location = new System.Drawing.Point(16, 30);
        this.lblNombreSalon.Name = "lblNombreSalon";
        this.lblNombreSalon.Size = new System.Drawing.Size(51, 15);
        this.lblNombreSalon.TabIndex = 0;
        this.lblNombreSalon.Text = "Nombre";
        //
        this.txtNombreSalon.Location = new System.Drawing.Point(120, 27);
        this.txtNombreSalon.MaxLength = 100;
        this.txtNombreSalon.Name = "txtNombreSalon";
        this.txtNombreSalon.Size = new System.Drawing.Size(300, 23);
        this.txtNombreSalon.TabIndex = 1;
        //
        this.lblUbicacionSalon.AutoSize = true;
        this.lblUbicacionSalon.Location = new System.Drawing.Point(16, 62);
        this.lblUbicacionSalon.Name = "lblUbicacionSalon";
        this.lblUbicacionSalon.Size = new System.Drawing.Size(59, 15);
        this.lblUbicacionSalon.TabIndex = 2;
        this.lblUbicacionSalon.Text = "Ubicacion";
        //
        this.txtUbicacionSalon.Location = new System.Drawing.Point(120, 59);
        this.txtUbicacionSalon.MaxLength = 150;
        this.txtUbicacionSalon.Name = "txtUbicacionSalon";
        this.txtUbicacionSalon.Size = new System.Drawing.Size(300, 23);
        this.txtUbicacionSalon.TabIndex = 3;
        //
        this.lblCapacidadSalon.AutoSize = true;
        this.lblCapacidadSalon.Location = new System.Drawing.Point(16, 94);
        this.lblCapacidadSalon.Name = "lblCapacidadSalon";
        this.lblCapacidadSalon.Size = new System.Drawing.Size(63, 15);
        this.lblCapacidadSalon.TabIndex = 4;
        this.lblCapacidadSalon.Text = "Capacidad";
        //
        this.numCapacidadSalon.Location = new System.Drawing.Point(120, 92);
        this.numCapacidadSalon.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numCapacidadSalon.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numCapacidadSalon.Name = "numCapacidadSalon";
        this.numCapacidadSalon.Size = new System.Drawing.Size(100, 23);
        this.numCapacidadSalon.TabIndex = 5;
        this.numCapacidadSalon.TextAlign = HorizontalAlignment.Right;
        this.numCapacidadSalon.Value = new decimal(new int[] { 50, 0, 0, 0 });
        //
        this.lblTarifaSalon.AutoSize = true;
        this.lblTarifaSalon.Location = new System.Drawing.Point(250, 94);
        this.lblTarifaSalon.Name = "lblTarifaSalon";
        this.lblTarifaSalon.Size = new System.Drawing.Size(63, 15);
        this.lblTarifaSalon.TabIndex = 6;
        this.lblTarifaSalon.Text = "Tarifa base";
        //
        this.numTarifaSalon.DecimalPlaces = 2;
        this.numTarifaSalon.Location = new System.Drawing.Point(330, 92);
        this.numTarifaSalon.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numTarifaSalon.Name = "numTarifaSalon";
        this.numTarifaSalon.Size = new System.Drawing.Size(120, 23);
        this.numTarifaSalon.TabIndex = 7;
        this.numTarifaSalon.TextAlign = HorizontalAlignment.Right;
        //
        this.chkSalonActivo.AutoSize = true;
        this.chkSalonActivo.Checked = true;
        this.chkSalonActivo.CheckState = CheckState.Checked;
        this.chkSalonActivo.Location = new System.Drawing.Point(120, 126);
        this.chkSalonActivo.Name = "chkSalonActivo";
        this.chkSalonActivo.Size = new System.Drawing.Size(60, 19);
        this.chkSalonActivo.TabIndex = 8;
        this.chkSalonActivo.Text = "Activo";
        this.chkSalonActivo.UseVisualStyleBackColor = true;
        //
        this.btnGuardarSalon.Location = new System.Drawing.Point(560, 124);
        this.btnGuardarSalon.Name = "btnGuardarSalon";
        this.btnGuardarSalon.Size = new System.Drawing.Size(100, 28);
        this.btnGuardarSalon.TabIndex = 9;
        this.btnGuardarSalon.Text = "Guardar";
        this.btnGuardarSalon.UseVisualStyleBackColor = true;
        this.btnGuardarSalon.Click += new System.EventHandler(this.btnGuardarSalon_Click);
        //
        this.btnCancelarSalon.Location = new System.Drawing.Point(666, 124);
        this.btnCancelarSalon.Name = "btnCancelarSalon";
        this.btnCancelarSalon.Size = new System.Drawing.Size(100, 28);
        this.btnCancelarSalon.TabIndex = 10;
        this.btnCancelarSalon.Text = "Cancelar";
        this.btnCancelarSalon.UseVisualStyleBackColor = true;
        this.btnCancelarSalon.Click += new System.EventHandler(this.btnCancelarSalon_Click);
        //
        this.btnEstadoSalon.Location = new System.Drawing.Point(772, 124);
        this.btnEstadoSalon.Name = "btnEstadoSalon";
        this.btnEstadoSalon.Size = new System.Drawing.Size(100, 28);
        this.btnEstadoSalon.TabIndex = 11;
        this.btnEstadoSalon.Text = "Inactivar";
        this.btnEstadoSalon.UseVisualStyleBackColor = true;
        this.btnEstadoSalon.Click += new System.EventHandler(this.btnEstadoSalon_Click);
        //
        // ===================================================================== RECURSOS
        //
        this.tabRecursos.Controls.Add(this.dgvRecursos);
        this.tabRecursos.Controls.Add(this.grpRecurso);
        this.tabRecursos.Controls.Add(this.pnlFiltroRecursos);
        this.tabRecursos.Location = new System.Drawing.Point(4, 24);
        this.tabRecursos.Name = "tabRecursos";
        this.tabRecursos.Padding = new Padding(3);
        this.tabRecursos.Size = new System.Drawing.Size(892, 510);
        this.tabRecursos.TabIndex = 2;
        this.tabRecursos.Text = "Recursos y servicios";
        this.tabRecursos.UseVisualStyleBackColor = true;
        //
        this.pnlFiltroRecursos.Controls.Add(this.btnNuevoRecurso);
        this.pnlFiltroRecursos.Controls.Add(this.btnBuscarRecursos);
        this.pnlFiltroRecursos.Controls.Add(this.cboEstadoRecurso);
        this.pnlFiltroRecursos.Controls.Add(this.lblEstadoRecurso);
        this.pnlFiltroRecursos.Controls.Add(this.cboTipoFiltroRecurso);
        this.pnlFiltroRecursos.Controls.Add(this.lblTipoFiltroRecurso);
        this.pnlFiltroRecursos.Controls.Add(this.txtFiltroRecurso);
        this.pnlFiltroRecursos.Controls.Add(this.lblBuscarRecurso);
        this.pnlFiltroRecursos.Dock = DockStyle.Top;
        this.pnlFiltroRecursos.Location = new System.Drawing.Point(3, 3);
        this.pnlFiltroRecursos.Name = "pnlFiltroRecursos";
        this.pnlFiltroRecursos.Size = new System.Drawing.Size(886, 42);
        this.pnlFiltroRecursos.TabIndex = 0;
        //
        this.lblBuscarRecurso.AutoSize = true;
        this.lblBuscarRecurso.Location = new System.Drawing.Point(6, 13);
        this.lblBuscarRecurso.Name = "lblBuscarRecurso";
        this.lblBuscarRecurso.Size = new System.Drawing.Size(43, 15);
        this.lblBuscarRecurso.TabIndex = 0;
        this.lblBuscarRecurso.Text = "Buscar";
        //
        this.txtFiltroRecurso.Location = new System.Drawing.Point(55, 10);
        this.txtFiltroRecurso.MaxLength = 100;
        this.txtFiltroRecurso.Name = "txtFiltroRecurso";
        this.txtFiltroRecurso.PlaceholderText = "Nombre del recurso";
        this.txtFiltroRecurso.Size = new System.Drawing.Size(200, 23);
        this.txtFiltroRecurso.TabIndex = 1;
        this.txtFiltroRecurso.KeyDown += new KeyEventHandler(this.txtFiltroRecurso_KeyDown);
        //
        this.lblTipoFiltroRecurso.AutoSize = true;
        this.lblTipoFiltroRecurso.Location = new System.Drawing.Point(270, 13);
        this.lblTipoFiltroRecurso.Name = "lblTipoFiltroRecurso";
        this.lblTipoFiltroRecurso.Size = new System.Drawing.Size(28, 15);
        this.lblTipoFiltroRecurso.TabIndex = 2;
        this.lblTipoFiltroRecurso.Text = "Tipo";
        //
        this.cboTipoFiltroRecurso.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboTipoFiltroRecurso.Location = new System.Drawing.Point(305, 10);
        this.cboTipoFiltroRecurso.Name = "cboTipoFiltroRecurso";
        this.cboTipoFiltroRecurso.Size = new System.Drawing.Size(130, 23);
        this.cboTipoFiltroRecurso.TabIndex = 3;
        //
        this.lblEstadoRecurso.AutoSize = true;
        this.lblEstadoRecurso.Location = new System.Drawing.Point(450, 13);
        this.lblEstadoRecurso.Name = "lblEstadoRecurso";
        this.lblEstadoRecurso.Size = new System.Drawing.Size(42, 15);
        this.lblEstadoRecurso.TabIndex = 4;
        this.lblEstadoRecurso.Text = "Estado";
        //
        this.cboEstadoRecurso.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboEstadoRecurso.Location = new System.Drawing.Point(498, 10);
        this.cboEstadoRecurso.Name = "cboEstadoRecurso";
        this.cboEstadoRecurso.Size = new System.Drawing.Size(120, 23);
        this.cboEstadoRecurso.TabIndex = 5;
        //
        this.btnBuscarRecursos.Location = new System.Drawing.Point(630, 9);
        this.btnBuscarRecursos.Name = "btnBuscarRecursos";
        this.btnBuscarRecursos.Size = new System.Drawing.Size(90, 25);
        this.btnBuscarRecursos.TabIndex = 6;
        this.btnBuscarRecursos.Text = "Buscar";
        this.btnBuscarRecursos.UseVisualStyleBackColor = true;
        this.btnBuscarRecursos.Click += new System.EventHandler(this.btnBuscarRecursos_Click);
        //
        this.btnNuevoRecurso.Location = new System.Drawing.Point(726, 9);
        this.btnNuevoRecurso.Name = "btnNuevoRecurso";
        this.btnNuevoRecurso.Size = new System.Drawing.Size(90, 25);
        this.btnNuevoRecurso.TabIndex = 7;
        this.btnNuevoRecurso.Text = "Nuevo";
        this.btnNuevoRecurso.UseVisualStyleBackColor = true;
        this.btnNuevoRecurso.Click += new System.EventHandler(this.btnNuevoRecurso_Click);
        //
        this.dgvRecursos.AllowUserToAddRows = false;
        this.dgvRecursos.AllowUserToDeleteRows = false;
        this.dgvRecursos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvRecursos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvRecursos.Dock = DockStyle.Fill;
        this.dgvRecursos.Location = new System.Drawing.Point(3, 45);
        this.dgvRecursos.MultiSelect = false;
        this.dgvRecursos.Name = "dgvRecursos";
        this.dgvRecursos.ReadOnly = true;
        this.dgvRecursos.RowHeadersVisible = false;
        this.dgvRecursos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvRecursos.Size = new System.Drawing.Size(886, 288);
        this.dgvRecursos.TabIndex = 1;
        this.dgvRecursos.SelectionChanged += new System.EventHandler(this.dgvRecursos_SelectionChanged);
        //
        this.grpRecurso.Controls.Add(this.btnEstadoRecurso);
        this.grpRecurso.Controls.Add(this.btnCancelarRecurso);
        this.grpRecurso.Controls.Add(this.btnGuardarRecurso);
        this.grpRecurso.Controls.Add(this.chkRecursoActivo);
        this.grpRecurso.Controls.Add(this.numPrecioRecurso);
        this.grpRecurso.Controls.Add(this.lblPrecioRecurso);
        this.grpRecurso.Controls.Add(this.numStockRecurso);
        this.grpRecurso.Controls.Add(this.lblStockRecurso);
        this.grpRecurso.Controls.Add(this.cboTipoRecurso);
        this.grpRecurso.Controls.Add(this.lblTipoRecurso);
        this.grpRecurso.Controls.Add(this.txtNombreRecurso);
        this.grpRecurso.Controls.Add(this.lblNombreRecurso);
        this.grpRecurso.Dock = DockStyle.Bottom;
        this.grpRecurso.Location = new System.Drawing.Point(3, 333);
        this.grpRecurso.Name = "grpRecurso";
        this.grpRecurso.Size = new System.Drawing.Size(886, 174);
        this.grpRecurso.TabIndex = 2;
        this.grpRecurso.TabStop = false;
        this.grpRecurso.Text = "Datos del recurso o servicio";
        //
        this.lblNombreRecurso.AutoSize = true;
        this.lblNombreRecurso.Location = new System.Drawing.Point(16, 30);
        this.lblNombreRecurso.Name = "lblNombreRecurso";
        this.lblNombreRecurso.Size = new System.Drawing.Size(51, 15);
        this.lblNombreRecurso.TabIndex = 0;
        this.lblNombreRecurso.Text = "Nombre";
        //
        this.txtNombreRecurso.Location = new System.Drawing.Point(120, 27);
        this.txtNombreRecurso.MaxLength = 100;
        this.txtNombreRecurso.Name = "txtNombreRecurso";
        this.txtNombreRecurso.Size = new System.Drawing.Size(300, 23);
        this.txtNombreRecurso.TabIndex = 1;
        //
        this.lblTipoRecurso.AutoSize = true;
        this.lblTipoRecurso.Location = new System.Drawing.Point(16, 62);
        this.lblTipoRecurso.Name = "lblTipoRecurso";
        this.lblTipoRecurso.Size = new System.Drawing.Size(28, 15);
        this.lblTipoRecurso.TabIndex = 2;
        this.lblTipoRecurso.Text = "Tipo";
        //
        this.cboTipoRecurso.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboTipoRecurso.Location = new System.Drawing.Point(120, 59);
        this.cboTipoRecurso.Name = "cboTipoRecurso";
        this.cboTipoRecurso.Size = new System.Drawing.Size(180, 23);
        this.cboTipoRecurso.TabIndex = 3;
        //
        this.lblStockRecurso.AutoSize = true;
        this.lblStockRecurso.Location = new System.Drawing.Point(16, 94);
        this.lblStockRecurso.Name = "lblStockRecurso";
        this.lblStockRecurso.Size = new System.Drawing.Size(66, 15);
        this.lblStockRecurso.TabIndex = 4;
        this.lblStockRecurso.Text = "Stock total";
        //
        this.numStockRecurso.Location = new System.Drawing.Point(120, 92);
        this.numStockRecurso.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numStockRecurso.Name = "numStockRecurso";
        this.numStockRecurso.Size = new System.Drawing.Size(100, 23);
        this.numStockRecurso.TabIndex = 5;
        this.numStockRecurso.TextAlign = HorizontalAlignment.Right;
        //
        this.lblPrecioRecurso.AutoSize = true;
        this.lblPrecioRecurso.Location = new System.Drawing.Point(250, 94);
        this.lblPrecioRecurso.Name = "lblPrecioRecurso";
        this.lblPrecioRecurso.Size = new System.Drawing.Size(84, 15);
        this.lblPrecioRecurso.TabIndex = 6;
        this.lblPrecioRecurso.Text = "Precio unitario";
        //
        this.numPrecioRecurso.DecimalPlaces = 2;
        this.numPrecioRecurso.Location = new System.Drawing.Point(348, 92);
        this.numPrecioRecurso.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numPrecioRecurso.Name = "numPrecioRecurso";
        this.numPrecioRecurso.Size = new System.Drawing.Size(120, 23);
        this.numPrecioRecurso.TabIndex = 7;
        this.numPrecioRecurso.TextAlign = HorizontalAlignment.Right;
        //
        this.chkRecursoActivo.AutoSize = true;
        this.chkRecursoActivo.Checked = true;
        this.chkRecursoActivo.CheckState = CheckState.Checked;
        this.chkRecursoActivo.Location = new System.Drawing.Point(120, 126);
        this.chkRecursoActivo.Name = "chkRecursoActivo";
        this.chkRecursoActivo.Size = new System.Drawing.Size(60, 19);
        this.chkRecursoActivo.TabIndex = 8;
        this.chkRecursoActivo.Text = "Activo";
        this.chkRecursoActivo.UseVisualStyleBackColor = true;
        //
        this.btnGuardarRecurso.Location = new System.Drawing.Point(560, 124);
        this.btnGuardarRecurso.Name = "btnGuardarRecurso";
        this.btnGuardarRecurso.Size = new System.Drawing.Size(100, 28);
        this.btnGuardarRecurso.TabIndex = 9;
        this.btnGuardarRecurso.Text = "Guardar";
        this.btnGuardarRecurso.UseVisualStyleBackColor = true;
        this.btnGuardarRecurso.Click += new System.EventHandler(this.btnGuardarRecurso_Click);
        //
        this.btnCancelarRecurso.Location = new System.Drawing.Point(666, 124);
        this.btnCancelarRecurso.Name = "btnCancelarRecurso";
        this.btnCancelarRecurso.Size = new System.Drawing.Size(100, 28);
        this.btnCancelarRecurso.TabIndex = 10;
        this.btnCancelarRecurso.Text = "Cancelar";
        this.btnCancelarRecurso.UseVisualStyleBackColor = true;
        this.btnCancelarRecurso.Click += new System.EventHandler(this.btnCancelarRecurso_Click);
        //
        this.btnEstadoRecurso.Location = new System.Drawing.Point(772, 124);
        this.btnEstadoRecurso.Name = "btnEstadoRecurso";
        this.btnEstadoRecurso.Size = new System.Drawing.Size(100, 28);
        this.btnEstadoRecurso.TabIndex = 11;
        this.btnEstadoRecurso.Text = "Inactivar";
        this.btnEstadoRecurso.UseVisualStyleBackColor = true;
        this.btnEstadoRecurso.Click += new System.EventHandler(this.btnEstadoRecurso_Click);
        //
        // stbCatalogos
        //
        this.stbCatalogos.Items.AddRange(new ToolStripItem[] { this.lblEstadoOperacion });
        this.stbCatalogos.Location = new System.Drawing.Point(0, 538);
        this.stbCatalogos.Name = "stbCatalogos";
        this.stbCatalogos.Size = new System.Drawing.Size(900, 22);
        this.stbCatalogos.TabIndex = 1;
        //
        this.lblEstadoOperacion.Name = "lblEstadoOperacion";
        this.lblEstadoOperacion.Size = new System.Drawing.Size(0, 17);
        //
        // FrmCatalogos
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(900, 560);
        this.Controls.Add(this.tabCatalogos);
        this.Controls.Add(this.stbCatalogos);
        this.MinimumSize = new System.Drawing.Size(880, 560);
        this.Name = "FrmCatalogos";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Catalogos";
        this.Load += new System.EventHandler(this.FrmCatalogos_Load);
        this.FormClosed += new FormClosedEventHandler(this.FrmCatalogos_FormClosed);
        this.tabCatalogos.ResumeLayout(false);
        this.tabClientes.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvClientes)).EndInit();
        this.grpCliente.ResumeLayout(false);
        this.grpCliente.PerformLayout();
        this.pnlFiltroClientes.ResumeLayout(false);
        this.pnlFiltroClientes.PerformLayout();
        this.tabSalones.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvSalones)).EndInit();
        this.grpSalon.ResumeLayout(false);
        this.grpSalon.PerformLayout();
        this.pnlFiltroSalones.ResumeLayout(false);
        this.pnlFiltroSalones.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numCapacidadSalon)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numTarifaSalon)).EndInit();
        this.tabRecursos.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvRecursos)).EndInit();
        this.grpRecurso.ResumeLayout(false);
        this.grpRecurso.PerformLayout();
        this.pnlFiltroRecursos.ResumeLayout(false);
        this.pnlFiltroRecursos.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numStockRecurso)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPrecioRecurso)).EndInit();
        this.stbCatalogos.ResumeLayout(false);
        this.stbCatalogos.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
