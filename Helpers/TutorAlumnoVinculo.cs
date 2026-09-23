using WebColegio.Models;

namespace WebColegio.Helpers
{
    /// <summary>
    /// Vincula un usuario tutor con alumnos únicamente por cédula (Tbl_Alumnos.CedulaTutor = Tbl_Usuario.Cedula).
    /// No usar nombres: evita mezclar tutores distintos (p. ej. un padre con hijas de otro hogar).
    /// </summary>
    public static class TutorAlumnoVinculo
    {
        public static bool EsRolTutor(string? nombreRol, int idRol)
        {
            if (idRol == 4)
                return true;

            var n = nombreRol?.Trim() ?? string.Empty;
            return n.Equals("Tutor", StringComparison.OrdinalIgnoreCase)
                || n.Equals("Tutora", StringComparison.OrdinalIgnoreCase);
        }

        public static IReadOnlyList<TblAlumno> AlumnosDeTutor(IEnumerable<TblAlumno>? alumnos, string? cedulaUsuario)
        {
            if (alumnos == null || string.IsNullOrWhiteSpace(cedulaUsuario))
                return Array.Empty<TblAlumno>();

            var cedula = cedulaUsuario.Trim();
            return alumnos
                .Where(a => a.Activo != false
                    && !string.IsNullOrWhiteSpace(a.CedulaTutor)
                    && string.Equals(a.CedulaTutor.Trim(), cedula, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static string TextoOGuion(IEnumerable<string?> valores)
        {
            var partes = valores
                .Select(v => (v ?? string.Empty).Trim())
                .Where(v => v.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return partes.Count == 0 ? "—" : string.Join(" / ", partes);
        }
    }
}
