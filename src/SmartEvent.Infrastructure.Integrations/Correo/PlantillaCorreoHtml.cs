using System.Globalization;
using System.Net;
using System.Text;
using SmartEvent.Core.Entities;
using SmartEvent.Core.Enums;

namespace SmartEvent.Infrastructure.Integrations.Correo;

/// <summary>
/// Composicion del correo HTML de la reserva.
///
/// PUNTO CRITICO DE SEGURIDAD: todo valor que provenga de la base de datos pasa por
/// <see cref="Escapar"/> antes de insertarse en la plantilla. Un cliente llamado
/// &lt;script&gt;... o una observacion con etiquetas HTML no puede alterar la estructura del
/// mensaje ni inyectar contenido en el lector de correo del destinatario. Lo unico que se
/// concatena sin escapar es el HTML de la propia plantilla, escrito aqui.
///
/// La plantilla usa estilos en linea porque los clientes de correo (Outlook, Gmail) ignoran
/// o recortan las hojas de estilo externas y muchas reglas de la etiqueta style.
/// </summary>
internal static class PlantillaCorreoHtml
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-EC");

    private const string ColorPrincipal = "#1F3864";
    private const string ColorBorde = "#D0D7E5";
    private const string ColorFondoSuave = "#F2F5FA";

    public static string ComponerAsunto(Reserva reserva, TipoNotificacion tipo) => tipo switch
    {
        TipoNotificacion.Cancelacion => $"Cancelacion de su reserva {reserva.Codigo} - SmartEvent",
        _ => $"Confirmacion de su reserva {reserva.Codigo} - SmartEvent"
    };

    public static string ComponerHtml(Reserva reserva, TipoNotificacion tipo)
    {
        var esCancelacion = tipo == TipoNotificacion.Cancelacion;

        var titulo = esCancelacion ? "Reserva cancelada" : "Reserva confirmada";
        var colorEstado = esCancelacion ? "#B3261E" : "#1B7F4B";

        var introduccion = esCancelacion
            ? "Le informamos que la siguiente reserva ha sido cancelada. Si considera que se trata de un error, responda a este mensaje."
            : "Nos complace confirmar los detalles de su reserva. Revise la informacion y comuniquenos cualquier ajuste.";

        var html = new StringBuilder(4096);

        html.Append("<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\">")
            .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">")
            .Append("<title>").Append(Escapar(titulo)).Append("</title></head>")
            .Append("<body style=\"margin:0;padding:0;background-color:#EEF1F6;")
            .Append("font-family:Segoe UI,Arial,Helvetica,sans-serif;color:#20242B;\">")
            .Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" ")
            .Append("style=\"background-color:#EEF1F6;padding:24px 12px;\"><tr><td align=\"center\">")
            .Append("<table role=\"presentation\" width=\"640\" cellpadding=\"0\" cellspacing=\"0\" ")
            .Append("style=\"width:640px;max-width:100%;background-color:#FFFFFF;border:1px solid ")
            .Append(ColorBorde).Append(";border-radius:6px;overflow:hidden;\">");

        // Encabezado
        html.Append("<tr><td style=\"background-color:").Append(ColorPrincipal)
            .Append(";padding:20px 24px;\">")
            .Append("<div style=\"color:#FFFFFF;font-size:20px;font-weight:600;\">SmartEvent</div>")
            .Append("<div style=\"color:#C8D3E8;font-size:13px;margin-top:2px;\">")
            .Append("Gestion de reservas de salones y recursos</div></td></tr>");

        // Titulo y estado
        html.Append("<tr><td style=\"padding:24px 24px 8px 24px;\">")
            .Append("<h1 style=\"margin:0 0 4px 0;font-size:19px;color:").Append(ColorPrincipal).Append(";\">")
            .Append(Escapar(titulo)).Append("</h1>")
            .Append("<span style=\"display:inline-block;padding:3px 10px;border-radius:12px;font-size:12px;")
            .Append("font-weight:600;color:#FFFFFF;background-color:").Append(colorEstado).Append(";\">")
            .Append(Escapar(reserva.Estado.ATextoUsuario().ToUpperInvariant())).Append("</span>")
            .Append("<p style=\"margin:14px 0 0 0;font-size:14px;line-height:1.5;\">Estimado/a ")
            .Append("<strong>").Append(Escapar(reserva.Cliente)).Append("</strong>,<br>")
            .Append(Escapar(introduccion)).Append("</p></td></tr>");

        // Datos de la reserva
        html.Append("<tr><td style=\"padding:16px 24px 0 24px;\">")
            .Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" ")
            .Append("style=\"border-collapse:collapse;font-size:13px;\">");

        AgregarFilaDato(html, "Codigo de reserva", reserva.Codigo);
        AgregarFilaDato(html, "Salon", reserva.Salon);
        AgregarFilaDato(html, "Fecha del evento", reserva.FechaEvento.ToString("dddd, dd 'de' MMMM 'de' yyyy", Cultura));
        AgregarFilaDato(html, "Horario", $"{reserva.HoraInicio:HH\\:mm} a {reserva.HoraFin:HH\\:mm} ({reserva.Duracion.TotalHours:0.#} horas)");
        AgregarFilaDato(html, "Numero de invitados", reserva.NumeroInvitados.ToString(Cultura));

        if (!string.IsNullOrWhiteSpace(reserva.Observacion))
        {
            AgregarFilaDato(html, "Observacion", reserva.Observacion);
        }

        if (esCancelacion && !string.IsNullOrWhiteSpace(reserva.MotivoCancelacion))
        {
            AgregarFilaDato(html, "Motivo de la cancelacion", reserva.MotivoCancelacion);
        }

        html.Append("</table></td></tr>");

        // Detalle de recursos
        html.Append("<tr><td style=\"padding:20px 24px 0 24px;\">")
            .Append("<h2 style=\"margin:0 0 8px 0;font-size:15px;color:").Append(ColorPrincipal)
            .Append(";\">Recursos y servicios contratados</h2>")
            .Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" ")
            .Append("style=\"border-collapse:collapse;font-size:13px;border:1px solid ").Append(ColorBorde).Append(";\">")
            .Append("<thead><tr style=\"background-color:").Append(ColorFondoSuave).Append(";\">")
            .Append(Encabezado("Recurso", "left"))
            .Append(Encabezado("Cant.", "center"))
            .Append(Encabezado("P. unitario", "right"))
            .Append(Encabezado("Dscto.", "right"))
            .Append(Encabezado("Subtotal", "right"))
            .Append("</tr></thead><tbody>");

        foreach (var detalle in reserva.Detalles)
        {
            html.Append("<tr>")
                .Append(Celda(Escapar(detalle.Recurso), "left"))
                .Append(Celda(detalle.Cantidad.ToString(Cultura), "center"))
                .Append(Celda(Moneda(detalle.PrecioUnitario), "right"))
                .Append(Celda(detalle.PorcentajeDescuento > 0
                    ? detalle.PorcentajeDescuento.ToString("0.##", Cultura) + " %"
                    : "-", "right"))
                .Append(Celda(Moneda(detalle.SubtotalLinea), "right"))
                .Append("</tr>");
        }

        html.Append("</tbody></table></td></tr>");

        // Totales
        html.Append("<tr><td style=\"padding:16px 24px 0 24px;\">")
            .Append("<table role=\"presentation\" align=\"right\" cellpadding=\"0\" cellspacing=\"0\" ")
            .Append("style=\"border-collapse:collapse;font-size:13px;min-width:280px;\">");

        AgregarFilaTotal(html, "Subtotal", Moneda(reserva.Subtotal), false);

        if (reserva.Descuento > 0)
        {
            AgregarFilaTotal(html, "Descuento", "- " + Moneda(reserva.Descuento), false);
            AgregarFilaTotal(html, "Base imponible", Moneda(reserva.BaseNeta), false);
        }

        AgregarFilaTotal(html, "Impuesto (15 %)", Moneda(reserva.Impuesto), false);
        AgregarFilaTotal(html, "TOTAL", Moneda(reserva.Total), true);

        html.Append("</table></td></tr>");

        // Pie
        html.Append("<tr><td style=\"padding:28px 24px 24px 24px;\">")
            .Append("<p style=\"margin:0;font-size:12px;color:#5B6472;line-height:1.5;border-top:1px solid ")
            .Append(ColorBorde).Append(";padding-top:14px;\">")
            .Append("Este mensaje fue generado automaticamente por SmartEvent el ")
            .Append(Escapar(DateTime.Now.ToString("dd/MM/yyyy 'a las' HH:mm", Cultura)))
            .Append(". Si necesita realizar algun cambio, responda a este correo indicando el codigo ")
            .Append("<strong>").Append(Escapar(reserva.Codigo)).Append("</strong>.")
            .Append("</p></td></tr>");

        html.Append("</table></td></tr></table></body></html>");

        return html.ToString();
    }

    /// <summary>
    /// Version en texto plano del mismo contenido. Se envia junto al HTML para los clientes de
    /// correo que no lo muestran y porque mejora la puntuacion antispam del mensaje.
    /// </summary>
    public static string ComponerTexto(Reserva reserva, TipoNotificacion tipo)
    {
        var texto = new StringBuilder();

        texto.AppendLine(tipo == TipoNotificacion.Cancelacion ? "RESERVA CANCELADA" : "RESERVA CONFIRMADA")
             .AppendLine(new string('=', 40))
             .AppendLine($"Codigo      : {reserva.Codigo}")
             .AppendLine($"Cliente     : {reserva.Cliente}")
             .AppendLine($"Salon       : {reserva.Salon}")
             .AppendLine($"Fecha       : {reserva.FechaEvento:dd/MM/yyyy}")
             .AppendLine($"Horario     : {reserva.HoraInicio:HH\\:mm} a {reserva.HoraFin:HH\\:mm}")
             .AppendLine($"Invitados   : {reserva.NumeroInvitados}")
             .AppendLine($"Estado      : {reserva.Estado.ATextoUsuario()}")
             .AppendLine();

        if (tipo == TipoNotificacion.Cancelacion && !string.IsNullOrWhiteSpace(reserva.MotivoCancelacion))
        {
            texto.AppendLine($"Motivo      : {reserva.MotivoCancelacion}").AppendLine();
        }

        texto.AppendLine("RECURSOS Y SERVICIOS").AppendLine(new string('-', 40));

        foreach (var detalle in reserva.Detalles)
        {
            texto.AppendLine($"{detalle.Cantidad,4} x {detalle.Recurso,-32} {Moneda(detalle.SubtotalLinea),12}");
        }

        texto.AppendLine(new string('-', 40))
             .AppendLine($"{"Subtotal",-38} {Moneda(reserva.Subtotal),12}");

        if (reserva.Descuento > 0)
        {
            texto.AppendLine($"{"Descuento",-38} {"- " + Moneda(reserva.Descuento),12}");
        }

        texto.AppendLine($"{"Impuesto (15 %)",-38} {Moneda(reserva.Impuesto),12}")
             .AppendLine($"{"TOTAL",-38} {Moneda(reserva.Total),12}")
             .AppendLine()
             .AppendLine("Mensaje generado automaticamente por SmartEvent.");

        return texto.ToString();
    }

    // -------------------------------------------------------------------------------- apoyo

    /// <summary>
    /// Codificacion HTML de cualquier valor de negocio. Es la unica puerta por la que entran
    /// datos externos a la plantilla.
    /// </summary>
    private static string Escapar(string? valor) => WebUtility.HtmlEncode(valor ?? string.Empty);

    private static string Moneda(decimal valor) => valor.ToString("C2", Cultura);

    private static string Encabezado(string texto, string alineacion) =>
        $"<th style=\"padding:8px 10px;text-align:{alineacion};border-bottom:1px solid {ColorBorde};" +
        $"font-size:12px;color:{ColorPrincipal};\">{Escapar(texto)}</th>";

    private static string Celda(string contenidoYaEscapado, string alineacion) =>
        $"<td style=\"padding:7px 10px;text-align:{alineacion};border-bottom:1px solid #EDF0F5;\">" +
        $"{contenidoYaEscapado}</td>";

    private static void AgregarFilaDato(StringBuilder html, string etiqueta, string valor)
    {
        html.Append("<tr><td style=\"padding:5px 0;width:190px;color:#5B6472;vertical-align:top;\">")
            .Append(Escapar(etiqueta)).Append("</td>")
            .Append("<td style=\"padding:5px 0;font-weight:600;\">")
            .Append(Escapar(valor)).Append("</td></tr>");
    }

    private static void AgregarFilaTotal(StringBuilder html, string etiqueta, string valor, bool destacado)
    {
        var estiloEtiqueta = destacado
            ? $"padding:9px 12px 9px 0;text-align:right;font-weight:700;color:{ColorPrincipal};border-top:2px solid {ColorBorde};"
            : "padding:4px 12px 4px 0;text-align:right;color:#5B6472;";

        var estiloValor = destacado
            ? $"padding:9px 0;text-align:right;font-weight:700;font-size:15px;color:{ColorPrincipal};border-top:2px solid {ColorBorde};"
            : "padding:4px 0;text-align:right;font-weight:600;";

        html.Append("<tr><td style=\"").Append(estiloEtiqueta).Append("\">").Append(Escapar(etiqueta)).Append("</td>")
            .Append("<td style=\"").Append(estiloValor).Append("\">").Append(Escapar(valor)).Append("</td></tr>");
    }
}
