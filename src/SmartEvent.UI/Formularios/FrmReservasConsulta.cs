using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Consulta historica de reservas con filtros combinables y carga asincronica.
///
/// DOS COMPORTAMIENTOS QUE CONVIENE DESTACAR:
///
/// 1. CANCELACION REAL. Cada busqueda cancela la anterior mediante OperacionCancelable. Sin
///    eso, dos busquedas lanzadas seguidas podrian terminar en orden inverso y pintar en la
///    grilla resultados que ya no corresponden a los filtros escritos. El boton Cancelar
///    busqueda permite ademas abortar una consulta lenta sin cerrar la pantalla.
///
/// 2. PAGINACION EN EL SERVIDOR. No se traen todas las reservas para filtrarlas en memoria:
///    el procedimiento devuelve solo la pagina pedida y el total de coincidencias.
/// </summary>
public partial class FrmReservasConsulta : Form
{
    private readonly ContenedorServicios _servicios;
    private readonly CancellationTokenSource _cancelacion = new();
    private readonly OperacionCancelable _busqueda;

    private int _paginaActual = 1;
    private int _totalPaginas = 1;
    private bool _cargando;

    public FrmReservasConsulta(ContenedorServicios servicios)
    {
        _servicios = servicios ?? throw new ArgumentNullException(nameof(servicios));

        InitializeComponent();

        _busqueda = new OperacionCancelable(_cancelacion.Token);
    }

    private async void FrmReservasConsulta_Load(object sender, EventArgs e)
    {
        _cargando = true;

        dtpDesde.Value = DateTime.Today.AddMonths(-1);
        dtpHasta.Value = DateTime.Today.AddMonths(3);

        cboTamanoPagina.Items.AddRange(new object[] { 25, 50, 100, 200 });
        cboTamanoPagina.SelectedIndex = 1;

        // Estados: se construyen desde el enum, con una opcion inicial para no filtrar.
        var estados = new List<OpcionEstado> { new("Todos", null) };
        estados.AddRange(Enum.GetValues<EstadoReserva>().Select(x => new OpcionEstado(x.ATextoUsuario(), x)));

        cboEstado.DisplayMember = nameof(OpcionEstado.Texto);
        cboEstado.ValueMember = nameof(OpcionEstado.Valor);
        cboEstado.DataSource = estados;
        cboEstado.SelectedIndex = 0;

        await CargarSalonesAsync();

        _cargando = false;

        await BuscarAsync(reiniciarPagina: true);
    }

    private async Task CargarSalonesAsync()
    {
        var salones = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Catalogos.ConsultarSalonesAsync(null, null, null, ct),
            _cancelacion.Token,
            "carga de salones");

        var opciones = new List<OpcionSalon> { new("Todos", null) };

        if (salones is not null)
        {
            opciones.AddRange(salones.Select(s => new OpcionSalon(s.Nombre, s.IdSalon)));
        }

