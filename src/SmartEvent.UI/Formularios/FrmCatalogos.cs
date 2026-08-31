using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Mantenimiento de los tres catalogos en un solo formulario con pestanas.
///
/// El patron es identico en las tres: filtros arriba, grilla en el centro y ficha de edicion
/// abajo. Seleccionar una fila carga la ficha; Guardar decide entre alta y modificacion segun
/// haya o no un identificador cargado.
///
/// PERMISOS: si el usuario no puede editar catalogos, la ficha completa se deshabilita al
/// abrir el formulario. Es solo comodidad visual; quien impide realmente la modificacion es
/// ServicioCatalogos, que vuelve a comprobar el rol en cada llamada.
///
/// La INACTIVACION es logica: nunca se borra una fila, porque las reservas historicas siguen
/// apuntando a ella.
/// </summary>
public partial class FrmCatalogos : Form
{
    /// <summary>Pestanas disponibles; el menu principal indica cual mostrar al abrir.</summary>
    public enum Pestana
    {
        Clientes = 0,
        Salones = 1,
        Recursos = 2
    }

    private readonly ContenedorServicios _servicios;
    private readonly CancellationTokenSource _cancelacion = new();

    private int _idClienteSeleccionado;
    private int _idSalonSeleccionado;
    private int _idRecursoSeleccionado;

    /// <summary>
    /// Evita que el evento SelectionChanged de las grillas dispare recargas mientras se esta
    /// repoblando el origen de datos.
    /// </summary>
    private bool _cargando;

    /// <summary>
    /// Pestanas ya consultadas. Cada catalogo se carga la PRIMERA vez que se muestra, no todos
    /// al abrir el formulario: son tres consultas independientes y normalmente solo se usa una.
    /// </summary>
    private readonly HashSet<Pestana> _pestanasCargadas = new();

    public FrmCatalogos(ContenedorServicios servicios)
    {
        _servicios = servicios ?? throw new ArgumentNullException(nameof(servicios));

        InitializeComponent();
    }

    public void MostrarPestana(Pestana pestana) => tabCatalogos.SelectedIndex = (int)pestana;

    private async void FrmCatalogos_Load(object sender, EventArgs e)
    {
        PrepararCombos();
        AplicarPermisos();

        await CargarPestanaActualAsync();
    }

    private async void tabCatalogos_SelectedIndexChanged(object sender, EventArgs e) =>
        await CargarPestanaActualAsync();

    /// <summary>Consulta el catalogo de la pestana visible si aun no se ha cargado.</summary>
    private async Task CargarPestanaActualAsync()
    {
        var pestana = (Pestana)tabCatalogos.SelectedIndex;

        if (!_pestanasCargadas.Add(pestana))
        {
            return;
        }

        switch (pestana)
        {
            case Pestana.Clientes:
                await BuscarClientesAsync();
                break;
            case Pestana.Salones:
                await BuscarSalonesAsync();
                break;
            case Pestana.Recursos:
                await BuscarRecursosAsync();
                break;
        }
    }

    private void PrepararCombos()
    {
        foreach (var combo in new[] { cboEstadoCliente, cboEstadoSalon, cboEstadoRecurso })
        {
            combo.DisplayMember = nameof(OpcionEstado.Texto);
            combo.ValueMember = nameof(OpcionEstado.Valor);
            combo.DataSource = new List<OpcionEstado>
            {
                new("Activos", true),
                new("Inactivos", false),
                new("Todos", null)
            };
            combo.SelectedIndex = 0;
        }

        // Tipos de recurso: se construyen desde el enum para que anadir uno nuevo no obligue a
        // tocar la interfaz.
        var tipos = Enum.GetValues<TipoRecurso>()
                        .Select(t => new OpcionTipo(t.ToString().ToUpperInvariant(), t))
                        .ToList();

        cboTipoRecurso.DisplayMember = nameof(OpcionTipo.Texto);
        cboTipoRecurso.ValueMember = nameof(OpcionTipo.Valor);
        cboTipoRecurso.DataSource = new List<OpcionTipo>(tipos);

        var tiposConTodos = new List<OpcionTipo> { new("Todos", null) };
        tiposConTodos.AddRange(tipos);

        cboTipoFiltroRecurso.DisplayMember = nameof(OpcionTipo.Texto);
        cboTipoFiltroRecurso.ValueMember = nameof(OpcionTipo.Valor);
        cboTipoFiltroRecurso.DataSource = tiposConTodos;
        cboTipoFiltroRecurso.SelectedIndex = 0;
    }

