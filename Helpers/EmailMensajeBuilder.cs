using System.Globalization;
using System.Net;
using System.Text;
using WebColegio.Models.ViewModel;

namespace WebColegio.Helpers
{
    public static class EmailMensajeBuilder
    {
        public static string Asunto(string nombreColegio, WhatsAppAvisoDestino destino)
        {
            var alumno = string.IsNullOrWhiteSpace(destino.NombreAlumno) ? "estudiante" : destino.NombreAlumno.Trim();
            return $"{nombreColegio}: aviso de saldo pendiente — {alumno}";
        }

        public static string Html(string nombreColegio, WhatsAppAvisoDestino destino)
        {
            var cultura = new CultureInfo("es-NI");
            var saludo = string.IsNullOrWhiteSpace(destino.NombreContacto)
                ? "estimado(a) familiar"
                : WebUtility.HtmlEncode(destino.NombreContacto.Trim());
            var colegio = WebUtility.HtmlEncode(nombreColegio);
            var alumno = WebUtility.HtmlEncode(destino.NombreAlumno);
            var grado = string.IsNullOrWhiteSpace(destino.Grado)
                ? ""
                : " (" + WebUtility.HtmlEncode(destino.Grado) + ")";

            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:15px;color:#222;line-height:1.5\">");
            sb.Append("<p>Buenos días, ").Append(saludo).Append(".</p>");
            sb.Append("<p>Le saluda el <strong>").Append(colegio).Append("</strong>.</p>");
            sb.Append("<p>El estudiante <strong>").Append(alumno).Append("</strong>").Append(grado);
            sb.Append(" presenta saldo pendiente:</p><ul>");

            if (destino.SaldoMatricula > 0.01m)
                sb.Append("<li>Matrícula: C$ ").Append(destino.SaldoMatricula.ToString("N2", cultura)).Append("</li>");

            if (destino.SaldoMensualidades > 0.01m || !string.IsNullOrWhiteSpace(destino.MesesPendientes))
            {
                sb.Append("<li>Mensualidades");
                if (!string.IsNullOrWhiteSpace(destino.MesesPendientes))
                    sb.Append(" (").Append(WebUtility.HtmlEncode(destino.MesesPendientes)).Append(')');
                sb.Append(": C$ ").Append(destino.SaldoMensualidades.ToString("N2", cultura)).Append("</li>");
            }

            sb.Append("<li><strong>Total a cancelar: C$ ")
                .Append(destino.TotalPendiente.ToString("N2", cultura))
                .Append("</strong></li></ul>");
            sb.Append("<p>Por favor acercarse a caja para regularizar. Gracias.</p>");
            sb.Append("<p style=\"font-size:12px;color:#666;margin-top:24px\">Este correo se envía de forma automática. No responda a este mensaje; la bandeja no se revisa.</p>");
            sb.Append("</div>");
            return sb.ToString();
        }
    }
}
