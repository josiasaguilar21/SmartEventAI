namespace SmartEvent.UI.Formularios;

/// <summary>
/// Dialogo reutilizable para pedir un texto con longitud minima obligatoria.
///
/// Se usa en los dos puntos donde la regla de negocio exige una explicacion escrita:
///   - Motivo de cancelacion (minimo 20 caracteres).
///   - Justificacion de contingencia al confirmar sin analisis de IA (minimo 20 caracteres).
///
/// El boton Aceptar permanece deshabilitado hasta alcanzar el minimo y un contador muestra
/// cuanto falta. Asi el usuario sabe por que no puede continuar, en lugar de descubrirlo al
/// recibir el rechazo del servidor.
/// </summary>
public partial class FrmTextoRequerido : Form
{
    private readonly int _longitudMinima;

    private FrmTextoRequerido(string titulo, string instruccion, int longitudMinima, string? textoInicial)
    {
        _longitudMinima = longitudMinima;

        InitializeComponent();

        Text = titulo;
        lblInstruccion.Text = instruccion;
        txtTexto.Text = textoInicial ?? string.Empty;

        ActualizarContador();
    }

    /// <summary>Texto escrito por el usuario, ya recortado.</summary>
    public string Texto => txtTexto.Text.Trim();

    /// <summary>
    /// Muestra el dialogo y devuelve el texto, o null si el usuario cancelo.
    /// </summary>
    public static string? Pedir(IWin32Window propietario, string titulo, string instruccion,
                                int longitudMinima = 20, string? textoInicial = null)
    {
        using var dialogo = new FrmTextoRequerido(titulo, instruccion, longitudMinima, textoInicial);

        return dialogo.ShowDialog(propietario) == DialogResult.OK ? dialogo.Texto : null;
    }

    private void txtTexto_TextChanged(object sender, EventArgs e) => ActualizarContador();

    private void ActualizarContador()
    {
        var longitud = txtTexto.Text.Trim().Length;
        var suficiente = longitud >= _longitudMinima;

        btnAceptar.Enabled = suficiente;

        lblContador.Text = suficiente
            ? $"{longitud} caracteres."
            : $"{longitud} de {_longitudMinima} caracteres minimos. Faltan {_longitudMinima - longitud}.";

        lblContador.ForeColor = suficiente
            ? SystemColors.GrayText
            : Color.FromArgb(179, 38, 30);
    }

    private void btnAceptar_Click(object sender, EventArgs e)
    {
        if (txtTexto.Text.Trim().Length < _longitudMinima)
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