    private void AplicarPermisos()
    {
        var puedeEditar = _servicios.Contexto.Requerida.PuedeEditarCatalogos;

        grpCliente.Enabled = puedeEditar;
        grpSalon.Enabled = puedeEditar;
        grpRecurso.Enabled = puedeEditar;

        btnNuevoCliente.Enabled = puedeEditar;
        btnNuevoSalon.Enabled = puedeEditar;
        btnNuevoRecurso.Enabled = puedeEditar;

        if (!puedeEditar)
        {
            lblEstadoOperacion.Text =
                "Su rol permite consultar los catalogos, pero no modificarlos.";
        }
    }

    // ================================================================================ CLIENTES

    private async void btnBuscarClientes_Click(object sender, EventArgs e) => await BuscarClientesAsync();

    private async void txtFiltroCliente_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        // Evita el pitido del sistema al pulsar Enter en un cuadro de texto.
        e.SuppressKeyPress = true;
        await BuscarClientesAsync();
    }

    private async Task BuscarClientesAsync()
    {
        var filtro = txtFiltroCliente.Text.Trim();
        var estado = (cboEstadoCliente.SelectedItem as OpcionEstado)?.Valor;

        var clientes = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.ConsultarClientesAsync(filtro, estado, ct),
            _cancelacion.Token,
            "consulta de clientes",
            btnBuscarClientes);

        if (clientes is null)
        {
            return;
        }

        _cargando = true;

        dgvClientes.DataSource = clientes.Select(c => new
        {
            c.IdCliente,
            c.Identificacion,
            Nombres = c.Nombres,
            c.Email,
            c.Telefono,
            Estado = c.Estado ? "Activo" : "Inactivo"
        }).ToList();

        ConfigurarGrillaClientes();
        _cargando = false;

        LimpiarFichaCliente();
        lblEstadoOperacion.Text = $"Clientes encontrados: {clientes.Count}.";
    }

    private void ConfigurarGrillaClientes()
    {
        if (dgvClientes.Columns.Count == 0)
        {
            return;
        }

        dgvClientes.Columns["IdCliente"]!.Visible = false;
        dgvClientes.Columns["Identificacion"]!.HeaderText = "Identificacion";
        dgvClientes.Columns["Nombres"]!.HeaderText = "Nombre / razon social";
        dgvClientes.Columns["Email"]!.HeaderText = "Correo electronico";
        dgvClientes.Columns["Telefono"]!.HeaderText = "Telefono";
        dgvClientes.Columns["Estado"]!.HeaderText = "Estado";
        dgvClientes.Columns["Identificacion"]!.FillWeight = 70;
        dgvClientes.Columns["Nombres"]!.FillWeight = 130;
        dgvClientes.Columns["Estado"]!.FillWeight = 50;
    }

    private void dgvClientes_SelectionChanged(object sender, EventArgs e)
    {
        if (_cargando || dgvClientes.CurrentRow is null)
        {
            return;
        }

        var fila = dgvClientes.CurrentRow;

        _idClienteSeleccionado = Convert.ToInt32(fila.Cells["IdCliente"].Value);
        txtIdentificacion.Text = Convert.ToString(fila.Cells["Identificacion"].Value) ?? string.Empty;
        txtNombresCliente.Text = Convert.ToString(fila.Cells["Nombres"].Value) ?? string.Empty;
        txtEmailCliente.Text = Convert.ToString(fila.Cells["Email"].Value) ?? string.Empty;
        txtTelefonoCliente.Text = Convert.ToString(fila.Cells["Telefono"].Value) ?? string.Empty;

        var activo = Convert.ToString(fila.Cells["Estado"].Value) == "Activo";
        chkClienteActivo.Checked = activo;
        btnEstadoCliente.Text = activo ? "Inactivar" : "Activar";
        btnEstadoCliente.Enabled = _servicios.Contexto.Requerida.PuedeEditarCatalogos;
    }

    private void btnNuevoCliente_Click(object sender, EventArgs e)
    {
        LimpiarFichaCliente();
        txtIdentificacion.Focus();
    }

    private void LimpiarFichaCliente()
    {
        _idClienteSeleccionado = 0;
        txtIdentificacion.Clear();
        txtNombresCliente.Clear();
        txtEmailCliente.Clear();
        txtTelefonoCliente.Clear();
        chkClienteActivo.Checked = true;
        btnEstadoCliente.Enabled = false;
        btnEstadoCliente.Text = "Inactivar";
    }

    private async void btnGuardarCliente_Click(object sender, EventArgs e)
    {
        var cliente = new Cliente
        {
            IdCliente = _idClienteSeleccionado,
            Identificacion = txtIdentificacion.Text,
            Nombres = txtNombresCliente.Text,
            Email = txtEmailCliente.Text,
            Telefono = txtTelefonoCliente.Text,
            Estado = chkClienteActivo.Checked
        };

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.GuardarClienteAsync(cliente, ct),
            _cancelacion.Token,
            "guardado de cliente",
            btnGuardarCliente, btnCancelarCliente);

        if (resultado is null)
        {
            return;
        }

        lblEstadoOperacion.Text = resultado.Mensaje;
        await BuscarClientesAsync();
    }

    private void btnCancelarCliente_Click(object sender, EventArgs e) => LimpiarFichaCliente();

    private async void btnEstadoCliente_Click(object sender, EventArgs e)
    {
        if (_idClienteSeleccionado == 0)
        {
            ManejadorErrores.Advertir(this, "Seleccione un cliente de la lista.");
            return;
        }

        var activar = btnEstadoCliente.Text == "Activar";

        if (!ManejadorErrores.Confirmar(this,
                activar
                    ? "Desea activar este cliente?"
                    : "Desea inactivar este cliente?\n\nNo se elimina informacion: la baja es logica y " +
                      "las reservas historicas se conservan."))
        {
            return;
        }

        var mensaje = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.CambiarEstadoClienteAsync(_idClienteSeleccionado, activar, ct),
            _cancelacion.Token,
            "cambio de estado de cliente",
            btnEstadoCliente);

        if (mensaje is null)
        {
            return;
        }

        lblEstadoOperacion.Text = mensaje;
        await BuscarClientesAsync();
    }

    // ================================================================================= SALONES

    private async void btnBuscarSalones_Click(object sender, EventArgs e) => await BuscarSalonesAsync();

    private async void txtFiltroSalon_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        await BuscarSalonesAsync();
    }

    private async Task BuscarSalonesAsync()
    {
        var filtro = txtFiltroSalon.Text.Trim();
        var estado = (cboEstadoSalon.SelectedItem as OpcionEstado)?.Valor;

        var salones = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.ConsultarSalonesAsync(filtro, estado, null, ct),
            _cancelacion.Token,
            "consulta de salones",
            btnBuscarSalones);

        if (salones is null)
        {
            return;
        }

        _cargando = true;

        dgvSalones.DataSource = salones.Select(s => new
        {
            s.IdSalon,
            s.Nombre,
            s.Ubicacion,
            s.Capacidad,
            TarifaBase = s.TarifaBase.ToString("N2"),
            Estado = s.Estado ? "Activo" : "Inactivo"
        }).ToList();

        if (dgvSalones.Columns.Count > 0)
        {
            dgvSalones.Columns["IdSalon"]!.Visible = false;
            dgvSalones.Columns["TarifaBase"]!.HeaderText = "Tarifa base";
            dgvSalones.Columns["TarifaBase"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvSalones.Columns["Capacidad"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        _cargando = false;

        LimpiarFichaSalon();
        lblEstadoOperacion.Text = $"Salones encontrados: {salones.Count}.";
    }

    private void dgvSalones_SelectionChanged(object sender, EventArgs e)
    {
        if (_cargando || dgvSalones.CurrentRow is null)
        {
            return;
        }

        var fila = dgvSalones.CurrentRow;

        _idSalonSeleccionado = Convert.ToInt32(fila.Cells["IdSalon"].Value);
        txtNombreSalon.Text = Convert.ToString(fila.Cells["Nombre"].Value) ?? string.Empty;
        txtUbicacionSalon.Text = Convert.ToString(fila.Cells["Ubicacion"].Value) ?? string.Empty;
        numCapacidadSalon.Value = Convert.ToInt32(fila.Cells["Capacidad"].Value);
        numTarifaSalon.Value = decimal.TryParse(Convert.ToString(fila.Cells["TarifaBase"].Value), out var tarifa)
            ? tarifa
            : 0m;

        var activo = Convert.ToString(fila.Cells["Estado"].Value) == "Activo";
        chkSalonActivo.Checked = activo;
        btnEstadoSalon.Text = activo ? "Inactivar" : "Activar";
        btnEstadoSalon.Enabled = _servicios.Contexto.Requerida.PuedeEditarCatalogos;
    }

    private void btnNuevoSalon_Click(object sender, EventArgs e)
    {
        LimpiarFichaSalon();
        txtNombreSalon.Focus();
    }

    private void LimpiarFichaSalon()
    {
        _idSalonSeleccionado = 0;
        txtNombreSalon.Clear();
        txtUbicacionSalon.Clear();
        numCapacidadSalon.Value = 50;
        numTarifaSalon.Value = 0m;
        chkSalonActivo.Checked = true;
        btnEstadoSalon.Enabled = false;
        btnEstadoSalon.Text = "Inactivar";
    }

    private async void btnGuardarSalon_Click(object sender, EventArgs e)
    {
        var salon = new Salon
        {
            IdSalon = _idSalonSeleccionado,
            Nombre = txtNombreSalon.Text,
            Ubicacion = txtUbicacionSalon.Text,
            Capacidad = (int)numCapacidadSalon.Value,
            TarifaBase = numTarifaSalon.Value,
            Estado = chkSalonActivo.Checked
        };

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.GuardarSalonAsync(salon, ct),
            _cancelacion.Token,
            "guardado de salon",
            btnGuardarSalon, btnCancelarSalon);

        if (resultado is null)
        {
            return;
        }

        lblEstadoOperacion.Text = resultado.Mensaje;
        await BuscarSalonesAsync();
    }

    private void btnCancelarSalon_Click(object sender, EventArgs e) => LimpiarFichaSalon();

    private async void btnEstadoSalon_Click(object sender, EventArgs e)
    {
        if (_idSalonSeleccionado == 0)
        {
            ManejadorErrores.Advertir(this, "Seleccione un salon de la lista.");
            return;
        }

        var activar = btnEstadoSalon.Text == "Activar";

        if (!ManejadorErrores.Confirmar(this,
                activar ? "Desea activar este salon?" : "Desea inactivar este salon?"))
        {
            return;
        }

        var mensaje = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.CambiarEstadoSalonAsync(_idSalonSeleccionado, activar, ct),
            _cancelacion.Token,
            "cambio de estado de salon",
            btnEstadoSalon);

        if (mensaje is null)
        {
            return;
        }

        lblEstadoOperacion.Text = mensaje;
        await BuscarSalonesAsync();
    }

    // ================================================================================ RECURSOS

    private async void btnBuscarRecursos_Click(object sender, EventArgs e) => await BuscarRecursosAsync();

    private async void txtFiltroRecurso_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        await BuscarRecursosAsync();
    }

    private async Task BuscarRecursosAsync()
    {
        var filtro = txtFiltroRecurso.Text.Trim();
        var tipo = (cboTipoFiltroRecurso.SelectedItem as OpcionTipo)?.Valor;
        var estado = (cboEstadoRecurso.SelectedItem as OpcionEstado)?.Valor;

        var recursos = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.ConsultarRecursosAsync(filtro, tipo, estado, ct),
            _cancelacion.Token,
            "consulta de recursos",
            btnBuscarRecursos);

        if (recursos is null)
        {
            return;
        }

        _cargando = true;

        dgvRecursos.DataSource = recursos.Select(r => new
        {
            r.IdRecurso,
            r.Nombre,
            Tipo = r.Tipo.ToString(),
            r.StockTotal,
            PrecioUnitario = r.PrecioUnitario.ToString("N2"),
            Estado = r.Estado ? "Activo" : "Inactivo"
        }).ToList();

        if (dgvRecursos.Columns.Count > 0)
        {
            dgvRecursos.Columns["IdRecurso"]!.Visible = false;
            dgvRecursos.Columns["StockTotal"]!.HeaderText = "Stock total";
            dgvRecursos.Columns["PrecioUnitario"]!.HeaderText = "Precio unitario";
            dgvRecursos.Columns["StockTotal"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvRecursos.Columns["PrecioUnitario"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        _cargando = false;

        LimpiarFichaRecurso();
        lblEstadoOperacion.Text = $"Recursos encontrados: {recursos.Count}.";
    }

    private void dgvRecursos_SelectionChanged(object sender, EventArgs e)
    {
        if (_cargando || dgvRecursos.CurrentRow is null)
        {
            return;
        }

        var fila = dgvRecursos.CurrentRow;

        _idRecursoSeleccionado = Convert.ToInt32(fila.Cells["IdRecurso"].Value);
        txtNombreRecurso.Text = Convert.ToString(fila.Cells["Nombre"].Value) ?? string.Empty;

        var tipoTexto = Convert.ToString(fila.Cells["Tipo"].Value);

        if (Enum.TryParse<TipoRecurso>(tipoTexto, out var tipo))
        {
            cboTipoRecurso.SelectedItem = (cboTipoRecurso.DataSource as List<OpcionTipo>)?
                .FirstOrDefault(o => o.Valor == tipo);
        }

        numStockRecurso.Value = Convert.ToInt32(fila.Cells["StockTotal"].Value);
        numPrecioRecurso.Value = decimal.TryParse(Convert.ToString(fila.Cells["PrecioUnitario"].Value), out var precio)
            ? precio
            : 0m;

        var activo = Convert.ToString(fila.Cells["Estado"].Value) == "Activo";
        chkRecursoActivo.Checked = activo;
        btnEstadoRecurso.Text = activo ? "Inactivar" : "Activar";
        btnEstadoRecurso.Enabled = _servicios.Contexto.Requerida.PuedeEditarCatalogos;
    }

    private void btnNuevoRecurso_Click(object sender, EventArgs e)
    {
        LimpiarFichaRecurso();
        txtNombreRecurso.Focus();
    }

    private void LimpiarFichaRecurso()
    {
        _idRecursoSeleccionado = 0;
        txtNombreRecurso.Clear();

        if (cboTipoRecurso.Items.Count > 0)
        {
            cboTipoRecurso.SelectedIndex = 0;
        }

        numStockRecurso.Value = 0;
        numPrecioRecurso.Value = 0m;
        chkRecursoActivo.Checked = true;
        btnEstadoRecurso.Enabled = false;
        btnEstadoRecurso.Text = "Inactivar";
    }

    private async void btnGuardarRecurso_Click(object sender, EventArgs e)
    {
        var tipo = (cboTipoRecurso.SelectedItem as OpcionTipo)?.Valor;

        if (tipo is null)
        {
            ManejadorErrores.Advertir(this, "Seleccione el tipo del recurso.");
            cboTipoRecurso.Focus();
            return;
        }

        var recurso = new Recurso
        {
            IdRecurso = _idRecursoSeleccionado,
            Nombre = txtNombreRecurso.Text,
            Tipo = tipo.Value,
            StockTotal = (int)numStockRecurso.Value,
            PrecioUnitario = numPrecioRecurso.Value,
            Estado = chkRecursoActivo.Checked
        };

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.GuardarRecursoAsync(recurso, ct),
            _cancelacion.Token,
            "guardado de recurso",
            btnGuardarRecurso, btnCancelarRecurso);

        if (resultado is null)
        {
            return;
        }

        lblEstadoOperacion.Text = resultado.Mensaje;
        await BuscarRecursosAsync();
    }

    private void btnCancelarRecurso_Click(object sender, EventArgs e) => LimpiarFichaRecurso();

    private async void btnEstadoRecurso_Click(object sender, EventArgs e)
    {
        if (_idRecursoSeleccionado == 0)
        {
            ManejadorErrores.Advertir(this, "Seleccione un recurso de la lista.");
            return;
        }

        var activar = btnEstadoRecurso.Text == "Activar";

        if (!ManejadorErrores.Confirmar(this,
                activar ? "Desea activar este recurso?" : "Desea inactivar este recurso?"))
        {
            return;
        }

        var mensaje = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.CambiarEstadoRecursoAsync(_idRecursoSeleccionado, activar, ct),
            _cancelacion.Token,
            "cambio de estado de recurso",
            btnEstadoRecurso);

        if (mensaje is null)
        {
            return;
        }

        lblEstadoOperacion.Text = mensaje;
        await BuscarRecursosAsync();
    }

    private void FrmCatalogos_FormClosed(object sender, FormClosedEventArgs e)
    {
        _cancelacion.Cancel();
        _cancelacion.Dispose();
    }

    // --------------------------------------------------------------------------------- apoyo

    /// <summary>Elemento de los combos de estado. Valor nulo significa "sin filtrar".</summary>
    private sealed record OpcionEstado(string Texto, bool? Valor);

    /// <summary>Elemento de los combos de tipo de recurso. Valor nulo significa "todos".</summary>
    private sealed record OpcionTipo(string Texto, TipoRecurso? Valor);
}
