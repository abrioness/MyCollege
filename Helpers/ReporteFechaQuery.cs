using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace WebColegio.Helpers
{
    /// <summary>
    /// Filtros de reportes por rango de fechas: parseo robusto desde query string y comparación por día calendario.
    /// </summary>
    public static class ReporteFechaQuery
    {
        private static DateTime? ParseFechaOpcional(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var s = value.Trim();
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d;
            if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out d))
                return d;
            return null;
        }

        /// <summary>
        /// Si el model binding devolvió null pero el navegador envió fechas (p. ej. type=date en formato ISO), las parsea aquí.
        /// </summary>
        public static (DateTime? Inicio, DateTime? Fin) ResolverRango(HttpRequest request, DateTime? fechainicio, DateTime? fechafin)
        {
            var ini = fechainicio ?? ParseFechaOpcional(request.Query["fechainicio"].FirstOrDefault());
            var fin = fechafin ?? ParseFechaOpcional(request.Query["fechafin"].FirstOrDefault());
            return (ini, fin);
        }

        /// <summary>Rango con nombres de parámetro personalizados (p. ej. desde/hasta en movimientos de inventario).</summary>
        public static (DateTime? Inicio, DateTime? Fin) ResolverRangoPorClaves(
            HttpRequest request,
            DateTime? inicio,
            DateTime? fin,
            string claveInicio,
            string claveFin)
        {
            var ini = inicio ?? ParseFechaOpcional(request.Query[claveInicio].FirstOrDefault());
            var f = fin ?? ParseFechaOpcional(request.Query[claveFin].FirstOrDefault());
            return (ini, f);
        }
    }
}
