using System.Globalization;
using System.Text;
using WebColegio.Models.ViewModel;

namespace WebColegio.Helpers
{
    public static class WhatsAppMensajeBuilder
    {
        public static string Construir(
            string nombreColegio,
            string? nombreContacto,
            WhatsAppAvisoDestino destino)
        {
            var cultura = new CultureInfo("es-NI");
            var sb = new StringBuilder();
            var saludo = string.IsNullOrWhiteSpace(nombreContacto) ? "estimado(a) familiar" : nombreContacto.Trim();

            sb.Append("Buenos días, ").Append(saludo).AppendLine(".");
            sb.AppendLine();
            sb.Append("Le saluda el ").Append(nombreColegio).AppendLine(".");
            sb.Append("El estudiante *").Append(destino.NombreAlumno).Append('*');
            if (!string.IsNullOrWhiteSpace(destino.Grado))
                sb.Append(" (").Append(destino.Grado).Append(')');
            sb.AppendLine(" presenta saldo pendiente:");
            sb.AppendLine();

            if (destino.SaldoMatricula > 0.01m)
                sb.Append("• Matrícula: C$ ").AppendLine(destino.SaldoMatricula.ToString("N2", cultura));

            if (destino.SaldoMensualidades > 0.01m || !string.IsNullOrWhiteSpace(destino.MesesPendientes))
            {
                sb.Append("• Mensualidades");
                if (!string.IsNullOrWhiteSpace(destino.MesesPendientes))
                    sb.Append(" (").Append(destino.MesesPendientes).Append(')');
                sb.Append(": C$ ").AppendLine(destino.SaldoMensualidades.ToString("N2", cultura));
            }

            sb.Append("• *Total a cancelar: C$ ").Append(destino.TotalPendiente.ToString("N2", cultura)).AppendLine("*");
            sb.AppendLine();
            sb.Append("Por favor acercarse a caja para regularizar. Gracias.");
            return sb.ToString();
        }
    }
}
