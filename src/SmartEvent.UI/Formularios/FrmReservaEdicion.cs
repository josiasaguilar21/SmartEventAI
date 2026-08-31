using SmartEvent.Application.Calculo;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;
using SmartEvent.UI.Comun;
using SmartEvent.UI.Composicion;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Alta y edicion de una reserva con su detalle.
///
/// Es la pantalla central del sistema y concentra cuatro comportamientos exigidos:
///   1. Calculo de totales EN TIEMPO REAL mientras se edita la grilla.
///   2. Validacion previa de disponibilidad antes de gastar un guardado.
///   3. Guardado ATOMICO: cabecera y detalle viajan juntos en una sola llamada.
///   4. Acciones de estado (confirmar, cancelar, finalizar) y analisis con IA.
///
/// El formulario no calcula nada que sea vinculante: los importes que muestra son una
/// previsualizacion y se sustituyen por los que devuelve SQL Server en cuanto se guarda.
/// </summary>
public partial class FrmReservaEdicion : Form
{
    private readonly ContenedorServicios _servicios;
    private readonly CancellationTokenSource _cancelacion = new();

    /// <summary>Permite cancelar un analisis de IA en curso sin cerrar la pantalla.</summary>
    private readonly OperacionCancelable _operacionIA;

    private int? _idReserva;
    private EstadoReserva _estado = EstadoReserva.Borrador;
    private string _codigo = string.Empty;

    private IReadOnlyList<Cliente> _clientes = Array.Empty<Cliente>();
    private IReadOnlyList<Salon> _salones = Array.Empty<Salon>();
    private IReadOnlyList<Recurso> _recursos = Array.Empty<Recurso>();

    private bool _cargando;
    private bool _hayCambiosSinGuardar;

    public FrmReservaEdicion(ContenedorServicios servicios, int? idReserva)
    {
        _servicios = servicios ?? throw new ArgumentNullException(nameof(servicios));
        _idReserva = idReserva;

        InitializeComponent();

        _operacionIA = new OperacionCancelable(_cancelacion.Token);
    }

    /// <summary>Se activa cuando la reserva se guarda o cambia de estado, para refrescar la consulta.</summary>
    public event EventHandler? ReservaModificada;

    // ================================================================================== carga

    private async void FrmReservaEdicion_Load(object sender, EventArgs e)
    {
        _cargando = true;

        dtpFecha.MinDate = DateTime.Today.AddYears(-2);
        dtpFecha.Value = DateTime.Today.AddDays(7);
        dtpHoraInicio.Value = DateTime.Today.AddHours(9);
        dtpHoraFin.Value = DateTime.Today.AddHours(13);

        var cargado = await EjecucionUi.EjecutarAsync(
            this, CargarCatalogosAsync, _cancelacion.Token, "carga de catalogos");

        if (!cargado)
        {
            _cargando = false;
            return;
        }

        if (_idReserva.HasValue)
        {
            await CargarReservaAsync(_idReserva.Value);
        }
        else
        {
            _codigo = string.Empty;
            lblCodigo.Text = "(nueva)";
            Text = "Reserva - Nueva";
        }

        _cargando = false;

        ActualizarDuracion();
        RecalcularTotales();
        AplicarEstadoAControles();
    }

