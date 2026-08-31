using System.Runtime.InteropServices;
using SmartEvent.Core.Dtos;
using SmartEvent.Core.Enums;

namespace SmartEvent.UI.Formularios;

/// <summary>
/// Presentacion del analisis devuelto por el modelo.
///
/// Es una pantalla de SOLO LECTURA a proposito: muestra el nivel de riesgo, el resumen, las
/// alertas, las recomendaciones y el borrador de correo, y no ofrece ninguna accion que
/// modifique la reserva. El unico boton que hace algo copia el borrador al portapapeles para
/// que la persona lo revise y decida.
///
/// Esa limitacion es intencional y es la traduccion visual de la regla "la IA solo asesora".
/// </summary>
public partial class FrmAnalisisIA : Form
{
    private FrmAnalisisIA(AnalisisIAResultado resultado, string codigoReserva)
    {
        InitializeComponent();

        Text = $"Analisis con IA - Reserva {codigoReserva}";

        var respuesta = resultado.Respuesta!;

        lblNivelRiesgo.Text = $"Nivel de riesgo: {respuesta.NivelRiesgoEnum.ToString().ToUpperInvariant()}";
        lblNivelRiesgo.ForeColor = ColorDelRiesgo(respuesta.NivelRiesgoEnum);

        lblResumen.Text = respuesta.Resumen;

        // Se usan cuadros de texto con ajuste de linea y no listas: una recomendacion larga
        // quedaria cortada por la derecha y obligaria a desplazarse para leerla entera.
        if (respuesta.Alertas.Count == 0)
        {
            txtAlertas.Text = "Sin alertas: no se detectaron riesgos destacables.";
            txtAlertas.ForeColor = SystemColors.GrayText;
        }
        else
        {
            txtAlertas.Text = Numerar(respuesta.Alertas);
        }

        txtRecomendaciones.Text = Numerar(respuesta.Recomendaciones);

        txtCorreoSugerido.Text = respuesta.CorreoSugerido.Replace("\n", Environment.NewLine);

        var tokens = resultado.TokensEntrada.HasValue || resultado.TokensSalida.HasValue
            ? $" | Tokens: {resultado.TokensEntrada?.ToString() ?? "n/d"} entrada / " +
              $"{resultado.TokensSalida?.ToString() ?? "n/d"} salida"
            : string.Empty;

        lblMetadatos.Text = $"Modelo: {resultado.Modelo} | Prompt: {resultado.PromptVersion}{tokens}";
    }

    /// <summary>Muestra el resultado. Solo debe invocarse con un analisis exitoso.</summary>
    public static void Mostrar(IWin32Window propietario, AnalisisIAResultado resultado, string codigoReserva)
    {
        ArgumentNullException.ThrowIfNull(resultado);

        if (!resultado.Exitoso || resultado.Respuesta is null)
        {
            return;
        }

        using var formulario = new FrmAnalisisIA(resultado, codigoReserva);
        formulario.ShowDialog(propietario);
    }

    /// <summary>Numera los elementos y los separa con una linea en blanco para que se distingan.</summary>
    private static string Numerar(IReadOnlyList<string> elementos) =>
        string.Join(Environment.NewLine + Environment.NewLine,
            elementos.Select((texto, indice) => $"{indice + 1}. {texto}"));

    private static Color ColorDelRiesgo(NivelRiesgo nivel) => nivel switch
    {
        NivelRiesgo.Alto => Color.FromArgb(179, 38, 30),
        NivelRiesgo.Medio => Color.FromArgb(150, 90, 0),
        _ => Color.FromArgb(27, 127, 75)
    };

    private void btnCopiar_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCorreoSugerido.Text))
        {
            return;
        }

        try
        {
            Clipboard.SetText(txtCorreoSugerido.Text);
            btnCopiar.Text = "Copiado";
        }
        catch (ExternalException)
        {
            // El portapapeles puede estar bloqueado por otra aplicacion. No es un fallo que
            // deba interrumpir nada: el texto sigue visible y seleccionable en pantalla.
            MessageBox.Show(this,
                "No fue posible acceder al portapapeles en este momento. " +
                "Puede seleccionar el texto y copiarlo manualmente.",
                "Copiar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
