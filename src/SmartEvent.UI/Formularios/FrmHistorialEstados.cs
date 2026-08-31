using SmartEvent.Core.Enums;
using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Historial de transiciones de estado de una reserva, leido de evt.ReservaAuditoria.
///
/// Es la evidencia visible de que una reserva cambia de estado UNA SOLA VEZ por transicion.
/// Importa especialmente al reintentar un correo fallido: el reenvio genera un registro mas en
/// com.CorreoEnviado, pero NINGUNA fila nueva aqui, porque no toca el estado de la reserva.
///
/// La auditoria no la escribe esta aplicacion: la escribe evt.sp_Reserva_CambiarEstado dentro
/// de la misma transaccion que aplica el cambio. Por eso no puede quedar un cambio sin su
/// registro ni un registro sin su cambio.
/// </summary>
public partial class FrmHistorialEstados : Form
{
    private readonly ContenedorServicios _servicios;
    private readonly int _idReserva;
    private readonly string _codigo;
    private readonly CancellationTokenSource _cancelacion = new();

    private FrmHistorialEstados(ContenedorServicios servicios, int idReserva, string codigo)
    {
        _servicios = servicios;
        _idReserva = idReserva;
        _codigo = codigo;

        InitializeComponent();

        Text = $"Historial de estados - Reserva {codigo}";
    }

    /// <summary>Abre el historial de una reserva como dialogo modal.</summary>
    public static void Mostrar(IWin32Window propietario, ContenedorServicios servicios,
                               int idReserva, string codigo)
    {
        using var formulario = new FrmHistorialEstados(servicios, idReserva, codigo);
        formulario.ShowDialog(propietario);
    }

    private async void FrmHistorialEstados_Load(object sender, EventArgs e)
    {
        lblEncabezado.Text =
            $"Reserva {_codigo}. Cada fila es un cambio de estado registrado por el motor de base " +
            "de datos dentro de la misma transaccion que lo aplico.";

        var historial = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.ObtenerHistorialAsync(_idReserva, ct),
            _cancelacion.Token,
            "consulta del historial de estados");

        if (historial is null)
        {
            return;
        }

        // Se muestra en orden cronologico: asi se lee el recorrido de la reserva de arriba abajo.
        dgvHistorial.DataSource = historial
            .OrderBy(h => h.Fecha)
            .ThenBy(h => h.IdAuditoria)
            .Select((h, indice) => new
            {
                Nro = indice + 1,
                Transicion = h.TransicionTexto,
                Estado = h.EstadoNuevo.ATextoUsuario(),
                Fecha = h.Fecha.ToString("dd/MM/yyyy HH:mm:ss"),
                Usuario = h.Usuario,
                Motivo = h.Motivo ?? string.Empty
            })
            .ToList();

        if (dgvHistorial.Columns.Count > 0)
        {
            dgvHistorial.Columns["Estado"]!.Visible = false;   // solo se usa para colorear
            dgvHistorial.Columns["Nro"]!.HeaderText = "#";
            dgvHistorial.Columns["Nro"]!.FillWeight = 25;
            dgvHistorial.Columns["Transicion"]!.HeaderText = "Cambio de estado";
            dgvHistorial.Columns["Transicion"]!.FillWeight = 110;
            dgvHistorial.Columns["Fecha"]!.FillWeight = 90;
            dgvHistorial.Columns["Usuario"]!.FillWeight = 70;
            dgvHistorial.Columns["Motivo"]!.FillWeight = 180;
            dgvHistorial.Columns["Nro"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        // Recuento por estado: es lo que demuestra que no hay transiciones duplicadas.
        var resumen = historial
            .GroupBy(h => h.EstadoNuevo)
            .Select(g => $"{g.Key.ATextoUsuario()}: {g.Count()}")
            .ToList();

        lblResumen.Text = $"{historial.Count} cambio(s) de estado   |   " + string.Join("   |   ", resumen);
    }

    /// <summary>Colorea la transicion con el color del estado al que se llego.</summary>
    private void dgvHistorial_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || dgvHistorial.Columns[e.ColumnIndex].Name != "Transicion")
        {
            return;
        }

        var estado = Convert.ToString(dgvHistorial.Rows[e.RowIndex].Cells["Estado"].Value);

        e.CellStyle!.ForeColor = estado switch
        {
            "Confirmada" => Color.FromArgb(27, 127, 75),
            "Cancelada" => Color.FromArgb(179, 38, 30),
            "Finalizada" => Color.FromArgb(31, 56, 100),
            _ => Color.FromArgb(150, 90, 0)
        };

        e.CellStyle.Font = new Font(dgvHistorial.Font, FontStyle.Bold);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _cancelacion.Cancel();
        _cancelacion.Dispose();

        base.OnFormClosed(e);
    }
}
