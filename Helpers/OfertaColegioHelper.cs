using System.Globalization;
using System.Text;
using WebColegio.Models;
using WebColegio;

namespace WebColegio.Helpers
{
    /// <summary>
    /// San Miguel Arcángel solo ofrece preescolar y primaria; no tiene secundaria.
    /// </summary>
    public static class OfertaColegioHelper
    {
        public static bool EsSanMiguel(string? nombreRecinto, int idRecinto = 0)
        {
            var n = Normalizar(nombreRecinto);
            if (n.Contains("miguel"))
                return true;
            if (n.Contains("francisco") || n.Contains("javier"))
                return false;
            return idRecinto == 2;
        }

        public static bool EsSecundariaModalidad(string? nombreModalidad)
            => Normalizar(nombreModalidad).Contains("secundaria");

        public static bool EsGradoSecundaria(string? nombreGrado)
        {
            var n = Normalizar(nombreGrado);
            if (n.Contains("secundaria"))
                return true;
            var numero = NotasDisplayHelper.ObtenerNumeroGrado(nombreGrado);
            return numero is >= 7 and <= 11;
        }

        public static bool RecintoOfreceModalidad(
            IEnumerable<Recintos>? recintos,
            IEnumerable<Modalidades>? modalidades,
            int idRecinto,
            int idModalidad)
        {
            if (idModalidad <= 0)
                return true;
            var recinto = recintos?.FirstOrDefault(r => r.IdRecinto == idRecinto);
            var modalidad = modalidades?.FirstOrDefault(m => m.IdModalidad == idModalidad);
            return RecintoOfreceModalidad(recinto?.Recinto, idRecinto, modalidad?.Modalidad);
        }

        public static bool RecintoOfreceModalidad(string? nombreRecinto, int idRecinto, string? nombreModalidad)
        {
            if (EsSanMiguel(nombreRecinto, idRecinto) && EsSecundariaModalidad(nombreModalidad))
                return false;
            return true;
        }

        public static bool RecintoOfreceMensualidad(
            IEnumerable<Recintos>? recintos,
            IEnumerable<Modalidades>? modalidades,
            IEnumerable<Grados>? grados,
            int idRecinto,
            int idModalidad,
            int idGrado)
        {
            if (!RecintoOfreceModalidad(recintos, modalidades, idRecinto, idModalidad))
                return false;
            if (!EsSanMiguel(recintos?.FirstOrDefault(r => r.IdRecinto == idRecinto)?.Recinto, idRecinto))
                return true;
            var grado = grados?.FirstOrDefault(g => g.IdGrado == idGrado);
            return !EsGradoSecundaria(grado?.NombreGrado);
        }

        public static string MensajeSinSecundariaSanMiguel()
            => "El Centro Educativo Parroquial San Miguel Arcángel no cuenta con secundaria. No se registra tarifa para esa modalidad.";

        private static string Normalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;
            var n = texto.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(n.Length);
            foreach (var ch in n)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.ToLowerInvariant(ch));
            }
            return sb.ToString();
        }
    }
}
