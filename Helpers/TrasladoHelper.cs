using System.Globalization;
using System.Text.RegularExpressions;
using WebColegio.Models;

namespace WebColegio.Helpers
{
    /// <summary>
    /// Traslado del mismo ciclo y la misma ficha de alumno: no se da de baja
    /// ni se crea otra matrícula. Se actualiza el recinto y se cobran
    /// matrícula y mensualidad del colegio destino desde el mes de ingreso.
    /// Los pagos del origen no abonan el destino.
    /// </summary>
    public static class TrasladoHelper
    {
        public const string TipoEstudianteTraslado = "Traslado";
        public const string MarcaObservacion = "[TRASLADO";

        public static bool EsTraslado(TblMatricula? matricula, TblAlumno? alumno = null)
        {
            if (ContieneMarca(matricula?.TipoEstudiante) || ContieneMarca(matricula?.Observaciones))
                return true;
            return ContieneMarca(alumno?.TipoEstudiante) || ContieneMarca(alumno?.Observaciones);
        }

        public static int? LeerRecintoOrigen(string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                return null;
            var match = Regex.Match(observaciones, @"\[TRASLADO[^\]]*origen=(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            return int.TryParse(match.Groups[1].Value, out var id) && id > 0 ? id : null;
        }

        public static DateTime? LeerFechaTraslado(string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                return null;
            var fechaMatch = Regex.Match(observaciones, @"\[TRASLADO[^\]]*fecha=(\d{4}-\d{2}-\d{2})", RegexOptions.IgnoreCase);
            if (fechaMatch.Success
                && DateTime.TryParseExact(fechaMatch.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                return fecha.Date;
            return null;
        }

        public static int? LeerMesIngreso(string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                return null;
            var mesMatch = Regex.Match(observaciones, @"\[TRASLADO[^\]]*mes=(\d{1,2})", RegexOptions.IgnoreCase);
            if (mesMatch.Success && int.TryParse(mesMatch.Groups[1].Value, out var mes) && mes is >= 1 and <= 12)
                return mes;
            var fecha = LeerFechaTraslado(observaciones);
            return fecha?.Month;
        }

        public static string Anotar(
            string? observacionesActuales,
            int idRecintoOrigen,
            string nombreOrigen,
            DateTime fecha,
            int mesIngreso)
        {
            if (mesIngreso < 1 || mesIngreso > 12)
                mesIngreso = fecha.Month;

            string marca =
                $"{MarcaObservacion} origen={idRecintoOrigen} fecha={fecha:yyyy-MM-dd} mes={mesIngreso}] " +
                $"Desde {nombreOrigen}. Cobra matrícula y mensualidad del colegio destino desde el mes de ingreso.";

            var actual = observacionesActuales?.Trim() ?? "";
            if (actual.Contains(MarcaObservacion, StringComparison.OrdinalIgnoreCase))
            {
                actual = Regex.Replace(
                    actual,
                    @"\[TRASLADO[^\]]*\](?:\s*Desde .+?mes de ingreso\.)?",
                    "",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline).Trim();
            }

            return string.IsNullOrWhiteSpace(actual) ? marca : marca + " " + actual;
        }

        /// <summary>
        /// El mes ya cubierto en el colegio de origen no se reabre con la tarifa nueva.
        /// Un abono parcial no se considera mes pagado.
        /// </summary>
        public static bool MesPagadoSeRespeta(decimal pagado, decimal tarifaOrigen, decimal tarifaDestino)
        {
            if (pagado <= MensualidadSobranteHelper.Centavo)
                return false;
            bool cubreOrigen = tarifaOrigen > 0m
                && MensualidadSobranteHelper.MesCancelado(tarifaOrigen, pagado);
            return cubreOrigen || MensualidadSobranteHelper.MesCancelado(tarifaDestino, pagado);
        }

        private static bool ContieneMarca(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;
            var t = texto.ToLowerInvariant();
            return t.Contains("traslado", StringComparison.Ordinal)
                || t.Contains(MarcaObservacion.ToLowerInvariant(), StringComparison.Ordinal);
        }
    }
}
