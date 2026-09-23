using WebColegio.Models;

namespace WebColegio.Helpers
{
    public enum TipoCorrelativoRecibo
    {
        Mensualidad = 1,
        Caja = 2,
        Egreso = 3,
        Arqueo = 4
    }

    /// <summary>
    /// Cada colegio tiene serie y rango propios para identificar el recibo de un vistazo.
    /// San Francisco: serie F (sigue el correlativo histórico A).
    /// San Miguel: serie M, números 5xxxx / 6xxxx / 7xxxx / 8xxxx.
    /// </summary>
    public static class ReciboNumeracionHelper
    {
        public const string SerieFrancisco = "F";
        public const string SerieMiguel = "M";
        public const string SerieHistorica = "A";

        public static string SerieDe(Recintos? recinto)
            => SerieDe(recinto?.Recinto, recinto?.IdRecinto);

        public static string SerieDe(string? nombreRecinto, int? idRecinto = null)
        {
            var n = (nombreRecinto ?? "").Trim().ToLowerInvariant();
            if (n.Contains("miguel"))
                return SerieMiguel;
            if (n.Contains("francisco") || n.Contains("javier"))
                return SerieFrancisco;
            return SerieFrancisco;
        }

        public static string EtiquetaColegio(string? serie)
            => EsMiguel(serie) ? "San Miguel Arcángel" : "San Francisco Javier";

        public static bool EsMiguel(string? serie)
            => string.Equals((serie ?? "").Trim(), SerieMiguel, StringComparison.OrdinalIgnoreCase);

        public static int Piso(string serie, TipoCorrelativoRecibo tipo)
        {
            bool miguel = EsMiguel(serie);
            return tipo switch
            {
                TipoCorrelativoRecibo.Mensualidad => miguel ? 50001 : 10001,
                TipoCorrelativoRecibo.Caja => miguel ? 60001 : 20001,
                TipoCorrelativoRecibo.Egreso => miguel ? 70001 : 30001,
                TipoCorrelativoRecibo.Arqueo => miguel ? 80001 : 40001,
                _ => miguel ? 50001 : 10001
            };
        }

        public static HashSet<string> SeriesParaMax(string serie)
        {
            if (EsMiguel(serie))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { SerieMiguel };

            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SerieFrancisco,
                SerieHistorica
            };
        }

        public static int Siguiente(IEnumerable<int> existentes, int piso)
        {
            int max = existentes.Where(n => n > 0).DefaultIfEmpty(0).Max();
            return Math.Max(max + 1, piso);
        }

        public static (string Serie, int Numero) Resolver(
            Recintos? recinto,
            IEnumerable<(string? Serie, int Numero)> existentes,
            TipoCorrelativoRecibo tipo)
        {
            var serie = SerieDe(recinto);
            var set = SeriesParaMax(serie);
            var nums = existentes
                .Where(x => set.Contains((x.Serie ?? SerieHistorica).Trim()))
                .Select(x => x.Numero);
            return (serie, Siguiente(nums, Piso(serie, tipo)));
        }

        public static string Formato(string? serie, int? numero)
        {
            var s = string.IsNullOrWhiteSpace(serie) ? SerieHistorica : serie.Trim();
            return $"{s}-{numero ?? 0}";
        }
    }
}
