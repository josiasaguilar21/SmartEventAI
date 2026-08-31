using System.Text;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Enums;
using SmartEvent.Infrastructure.Data.Registro;
using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Auditoria de las integraciones externas: intentos de correo y analisis de IA.
///
/// Esta pantalla EXISTE PARA DIAGNOSTICAR, asi que si muestra los mensajes tecnicos de error.
/// La diferencia con exponer informacion sensible es que esos mensajes los redactan los
/// servicios de integracion de forma controlada: describen la causa (credenciales rechazadas,
/// tiempo de espera agotado, limite de uso) sin incluir nunca el host, el usuario, la
/// contrasena ni la clave de API.
///
/// Es tambien el punto donde se reintenta un correo fallido. El reenvio NO cambia el estado de
/// la reserva: por eso puede repetirse sin riesgo de duplicar transiciones (CA-07).
/// </summary>
public partial class FrmAuditoriaIntegraciones : Form
{
    private readonly ContenedorServicios _servicios;
    private readonly CancellationTokenSource _cancelacion = new();

    private bool _cargando;
    private int _idReservaCorreoSeleccionado;
    private EstadoCorreo _estadoCorreoSeleccionado;

    public FrmAuditoriaIntegraciones(ContenedorServicios servicios)
    {
        _servicios = servicios ?? throw new ArgumentNullException(nameof(servicios));

        InitializeComponent();
    }

    private async void FrmAuditoriaIntegraciones_Load(object sender, EventArgs e)
    {
        _cargando = true;

        dtpDesdeCorreo.Value = DateTime.Today.AddDays(-30);
        dtpHastaCorreo.Value = DateTime.Today;

        cboEstadoCorreo.DisplayMember = nameof(OpcionCorreo.Texto);
        cboEstadoCorreo.ValueMember = nameof(OpcionCorreo.Valor);
        cboEstadoCorreo.DataSource = new List<OpcionCorreo>
        {
            new("Todos", null),
            new("Enviados", EstadoCorreo.Enviado),
            new("Con error", EstadoCorreo.Error)
        };

        cboResultadoAnalisis.DisplayMember = nameof(OpcionResultado.Texto);
        cboResultadoAnalisis.ValueMember = nameof(OpcionResultado.Valor);
        cboResultadoAnalisis.DataSource = new List<OpcionResultado>
        {
            new("Todos", null),
            new("Exitosos", true),
            new("Fallidos", false)
        };

        var niveles = new List<OpcionNivel> { new("Todos", null) };
        niveles.AddRange(Enum.GetValues<NivelRiesgo>().Select(n => new OpcionNivel(n.ToString().ToUpperInvariant(), n)));

        cboNivelRiesgo.DisplayMember = nameof(OpcionNivel.Texto);
        cboNivelRiesgo.ValueMember = nameof(OpcionNivel.Valor);
        cboNivelRiesgo.DataSource = niveles;

        // La ruta del registro local se muestra para que el diagnostico no dependa de adivinar
        // donde quedaron los archivos.
        if (_servicios.Registro is RegistroEventosArchivo registroArchivo)
        {
            lblArchivoRegistro.Text = $"Registro: {registroArchivo.ArchivoActual}";
        }

        _cargando = false;

        await BuscarCorreosAsync();
        await BuscarAnalisisAsync();
    }

    // ================================================================================ correos

    private async void btnBuscarCorreos_Click(object sender, EventArgs e) => await BuscarCorreosAsync();