        cboSalon.DisplayMember = nameof(OpcionSalon.Texto);
        cboSalon.ValueMember = nameof(OpcionSalon.Valor);
        cboSalon.DataSource = opciones;
        cboSalon.SelectedIndex = 0;
    }

    // =============================================================================== busqueda

    private async void btnBuscar_Click(object sender, EventArgs e) => await BuscarAsync(reiniciarPagina: true);

    private async void Filtro_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        await BuscarAsync(reiniciarPagina: true);
    }

    private async Task BuscarAsync(bool reiniciarPagina)
    {
        if (_cargando)
        {
            return;
        }

        if (reiniciarPagina)
        {
            _paginaActual = 1;
        }

        var filtro = new ReservaFiltroDto
        {
            Codigo = txtCodigo.Text.Trim(),
            TextoCliente = txtCliente.Text.Trim(),
            FechaDesde = chkFechas.Checked ? DateOnly.FromDateTime(dtpDesde.Value) : null,
            FechaHasta = chkFechas.Checked ? DateOnly.FromDateTime(dtpHasta.Value) : null,
            IdSalon = (cboSalon.SelectedItem as OpcionSalon)?.Valor,
            Estado = (cboEstado.SelectedItem as OpcionEstado)?.Valor,
            Pagina = _paginaActual,
            TamanoPagina = Convert.ToInt32(cboTamanoPagina.SelectedItem ?? 50)
        };

        // Cada busqueda cancela la anterior.
        var token = _busqueda.Reiniciar();

        prgBuscando.Visible = true;
        btnCancelarBusqueda.Enabled = true;
        lblMensajeEstado.Text = "Buscando...";

        var pagina = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.ConsultarAsync(filtro, ct),
            token,
            "consulta de reservas",
            btnBuscar, btnAnterior, btnSiguiente);

        prgBuscando.Visible = false;
        btnCancelarBusqueda.Enabled = false;

        if (pagina is null)
        {
            lblMensajeEstado.Text = "Busqueda cancelada o sin completar.";
            return;
        }

        MostrarResultados(pagina);
    }

    private void MostrarResultados(PaginaResultado<ReservaResumenDto> pagina)
    {
        _totalPaginas = Math.Max(1, pagina.TotalPaginas);
        _paginaActual = pagina.Pagina;

        dgvReservas.DataSource = pagina.Elementos.Select(r => new
        {
            r.IdReserva,
            r.Codigo,
            Cliente = r.Cliente,
            Salon = r.Salon,
            Fecha = r.FechaEventoTexto,
            Horario = r.HorarioTexto,
            Invitados = r.NumeroInvitados,
            Detalles = r.TotalDetalles,
            Total = r.Total.ToString("N2"),
            Estado = r.EstadoTexto,
            IA = r.TieneAnalisisIA ? "Si" : "-",
            Correo = r.TieneCorreoEnviado ? "Si" : "-"
        }).ToList();

        ConfigurarColumnas();

        lblResultados.Text = pagina.TotalRegistros == 0
            ? "Sin coincidencias"
            : $"{pagina.Elementos.Count} de {pagina.TotalRegistros} reserva(s)";

        lblPagina.Text = $"Pagina {_paginaActual} de {_totalPaginas}";
        btnAnterior.Enabled = pagina.HayPaginaAnterior;
        btnSiguiente.Enabled = pagina.HayPaginaSiguiente;

        lblMensajeEstado.Text = pagina.TotalRegistros == 0
            ? "No se encontraron reservas con los filtros indicados."
            : $"Busqueda completada: {pagina.TotalRegistros} resultado(s).";
    }

    private void ConfigurarColumnas()
    {
        if (dgvReservas.Columns.Count == 0)
        {
            return;
        }

        dgvReservas.Columns["IdReserva"]!.Visible = false;
        dgvReservas.Columns["Codigo"]!.FillWeight = 110;
        dgvReservas.Columns["Cliente"]!.FillWeight = 140;
        dgvReservas.Columns["Salon"]!.FillWeight = 110;
        dgvReservas.Columns["Fecha"]!.FillWeight = 70;
        dgvReservas.Columns["Horario"]!.FillWeight = 80;
        dgvReservas.Columns["Invitados"]!.FillWeight = 60;
        dgvReservas.Columns["Detalles"]!.FillWeight = 55;
        dgvReservas.Columns["Total"]!.FillWeight = 70;
        dgvReservas.Columns["Estado"]!.FillWeight = 75;
        dgvReservas.Columns["IA"]!.FillWeight = 40;
        dgvReservas.Columns["Correo"]!.FillWeight = 50;

        dgvReservas.Columns["Total"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvReservas.Columns["Invitados"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvReservas.Columns["Detalles"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvReservas.Columns["IA"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvReservas.Columns["Correo"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    /// <summary>
    /// Colorea la columna Estado. Los estados deben distinguirse de un vistazo, sin tener que
    /// leer la celda, porque es lo primero que mira quien revisa la lista.
    /// </summary>
    private void dgvReservas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || dgvReservas.Columns[e.ColumnIndex].Name != "Estado")
        {
            return;
        }

        var estado = Convert.ToString(e.Value);

        e.CellStyle!.ForeColor = estado switch
        {
            "Confirmada" => Color.FromArgb(27, 127, 75),
            "Cancelada" => Color.FromArgb(179, 38, 30),
            "Finalizada" => Color.FromArgb(31, 56, 100),
            _ => Color.FromArgb(150, 90, 0)
        };

        e.CellStyle.Font = new Font(dgvReservas.Font, FontStyle.Bold);
    }

    private void btnCancelarBusqueda_Click(object sender, EventArgs e)
    {
        _busqueda.Cancelar();

        prgBuscando.Visible = false;
        btnCancelarBusqueda.Enabled = false;
        lblMensajeEstado.Text = "Busqueda cancelada por el usuario.";
    }

    private async void btnLimpiar_Click(object sender, EventArgs e)
    {
        _cargando = true;

        txtCodigo.Clear();
        txtCliente.Clear();
        chkFechas.Checked = false;
        cboSalon.SelectedIndex = 0;
        cboEstado.SelectedIndex = 0;

        _cargando = false;

        await BuscarAsync(reiniciarPagina: true);
    }

    private void chkFechas_CheckedChanged(object sender, EventArgs e)
    {
        dtpDesde.Enabled = chkFechas.Checked;
        dtpHasta.Enabled = chkFechas.Checked;
    }

    private async void btnAnterior_Click(object sender, EventArgs e)
    {
        if (_paginaActual <= 1)
        {
            return;
        }

        _paginaActual--;
        await BuscarAsync(reiniciarPagina: false);
    }

    private async void btnSiguiente_Click(object sender, EventArgs e)
    {
        if (_paginaActual >= _totalPaginas)
        {
            return;
        }

        _paginaActual++;
        await BuscarAsync(reiniciarPagina: false);
    }

    private async void cboTamanoPagina_SelectedIndexChanged(object sender, EventArgs e) =>
        await BuscarAsync(reiniciarPagina: true);

    // ================================================================================ acciones

    private void dgvReservas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            AbrirSeleccionada();
        }
    }

    private void btnAbrir_Click(object sender, EventArgs e) => AbrirSeleccionada();

    private void AbrirSeleccionada()
    {
        if (dgvReservas.CurrentRow is null)
        {
            ManejadorErrores.Advertir(this, "Seleccione una reserva de la lista.");
            return;
        }

        var idReserva = Convert.ToInt32(dgvReservas.CurrentRow.Cells["IdReserva"].Value);

        AbrirEdicion(idReserva);
    }

    private void btnNueva_Click(object sender, EventArgs e) => AbrirEdicion(null);

    private void AbrirEdicion(int? idReserva)
    {
        var formulario = new FrmReservaEdicion(_servicios, idReserva)
        {
            MdiParent = MdiParent,
            WindowState = FormWindowState.Maximized
        };

        // Cuando la reserva se guarda o cambia de estado, esta lista se refresca sola: el
        // usuario no tiene que acordarse de volver a pulsar Buscar.
        formulario.ReservaModificada += async (_, _) => await BuscarAsync(reiniciarPagina: false);

        formulario.Show();
    }

    private void FrmReservasConsulta_FormClosed(object sender, FormClosedEventArgs e)
    {
        _busqueda.Dispose();
        _cancelacion.Cancel();
        _cancelacion.Dispose();
    }

    // --------------------------------------------------------------------------------- apoyo

    private sealed record OpcionEstado(string Texto, EstadoReserva? Valor);

    private sealed record OpcionSalon(string Texto, int? Valor);
}
