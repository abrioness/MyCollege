using WebColegio.Models;

namespace WebColegio.Helpers
{
    public static class CostoNivelHelper
    {
        public static bool EsValido(int idGrado, IEnumerable<Grados>? grados)
        {
            if (idGrado <= 0)
                return false;
            var g = grados?.FirstOrDefault(x => x.IdGrado == idGrado);
            if (g == null)
                return false;
            if (!g.Activo)
                return false;
            return !EsNombreInvalido(g.NombreGrado);
        }

        public static bool EsNombreInvalido(string? nombre)
        {
            var n = (nombre ?? "").Trim();
            if (n.Length == 0)
                return true;
            var t = n.ToLowerInvariant();
            return t is "ninguno" or "ninguna" or "n/a" or "na" or "-" or "0"
                || t.Contains("ningun");
        }

        public static string NombreParaLista(int idGrado, IEnumerable<Grados>? grados)
        {
            var g = grados?.FirstOrDefault(x => x.IdGrado == idGrado);
            if (g == null || string.IsNullOrWhiteSpace(g.NombreGrado))
                return $"Código {idGrado} (no existe en catálogo)";
            if (EsNombreInvalido(g.NombreGrado))
                return g.NombreGrado.Trim();
            if (!g.Activo)
                return g.NombreGrado.Trim() + " (inactivo)";
            return g.NombreGrado.Trim();
        }
    }
}
