using System.Text.RegularExpressions;
using WebColegio.Models;

namespace WebColegio.Helpers
{
    public static class WhatsAppTelefonoHelper
    {
        public static (string? NumeroE164, string? Origen, string? NombreContacto) ResolverContacto(
            TblAlumno alumno,
            string codigoPais = "505")
        {
            var candidatos = new (string? Numero, string Origen, string? Nombre)[]
            {
                (alumno.ContactoTutor, "Tutor", alumno.NombreTutor),
                (alumno.TelefonoMadre, "Madre", alumno.NombreMadre),
                (alumno.TelefonoPadre, "Padre", alumno.NombrePadre),
                (alumno.Telefono, "Alumno", $"{alumno.Nombre} {alumno.Apellido}".Trim())
            };

            foreach (var c in candidatos)
            {
                var e164 = Normalizar(c.Numero, codigoPais);
                if (e164 != null)
                    return (e164, c.Origen, string.IsNullOrWhiteSpace(c.Nombre) ? c.Origen : c.Nombre.Trim());
            }

            return (null, null, alumno.NombreTutor);
        }

        public static string? Normalizar(string? crudo, string codigoPais = "505")
        {
            if (string.IsNullOrWhiteSpace(crudo))
                return null;

            var digitos = Regex.Replace(crudo, @"\D", "");
            if (digitos.Length < 8)
                return null;

            if (digitos.StartsWith("00"))
                digitos = digitos[2..];

            if (digitos.Length == 8)
                digitos = codigoPais + digitos;

            if (digitos.Length < 11 || digitos.Length > 15)
                return null;

            return digitos;
        }
    }
}
