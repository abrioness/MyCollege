using System.Net.Mail;
using WebColegio.Models;

namespace WebColegio.Helpers
{
    public static class EmailContactoHelper
    {
        public static (string? Correo, string? Origen, string? Nombre) Resolver(
            TblAlumno alumno,
            IEnumerable<TblUsuarios>? usuarios)
        {
            if (EsValido(alumno.Correo))
                return (alumno.Correo!.Trim(), "Ficha del alumno", alumno.NombreTutor);

            if (!string.IsNullOrWhiteSpace(alumno.CedulaTutor) && usuarios != null)
            {
                var tutor = usuarios.FirstOrDefault(u =>
                    u.Activo
                    && !string.IsNullOrWhiteSpace(u.Cedula)
                    && string.Equals(u.Cedula.Trim(), alumno.CedulaTutor.Trim(), StringComparison.OrdinalIgnoreCase)
                    && EsValido(u.Correo));
                if (tutor != null)
                    return (tutor.Correo!.Trim(), "Usuario tutor", tutor.NombreCompleto);
            }

            return (null, null, alumno.NombreTutor);
        }

        public static bool EsValido(string? correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;
            try
            {
                var addr = new MailAddress(correo.Trim());
                return addr.Address.Contains('@');
            }
            catch
            {
                return false;
            }
        }
    }
}