    private async Task CargarCatalogosAsync(CancellationToken ct)
    {
        // Las tres consultas son independientes: se lanzan a la vez y se espera al conjunto.
        // Con tres idas y vueltas secuenciales la pantalla tardaria el triple en estar lista.
        var tareaClientes = _servicios.Catalogos.ConsultarClientesAsync(null, true, ct);
        var tareaSalones = _servicios.Catalogos.ConsultarSalonesAsync(null, true, null, ct);
        var tareaRecursos = _servicios.Catalogos.ConsultarRecursosAsync(null, null, true, ct);

        await Task.WhenAll(tareaClientes, tareaSalones, tareaRecursos).ConfigureAwait(true);

        _clientes = tareaClientes.Result;
        _salones = tareaSalones.Result;
        _recursos = tareaRecursos.Result;

        cboCliente.DisplayMember = nameof(Cliente.Descripcion);
        cboCliente.ValueMember = nameof(Cliente.IdCliente);
        cboCliente.DataSource = new List<Cliente>(_clientes);
        cboCliente.SelectedIndex = -1;

        cboSalon.DisplayMember = nameof(Salon.Descripcion);
        cboSalon.ValueMember = nameof(Salon.IdSalon);
        cboSalon.DataSource = new List<Salon>(_salones);
        cboSalon.SelectedIndex = -1;

        cboRecurso.DisplayMember = nameof(Recurso.Descripcion);
        cboRecurso.ValueMember = nameof(Recurso.IdRecurso);
        cboRecurso.DataSource = new List<Recurso>(_recursos);
        cboRecurso.SelectedIndex = -1;
    }

    private async Task CargarReservaAsync(int idReserva)
    {
        var reserva = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.ObtenerAsync(idReserva, ct),
            _cancelacion.Token,
            "carga de la reserva");

        if (reserva is null)
        {
            ManejadorErrores.Advertir(this, "La reserva solicitada ya no existe.");
            Close();
            return;
        }

        _cargando = true;

        _idReserva = reserva.IdReserva;
        _codigo = reserva.Codigo;
        _estado = reserva.Estado;

        lblCodigo.Text = reserva.Codigo;
        Text = $"Reserva {reserva.Codigo}";

        cboCliente.SelectedValue = reserva.IdCliente;
        cboSalon.SelectedValue = reserva.IdSalon;

        dtpFecha.Value = reserva.FechaEvento.ToDateTime(TimeOnly.MinValue);
        dtpHoraInicio.Value = DateTime.Today.Add(reserva.HoraInicio.ToTimeSpan());
        dtpHoraFin.Value = DateTime.Today.Add(reserva.HoraFin.ToTimeSpan());
        numInvitados.Value = reserva.NumeroInvitados;
        txtObservacion.Text = reserva.Observacion ?? string.Empty;
        numDescuentoGlobal.Value = reserva.Descuento;

        dgvDetalle.Rows.Clear();

        foreach (var detalle in reserva.Detalles)
        {
            dgvDetalle.Rows.Add(
                detalle.IdRecurso,
                detalle.Recurso,
                detalle.TipoRecurso.ToString(),
                detalle.Cantidad,
                detalle.PrecioUnitario,
                detalle.PorcentajeDescuento,
                detalle.SubtotalLinea,
                detalle.StockTotal);
        }

        _cargando = false;
        _hayCambiosSinGuardar = false;

        MostrarEstado(reserva.Estado);
        ActualizarCapacidad();
        ActualizarDuracion();
        RecalcularTotales();
        AplicarEstadoAControles();