    private async void FiltroCorreo_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        await BuscarCorreosAsync();
    }

    private void chkFechasCorreo_CheckedChanged(object sender, EventArgs e)
    {
        dtpDesdeCorreo.Enabled = chkFechasCorreo.Checked;
        dtpHastaCorreo.Enabled = chkFechasCorreo.Checked;
    }

    private async Task BuscarCorreosAsync()
    {
        if (_cargando)
        {
            return;
        }

        var filtro = new FiltroCorreoDto
        {
            Codigo = txtCodigoCorreo.Text.Trim(),
            Estado = (cboEstadoCorreo.SelectedItem as OpcionCorreo)?.Valor,
            FechaDesde = chkFechasCorreo.Checked ? DateOnly.FromDateTime(dtpDesdeCorreo.Value) : null,
            FechaHasta = chkFechasCorreo.Checked ? DateOnly.FromDateTime(dtpHastaCorreo.Value) : null
        };

        var correos = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Auditoria.ConsultarCorreosAsync(filtro, ct),
            _cancelacion.Token,
            "auditoria de correos",
            btnBuscarCorreos);

        if (correos is null)
        {
            return;
        }

        _cargando = true;

        dgvCorreos.DataSource = correos.Select(c => new
        {
            c.IdCorreo,
            c.IdReserva,
            Reserva = c.CodigoReserva,
            Cliente = c.ClienteReserva,
            Tipo = c.TipoNotificacion.ToString(),
            c.Destinatario,
            c.Asunto,
            Fecha = c.FechaIntento.ToString("dd/MM/yyyy HH:mm"),
            Estado = c.Estado == EstadoCorreo.Enviado ? "ENVIADO" : "ERROR",
            Error = c.Error ?? string.Empty,
            Usuario = c.Usuario ?? string.Empty
        }).ToList();

        if (dgvCorreos.Columns.Count > 0)
        {
            dgvCorreos.Columns["IdCorreo"]!.Visible = false;
            dgvCorreos.Columns["IdReserva"]!.Visible = false;
            dgvCorreos.Columns["Error"]!.Visible = false;
            dgvCorreos.Columns["Reserva"]!.FillWeight = 85;
            dgvCorreos.Columns["Cliente"]!.FillWeight = 130;
            dgvCorreos.Columns["Tipo"]!.FillWeight = 85;
            dgvCorreos.Columns["Destinatario"]!.FillWeight = 130;
            dgvCorreos.Columns["Asunto"]!.FillWeight = 170;
            dgvCorreos.Columns["Fecha"]!.FillWeight = 95;
            dgvCorreos.Columns["Estado"]!.FillWeight = 70;
            dgvCorreos.Columns["Usuario"]!.FillWeight = 80;
        }

        _cargando = false;

        txtErrorCorreo.Clear();
        btnReenviar.Enabled = false;

        var conError = correos.Count(c => c.Estado == EstadoCorreo.Error);

        lblMensajeEstado.Text = correos.Count == 0
            ? "No hay intentos de correo con los filtros indicados."
            : $"{correos.Count} intento(s) de correo. Con error: {conError}.";
    }

    private void dgvCorreos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || dgvCorreos.Columns[e.ColumnIndex].Name != "Estado")
        {
            return;
        }

        var enviado = Convert.ToString(e.Value) == "ENVIADO";

        e.CellStyle!.ForeColor = enviado ? Color.FromArgb(27, 127, 75) : Color.FromArgb(179, 38, 30);
        e.CellStyle.Font = new Font(dgvCorreos.Font, FontStyle.Bold);
    }

    private void dgvCorreos_SelectionChanged(object sender, EventArgs e)
    {
        if (_cargando || dgvCorreos.CurrentRow is null)
        {
            return;
        }

        var fila = dgvCorreos.CurrentRow;

        _idReservaCorreoSeleccionado = Convert.ToInt32(fila.Cells["IdReserva"].Value);
        _estadoCorreoSeleccionado = Convert.ToString(fila.Cells["Estado"].Value) == "ENVIADO"
            ? EstadoCorreo.Enviado
            : EstadoCorreo.Error;

        var error = Convert.ToString(fila.Cells["Error"].Value) ?? string.Empty;

        var detalle = new StringBuilder()
            .AppendLine($"Reserva      : {fila.Cells["Reserva"].Value}")
            .AppendLine($"Destinatario : {fila.Cells["Destinatario"].Value}")
            .AppendLine($"Asunto       : {fila.Cells["Asunto"].Value}")
            .AppendLine($"Fecha        : {fila.Cells["Fecha"].Value}")
            .AppendLine($"Resultado    : {fila.Cells["Estado"].Value}")
            .AppendLine();

        detalle.Append(string.IsNullOrWhiteSpace(error)
            ? "El envio se completo sin errores."
            : "Motivo del fallo:" + Environment.NewLine + error);

        txtErrorCorreo.Text = detalle.ToString();

        // Solo tiene sentido reenviar cuando hubo un fallo previo.
        btnReenviar.Enabled = _estadoCorreoSeleccionado == EstadoCorreo.Error;
    }

    /// <summary>
    /// Reintento explicito y auditable (CA-07). No modifica el estado de la reserva: solo
    /// vuelve a componer y enviar la notificacion, dejando un nuevo registro del intento.
    /// </summary>
    private async void btnReenviar_Click(object sender, EventArgs e)
    {
        if (_idReservaCorreoSeleccionado == 0)
        {
            return;
        }

        if (!ManejadorErrores.Confirmar(this,
                "Se volvera a enviar la notificacion de esta reserva.\n\n" +
                "El estado de la reserva NO cambia: solo se repite el envio y queda un nuevo " +
                "registro en la auditoria.\n\nDesea continuar?",
                "Reenviar notificacion"))
        {
            return;
        }

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.ReenviarNotificacionAsync(_idReservaCorreoSeleccionado, ct),
            _cancelacion.Token,
            "reenvio de notificacion",
            btnReenviar);

        if (resultado is null)
        {
            return;
        }

        if (resultado.Enviado)
        {
            ManejadorErrores.Informar(this,
                $"La notificacion se envio correctamente a {resultado.Destinatario}.",
                "Reenvio completado");
        }
        else
        {
            ManejadorErrores.Advertir(this,
                "El reenvio volvio a fallar:" + Environment.NewLine + Environment.NewLine +
                resultado.Error + Environment.NewLine + Environment.NewLine +
                "Este segundo intento tambien quedo auditado.",
                "El reenvio no se completo");
        }

        await BuscarCorreosAsync();
    }

    // =============================================================================== analisis

    private async void btnBuscarAnalisis_Click(object sender, EventArgs e) => await BuscarAnalisisAsync();

    private async void FiltroAnalisis_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        await BuscarAnalisisAsync();
    }

    private async Task BuscarAnalisisAsync()
    {
        if (_cargando)
        {
            return;
        }

        var filtro = new FiltroAnalisisDto
        {
            Codigo = txtCodigoAnalisis.Text.Trim(),
            Exitoso = (cboResultadoAnalisis.SelectedItem as OpcionResultado)?.Valor,
            NivelRiesgo = (cboNivelRiesgo.SelectedItem as OpcionNivel)?.Valor
        };

        var analisis = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Auditoria.ConsultarAnalisisAsync(filtro, ct),
            _cancelacion.Token,
            "auditoria de analisis de IA",
            btnBuscarAnalisis);

        if (analisis is null)
        {
            return;
        }

        _cargando = true;

        dgvAnalisis.DataSource = analisis.Select(a => new
        {
            a.IdAnalisis,
            Reserva = a.CodigoReserva,
            Cliente = a.ClienteReserva,
            a.Modelo,
            Prompt = a.PromptVersion,
            Riesgo = a.NivelRiesgo?.ToString().ToUpperInvariant() ?? "-",
            Tokens = a.TokensEntrada.HasValue || a.TokensSalida.HasValue
                ? $"{a.TokensEntrada?.ToString() ?? "?"}/{a.TokensSalida?.ToString() ?? "?"}"
                : "-",
            Fecha = a.Fecha.ToString("dd/MM/yyyy HH:mm"),
            Resultado = a.Exitoso ? "EXITOSO" : "FALLIDO",
            Json = a.RespuestaJson ?? string.Empty,
            Error = a.Error ?? string.Empty,
            Usuario = a.Usuario ?? string.Empty
        }).ToList();

        if (dgvAnalisis.Columns.Count > 0)
        {
            dgvAnalisis.Columns["IdAnalisis"]!.Visible = false;
            dgvAnalisis.Columns["Json"]!.Visible = false;
            dgvAnalisis.Columns["Error"]!.Visible = false;
            dgvAnalisis.Columns["Reserva"]!.FillWeight = 85;
            dgvAnalisis.Columns["Cliente"]!.FillWeight = 130;
            dgvAnalisis.Columns["Modelo"]!.FillWeight = 130;
            dgvAnalisis.Columns["Prompt"]!.FillWeight = 55;
            dgvAnalisis.Columns["Riesgo"]!.FillWeight = 60;
            dgvAnalisis.Columns["Tokens"]!.FillWeight = 65;
            dgvAnalisis.Columns["Fecha"]!.FillWeight = 95;
            dgvAnalisis.Columns["Resultado"]!.FillWeight = 70;
            dgvAnalisis.Columns["Usuario"]!.FillWeight = 80;
        }

        _cargando = false;

        txtDetalleAnalisis.Clear();

        var fallidos = analisis.Count(a => !a.Exitoso);

        lblMensajeEstado.Text = analisis.Count == 0
            ? "No hay analisis de IA con los filtros indicados."
            : $"{analisis.Count} analisis registrado(s). Fallidos: {fallidos}.";
    }

    private void dgvAnalisis_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var columna = dgvAnalisis.Columns[e.ColumnIndex].Name;

        if (columna == "Resultado")
        {
            var exitoso = Convert.ToString(e.Value) == "EXITOSO";

            e.CellStyle!.ForeColor = exitoso ? Color.FromArgb(27, 127, 75) : Color.FromArgb(179, 38, 30);
            e.CellStyle.Font = new Font(dgvAnalisis.Font, FontStyle.Bold);
        }
        else if (columna == "Riesgo")
        {
            e.CellStyle!.ForeColor = Convert.ToString(e.Value) switch
            {
                "ALTO" => Color.FromArgb(179, 38, 30),
                "MEDIO" => Color.FromArgb(150, 90, 0),
                "BAJO" => Color.FromArgb(27, 127, 75),
                _ => SystemColors.GrayText
            };
        }
    }

    private void dgvAnalisis_SelectionChanged(object sender, EventArgs e)
    {
        if (_cargando || dgvAnalisis.CurrentRow is null)
        {
            return;
        }

        var fila = dgvAnalisis.CurrentRow;
        var json = Convert.ToString(fila.Cells["Json"].Value) ?? string.Empty;
        var error = Convert.ToString(fila.Cells["Error"].Value) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(json))
        {
            // El JSON se reformatea con sangria para que sea legible en pantalla; es la
            // evidencia de que la respuesta llego estructurada y validada.
            txtDetalleAnalisis.Text = _servicios.Auditoria.FormatearJson(json)
                                                          .Replace("\n", Environment.NewLine);
            return;
        }

        txtDetalleAnalisis.Text = string.IsNullOrWhiteSpace(error)
            ? "Sin informacion adicional."
            : "El analisis no se completo." + Environment.NewLine + Environment.NewLine +
              "Motivo:" + Environment.NewLine + error;
    }

    private void FrmAuditoriaIntegraciones_FormClosed(object sender, FormClosedEventArgs e)
    {
        _cancelacion.Cancel();
        _cancelacion.Dispose();
    }

    // --------------------------------------------------------------------------------- apoyo

    private sealed record OpcionCorreo(string Texto, EstadoCorreo? Valor);

    private sealed record OpcionResultado(string Texto, bool? Valor);

    private sealed record OpcionNivel(string Texto, NivelRiesgo? Valor);
}