        // Los importes que se muestran tras cargar son los persistidos por SQL Server.
        lblSubtotal.Text = reserva.Subtotal.ToString("N2");
        lblImpuesto.Text = reserva.Impuesto.ToString("N2");
        lblTotal.Text = reserva.Total.ToString("N2");
    }

    // ============================================================== estado de los controles

    private void MostrarEstado(EstadoReserva estado)
    {
        _estado = estado;
        lblEstado.Text = estado.ATextoUsuario().ToUpperInvariant();

        lblEstado.ForeColor = estado switch
        {
            EstadoReserva.Confirmada => Color.FromArgb(27, 127, 75),
            EstadoReserva.Cancelada => Color.FromArgb(179, 38, 30),
            EstadoReserva.Finalizada => Color.FromArgb(31, 56, 100),
            _ => Color.FromArgb(150, 90, 0)
        };
    }

    /// <summary>
    /// Habilita o deshabilita los controles segun el estado de la reserva.
    ///
    /// Una reserva que ya no esta en BORRADOR no puede editar cliente, salon, fecha, horario ni
    /// detalles. Aqui se refleja visualmente; la regla la impone evt.sp_Reserva_Guardar, que
    /// rechaza cualquier intento aunque se manipule la interfaz.
    /// </summary>
    private void AplicarEstadoAControles()
    {
        var editable = _estado.PermiteEdicion();
        var existe = _idReserva.HasValue;

        cboCliente.Enabled = editable;
        cboSalon.Enabled = editable;
        dtpFecha.Enabled = editable;
        dtpHoraInicio.Enabled = editable;
        dtpHoraFin.Enabled = editable;
        numInvitados.Enabled = editable;
        txtObservacion.ReadOnly = !editable;
        numDescuentoGlobal.Enabled = editable;

        pnlAgregar.Enabled = editable;
        dgvDetalle.ReadOnly = !editable;

        btnGuardar.Enabled = editable;
        btnValidar.Enabled = editable;

        btnAnalizarIA.Enabled = existe;
        btnHistorial.Enabled = existe;
        btnConfirmar.Enabled = existe && _estado == EstadoReserva.Borrador;
        btnCancelarReserva.Enabled = existe && _estado is EstadoReserva.Borrador or EstadoReserva.Confirmada;
        btnFinalizar.Enabled = existe && _estado == EstadoReserva.Confirmada;

        // El descuento de linea admite hasta 20 %, pero por encima del 10 % requiere rol
        // ADMINISTRADOR. Se acota el control para que el coordinador ni siquiera pueda teclearlo.
        numDescuentoLinea.Maximum = _servicios.Contexto.Requerida.PuedeAplicarDescuentoAlto ? 20m : 10m;
    }

    private void ActualizarCapacidad()
    {
        if (cboSalon.SelectedItem is not Salon salon)
        {
            lblCapacidad.Text = string.Empty;
            return;
        }

        var invitados = (int)numInvitados.Value;
        var excede = invitados > salon.Capacidad;

        lblCapacidad.Text = $"Capacidad: {salon.Capacidad}";
        lblCapacidad.ForeColor = excede ? Color.FromArgb(179, 38, 30) : SystemColors.GrayText;

        numInvitados.ForeColor = excede ? Color.FromArgb(179, 38, 30) : SystemColors.WindowText;
    }

    private void ActualizarDuracion()
    {
        var horas = (dtpHoraFin.Value.TimeOfDay - dtpHoraInicio.Value.TimeOfDay).TotalHours;

        if (horas <= 0)
        {
            lblDuracion.Text = "Horario invalido";
            lblDuracion.ForeColor = Color.FromArgb(179, 38, 30);
            return;
        }

        var valida = horas is >= 2 and <= 12;

        lblDuracion.Text = $"{horas:0.#} h";
        lblDuracion.ForeColor = valida ? SystemColors.GrayText : Color.FromArgb(179, 38, 30);
    }

    private void cboSalon_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_cargando)
        {
            return;
        }

        MarcarCambio();
        ActualizarCapacidad();
        RecalcularTotales();   // la tarifa base del salon forma parte del subtotal
    }

    private void numInvitados_ValueChanged(object sender, EventArgs e)
    {
        if (_cargando)
        {
            return;
        }

        MarcarCambio();
        ActualizarCapacidad();
    }

    private void Horario_ValueChanged(object sender, EventArgs e)
    {
        if (_cargando)
        {
            return;
        }

        MarcarCambio();
        ActualizarDuracion();
    }

    private void MarcarCambio() => _hayCambiosSinGuardar = true;

    // ================================================================== detalle de la reserva

    private void cboRecurso_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Al elegir un recurso se propone su precio de catalogo; el usuario puede cambiarlo.
        if (cboRecurso.SelectedItem is Recurso recurso)
        {
            numPrecio.Value = recurso.PrecioUnitario;
            numCantidad.Maximum = Math.Max(1, recurso.StockTotal);
            return;
        }

        // Sin recurso seleccionado no debe quedar un precio residual del enlace de datos.
        numPrecio.Value = 0m;
        numCantidad.Value = 1;
    }

    private void btnAgregarDetalle_Click(object sender, EventArgs e)
    {
        if (cboRecurso.SelectedItem is not Recurso recurso)
        {
            ManejadorErrores.Advertir(this, "Seleccione el recurso que desea agregar.");
            cboRecurso.Focus();
            return;
        }

        // Un recurso no puede repetirse: si ya esta en la grilla se suma la cantidad en lugar
        // de crear una segunda linea que la base rechazaria.
        foreach (DataGridViewRow fila in dgvDetalle.Rows)
        {
            if (Convert.ToInt32(fila.Cells[colIdRecurso.Name].Value) != recurso.IdRecurso)
            {
                continue;
            }

            var cantidadActual = Convert.ToInt32(fila.Cells[colCantidad.Name].Value);
            var nuevaCantidad = cantidadActual + (int)numCantidad.Value;

            if (nuevaCantidad > recurso.StockTotal)
            {
                ManejadorErrores.Advertir(this,
                    $"'{recurso.Nombre}' ya esta en el detalle con {cantidadActual} unidades. " +
                    $"Sumar {numCantidad.Value} superaria el inventario total ({recurso.StockTotal}).");
                return;
            }

            fila.Cells[colCantidad.Name].Value = nuevaCantidad;
            RecalcularFila(fila);
            RecalcularTotales();
            MarcarCambio();

            lblMensajeEstado.Text = $"Se acumulo la cantidad de '{recurso.Nombre}' en la linea existente.";
            return;
        }

        dgvDetalle.Rows.Add(
            recurso.IdRecurso,
            recurso.Nombre,
            recurso.Tipo.ToString(),
            (int)numCantidad.Value,
            numPrecio.Value,
            numDescuentoLinea.Value,
            0m,
            recurso.StockTotal);

        RecalcularFila(dgvDetalle.Rows[^1]);
        RecalcularTotales();
        MarcarCambio();

        cboRecurso.SelectedIndex = -1;
        numCantidad.Value = 1;
        numPrecio.Value = 0m;
        numDescuentoLinea.Value = 0m;
        cboRecurso.Focus();
    }

    private void btnQuitarDetalle_Click(object sender, EventArgs e)
    {
        if (dgvDetalle.CurrentRow is null)
        {
            ManejadorErrores.Advertir(this, "Seleccione la linea que desea quitar.");
            return;
        }

        dgvDetalle.Rows.Remove(dgvDetalle.CurrentRow);

        RecalcularTotales();
        MarcarCambio();
    }

    /// <summary>
    /// Valida la celda ANTES de aceptar el valor. Es lo que impide que un texto o un numero
    /// fuera de rango llegue siquiera al modelo.
    /// </summary>
    private void dgvDetalle_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
    {
        if (_cargando || e.RowIndex < 0)
        {
            return;
        }

        var columna = dgvDetalle.Columns[e.ColumnIndex].Name;
        var texto = Convert.ToString(e.FormattedValue) ?? string.Empty;

        if (columna == colCantidad.Name)
        {
            if (!int.TryParse(texto, out var cantidad) || cantidad <= 0)
            {
                MostrarErrorCelda(e, "La cantidad debe ser un numero entero mayor que cero.");
                return;
            }

            var stock = Convert.ToInt32(dgvDetalle.Rows[e.RowIndex].Cells[colStock.Name].Value);

            if (cantidad > stock)
            {
                MostrarErrorCelda(e, $"La cantidad supera el inventario total registrado ({stock}).");
            }
        }
        else if (columna == colPrecio.Name)
        {
            if (!decimal.TryParse(texto, out var precio) || precio < 0)
            {
                MostrarErrorCelda(e, "El precio unitario debe ser un numero mayor o igual a cero.");
            }
        }
        else if (columna == colDescuento.Name)
        {
            if (!decimal.TryParse(texto, out var descuento) || descuento < 0 || descuento > 20)
            {
                MostrarErrorCelda(e, "El descuento de linea debe estar entre 0 y 20 por ciento.");
                return;
            }

            if (descuento > 10 && !_servicios.Contexto.Requerida.PuedeAplicarDescuentoAlto)
            {
                MostrarErrorCelda(e,
                    "Su rol solo permite descuentos de hasta el 10 por ciento. " +
                    "Solicite la autorizacion de un administrador.");
            }
        }
    }

    /// <summary>
    /// Rechaza el valor escrito en una celda.
    ///
    /// Se avisa por TRES vias a la vez y no es redundancia gratuita: el icono de error en el
    /// encabezado de fila deja la marca visible mientras el problema persista, la barra de
    /// estado conserva el texto completo, y el aviso modal garantiza que el usuario se entere.
    /// Sin el aviso, cancelar la validacion dejaria la celda bloqueada sin explicar por que.
    /// </summary>
    private void MostrarErrorCelda(DataGridViewCellValidatingEventArgs e, string mensaje)
    {
        e.Cancel = true;

        dgvDetalle.Rows[e.RowIndex].ErrorText = mensaje;
        lblMensajeEstado.Text = mensaje;

        ManejadorErrores.Advertir(this,
            mensaje + Environment.NewLine + Environment.NewLine +
            "Corrija el valor o pulse Esc para deshacer el cambio.",
            "Valor no admitido");
    }

    private void dgvDetalle_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        if (_cargando || e.RowIndex < 0)
        {
            return;
        }

        dgvDetalle.Rows[e.RowIndex].ErrorText = string.Empty;

        RecalcularFila(dgvDetalle.Rows[e.RowIndex]);
        RecalcularTotales();
        MarcarCambio();
    }

    /// <summary>Evita el dialogo de error nativo de la grilla; el mensaje ya se muestra en la barra.</summary>
    private void dgvDetalle_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        e.ThrowException = false;
        lblMensajeEstado.Text = "El valor escrito no es valido para esa columna.";
    }

    private void RecalcularFila(DataGridViewRow fila)
    {
        var cantidad = Convert.ToInt32(fila.Cells[colCantidad.Name].Value);
        var precio = Convert.ToDecimal(fila.Cells[colPrecio.Name].Value);
        var descuento = Convert.ToDecimal(fila.Cells[colDescuento.Name].Value);

        fila.Cells[colSubtotalLinea.Name].Value =
            CalculadoraTotales.CalcularSubtotalLinea(cantidad, precio, descuento);
    }

    private void numDescuentoGlobal_ValueChanged(object sender, EventArgs e)
    {
        if (_cargando)
        {
            return;
        }

        MarcarCambio();
        RecalcularTotales();
    }

    /// <summary>
    /// Recalculo en tiempo real. Es una PREVISUALIZACION: usa la misma formula que el
    /// procedimiento almacenado, pero el valor que se persiste lo decide siempre SQL Server.
    /// </summary>
    private void RecalcularTotales()
    {
        var tarifaBase = (cboSalon.SelectedItem as Salon)?.TarifaBase ?? 0m;

        var totales = CalculadoraTotales.Calcular(tarifaBase, LeerDetalles(), numDescuentoGlobal.Value);

        lblSubtotal.Text = totales.Subtotal.ToString("N2");
        lblImpuesto.Text = totales.Impuesto.ToString("N2");
        lblTotal.Text = totales.Total.ToString("N2");

        // El descuento global no puede superar el subtotal: se avisa en el momento.
        var excede = numDescuentoGlobal.Value > totales.Subtotal;
        numDescuentoGlobal.ForeColor = excede ? Color.FromArgb(179, 38, 30) : SystemColors.WindowText;
    }

    private List<ReservaDetalleGuardarDto> LeerDetalles()
    {
        var detalles = new List<ReservaDetalleGuardarDto>();

        foreach (DataGridViewRow fila in dgvDetalle.Rows)
        {
            if (fila.IsNewRow)
            {
                continue;
            }

            detalles.Add(new ReservaDetalleGuardarDto
            {
                IdRecurso = Convert.ToInt32(fila.Cells[colIdRecurso.Name].Value),
                Cantidad = Convert.ToInt32(fila.Cells[colCantidad.Name].Value),
                PrecioUnitario = Convert.ToDecimal(fila.Cells[colPrecio.Name].Value),
                PorcentajeDescuento = Convert.ToDecimal(fila.Cells[colDescuento.Name].Value)
            });
        }

        return detalles;
    }

    private ReservaGuardarDto ConstruirDto() => new()
    {
        IdReserva = _idReserva,
        IdCliente = (cboCliente.SelectedItem as Cliente)?.IdCliente ?? 0,
        IdSalon = (cboSalon.SelectedItem as Salon)?.IdSalon ?? 0,
        FechaEvento = DateOnly.FromDateTime(dtpFecha.Value),
        HoraInicio = TimeOnly.FromTimeSpan(new TimeSpan(dtpHoraInicio.Value.Hour, dtpHoraInicio.Value.Minute, 0)),
        HoraFin = TimeOnly.FromTimeSpan(new TimeSpan(dtpHoraFin.Value.Hour, dtpHoraFin.Value.Minute, 0)),
        NumeroInvitados = (int)numInvitados.Value,
        Descuento = numDescuentoGlobal.Value,
        Observacion = txtObservacion.Text.Trim(),
        Detalles = LeerDetalles()
    };

    // ============================================================================== acciones

    private async void btnValidar_Click(object sender, EventArgs e)
    {
        var dto = ConstruirDto();

        // Primero las reglas locales: son inmediatas y no consumen una llamada al servidor.
        var validacion = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.ValidarAsync(dto, ct),
            _cancelacion.Token,
            "validacion de la reserva",
            btnValidar);

        if (validacion is null)
        {
            return;
        }

        if (!validacion.EsValido)
        {
            ManejadorErrores.Advertir(this, validacion.MensajeCompleto(), "Revise los datos");
            lblMensajeEstado.Text = $"{validacion.Errores.Count} punto(s) por corregir.";
            return;
        }

        // Despues la disponibilidad, que solo puede resolver el motor.
        var disponibilidad = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.ComprobarDisponibilidadAsync(dto, ct),
            _cancelacion.Token,
            "comprobacion de disponibilidad",
            btnValidar);

        if (disponibilidad is null)
        {
            return;
        }

        if (disponibilidad.EsValido)
        {
            lblMensajeEstado.Text = disponibilidad.Mensaje;
            ManejadorErrores.Informar(this, disponibilidad.Mensaje, "Disponibilidad");
            return;
        }

        var detalle = string.Join(Environment.NewLine,
            disponibilidad.Conflictos.Select(c => $"  - [{c.Tipo}] {c.Detalle}"));

        ManejadorErrores.Advertir(this,
            "No es posible reservar con estos datos:" + Environment.NewLine + Environment.NewLine + detalle,
            "Disponibilidad");

        lblMensajeEstado.Text = $"{disponibilidad.Conflictos.Count} conflicto(s) de disponibilidad.";
    }

    private async void btnGuardar_Click(object sender, EventArgs e)
    {
        var dto = ConstruirDto();

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.GuardarAsync(dto, ct),
            _cancelacion.Token,
            "guardado de la reserva",
            btnGuardar, btnValidar, btnConfirmar);

        if (resultado is null)
        {
            return;
        }

        _idReserva = resultado.IdReserva;
        _codigo = resultado.Codigo;
        _hayCambiosSinGuardar = false;

        lblCodigo.Text = resultado.Codigo;
        Text = $"Reserva {resultado.Codigo}";
        lblMensajeEstado.Text = resultado.Mensaje;

        ReservaModificada?.Invoke(this, EventArgs.Empty);

        // Se recarga desde la base para mostrar los importes DEFINITIVOS calculados por el
        // motor, no los que la pantalla estimo mientras se editaba.
        await CargarReservaAsync(resultado.IdReserva);
    }

    private async void btnAnalizarIA_Click(object sender, EventArgs e)
    {
        if (!_idReserva.HasValue)
        {
            ManejadorErrores.Advertir(this, "Guarde la reserva antes de solicitar el analisis con IA.");
            return;
        }

        if (_hayCambiosSinGuardar && !ManejadorErrores.Confirmar(this,
                "Hay cambios sin guardar. El analisis se hara sobre la version guardada.\n\nDesea continuar?"))
        {
            return;
        }

        // Token propio: permite cancelar el analisis sin afectar al resto de la pantalla.
        var token = _operacionIA.Reiniciar();

        prgOcupado.Visible = true;
        lblMensajeEstado.Text = $"Consultando el modelo {_servicios.ModeloIA}...";

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.AnalizarConIAAsync(_idReserva.Value, ct),
            token,
            "analisis con IA",
            btnAnalizarIA, btnConfirmar, btnGuardar);

        prgOcupado.Visible = false;

        if (resultado is null)
        {
            lblMensajeEstado.Text = "Analisis cancelado o no completado.";
            return;
        }

        if (resultado.Exitoso)
        {
            lblMensajeEstado.Text = $"Analisis completado. Nivel de riesgo: {resultado.Respuesta!.NivelRiesgo}.";
            FrmAnalisisIA.Mostrar(this, resultado, _codigo);
            return;
        }

        // Fallo controlado: la aplicacion sigue operativa y se explica la alternativa.
        lblMensajeEstado.Text = "El analisis no pudo completarse. El intento quedo auditado.";

        ManejadorErrores.Advertir(this,
            resultado.Error + Environment.NewLine + Environment.NewLine +
            "El intento quedo registrado en la auditoria. Puede reintentarlo o confirmar la " +
            "reserva escribiendo una justificacion de contingencia.",
            "Analisis con IA no disponible");
    }

    private async void btnConfirmar_Click(object sender, EventArgs e)
    {
        if (!_idReserva.HasValue)
        {
            ManejadorErrores.Advertir(this, "Guarde la reserva antes de confirmarla.");
            return;
        }

        if (_hayCambiosSinGuardar)
        {
            ManejadorErrores.Advertir(this,
                "Hay cambios sin guardar. Guarde la reserva antes de confirmarla.");
            return;
        }

        // Se comprueba si ya existe un analisis exitoso. Si no lo hay, se ofrece la
        // contingencia manual en lugar de bloquear al usuario sin alternativa.
        var analisis = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Auditoria.ObtenerUltimoAnalisisAsync(_idReserva.Value, ct),
            _cancelacion.Token,
            "consulta del analisis de IA",
            btnConfirmar);

        string? justificacion = null;

        if (analisis is null)
        {
            var continuar = ManejadorErrores.Confirmar(this,
                "Esta reserva no tiene un analisis de IA exitoso.\n\n" +
                "Puede ejecutarlo ahora o confirmar registrando una justificacion de contingencia.\n\n" +
                "Desea escribir la justificacion y continuar?",
                "Confirmar sin analisis de IA");

            if (!continuar)
            {
                return;
            }

            justificacion = FrmTextoRequerido.Pedir(this,
                "Justificacion de contingencia",
                "Explique por que se confirma la reserva sin un analisis de IA exitoso. " +
                "Este texto queda guardado en la reserva y en la auditoria.",
                longitudMinima: 20);

            if (justificacion is null)
            {
                return;
            }
        }
        else if (!ManejadorErrores.Confirmar(this,
                     $"Se confirmara la reserva {_codigo} y se enviara la notificacion al cliente.\n\n" +
                     "Desea continuar?",
                     "Confirmar reserva"))
        {
            return;
        }

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.ConfirmarAsync(_idReserva.Value, justificacion, ct),
            _cancelacion.Token,
            "confirmacion de la reserva",
            btnConfirmar, btnGuardar, btnCancelarReserva);

        if (resultado is null)
        {
            return;
        }

        InformarCambioDeEstado(resultado);
        await CargarReservaAsync(_idReserva.Value);
    }

    private async void btnCancelarReserva_Click(object sender, EventArgs e)
    {
        if (!_idReserva.HasValue)
        {
            return;
        }

        var motivo = FrmTextoRequerido.Pedir(this,
            "Cancelar reserva",
            $"Indique el motivo por el que se cancela la reserva {_codigo}. " +
            "El motivo queda auditado y se incluye en la notificacion al cliente.",
            longitudMinima: 20);

        if (motivo is null)
        {
            return;
        }

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.CancelarAsync(_idReserva.Value, motivo, ct),
            _cancelacion.Token,
            "cancelacion de la reserva",
            btnCancelarReserva, btnConfirmar, btnGuardar);

        if (resultado is null)
        {
            return;
        }

        InformarCambioDeEstado(resultado);
        await CargarReservaAsync(_idReserva.Value);
    }

    private async void btnFinalizar_Click(object sender, EventArgs e)
    {
        if (!_idReserva.HasValue)
        {
            return;
        }

        if (!ManejadorErrores.Confirmar(this,
                $"Se marcara la reserva {_codigo} como FINALIZADA.\n\n" +
                "Es un estado terminal: despues no admite ningun otro cambio.\n\nDesea continuar?",
                "Finalizar evento"))
        {
            return;
        }

        var resultado = await EjecucionUi.EjecutarAsync(
            this,
            ct => _servicios.Reservas.FinalizarAsync(_idReserva.Value, ct),
            _cancelacion.Token,
            "finalizacion de la reserva",
            btnFinalizar);

        if (resultado is null)
        {
            return;
        }

        InformarCambioDeEstado(resultado);
        await CargarReservaAsync(_idReserva.Value);
    }

    /// <summary>
    /// Informa el resultado de un cambio de estado distinguiendo dos cosas que NO son lo mismo:
    /// el cambio de estado (que ya ocurrio y es definitivo) y el correo (que pudo fallar).
    /// </summary>
    private void InformarCambioDeEstado(CambioEstadoResultado resultado)
    {
        ReservaModificada?.Invoke(this, EventArgs.Empty);

        if (resultado.Correo is null)
        {
            lblMensajeEstado.Text = resultado.Mensaje;
            ManejadorErrores.Informar(this, resultado.Mensaje);
            return;
        }

        if (resultado.Correo.Enviado)
        {
            lblMensajeEstado.Text = $"{resultado.Mensaje} Notificacion enviada a {resultado.Correo.Destinatario}.";

            ManejadorErrores.Informar(this,
                resultado.Mensaje + Environment.NewLine + Environment.NewLine +
                $"Se envio la notificacion a {resultado.Correo.Destinatario}.");

            return;
        }

        lblMensajeEstado.Text = $"{resultado.Mensaje} El correo no pudo enviarse.";

        ManejadorErrores.Advertir(this,
            resultado.Mensaje + Environment.NewLine + Environment.NewLine +
            "El cambio de estado se aplico correctamente, pero NO se pudo enviar la notificacion:" +
            Environment.NewLine + Environment.NewLine +
            resultado.Correo.Error + Environment.NewLine + Environment.NewLine +
            "El intento quedo auditado. Puede reintentar el envio desde " +
            "Auditoria > Correos y analisis de IA, sin que la reserva cambie de estado otra vez.",
            "Reserva actualizada, correo pendiente");
    }

    /// <summary>
    /// Muestra el historial de transiciones. Es la evidencia, desde la propia aplicacion, de que
    /// cada cambio de estado se registro una sola vez: un reenvio de correo no anade filas aqui.
    /// </summary>
    private void btnHistorial_Click(object sender, EventArgs e)
    {
        if (!_idReserva.HasValue)
        {
            ManejadorErrores.Advertir(this, "Guarde la reserva para consultar su historial de estados.");
            return;
        }

        FrmHistorialEstados.Mostrar(this, _servicios, _idReserva.Value, _codigo);
    }

    private void btnCerrar_Click(object sender, EventArgs e) => Close();

    private void FrmReservaEdicion_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_hayCambiosSinGuardar && _estado.PermiteEdicion())
        {
            var respuesta = MessageBox.Show(this,
                "La reserva tiene cambios sin guardar.\n\nDesea cerrar de todos modos?",
                "Cambios sin guardar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (respuesta != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        _operacionIA.Dispose();
        _cancelacion.Cancel();
        _cancelacion.Dispose();
    }
}
