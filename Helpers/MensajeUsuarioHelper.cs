using System.Text.RegularExpressions;

namespace WebColegio.Helpers
{
    /// <summary>
    /// En producción oculta textos de diagnóstico (tablas, SQL, API, JWT, IIS).
    /// En Development se conserva el detalle para quien mantiene el sistema.
    /// </summary>
    public static class MensajeUsuarioHelper
    {
        public const string OperacionNoCompletada =
            "No se pudo completar la operación. Intente de nuevo o contacte al administrador.";
        public const string SesionExpirada =
            "Su sesión expiró. Cierre sesión e ingrese de nuevo.";
        public const string ServicioNoDisponible =
            "El servicio no está disponible en este momento. Intente más tarde.";
        public const string CorreoNoEnviado =
            "No se pudo enviar el correo. Contacte al administrador.";
        public const string SinResultados =
            "No hay registros para mostrar.";

        /// <summary>True solo en Development. Lo asigna Program.cs al arrancar.</summary>
        public static bool MostrarDetalleTecnico { get; set; }

        private static readonly Regex TokenTecnico = new(
            @"Tbl_|api/|Swagger|JWT|web\.config|ConnectionStrings|ApiSettings|stdout|Api_Colegio|ColSanFrancisco|DeserializeError|ConnectionError|AllowInvalidCertificate|IIS Express|docs/|\.sql\b|127\.0\.0\.1|localhost:\d+|HTTP\s*\d{3}|endpoint|CatPeriodo|IdPeriodo|JWT_SECRET|\.dll\b|SMTP|AccessToken|PhoneNumberId|NullReference|Object reference|InnerException|InvalidOperation|HttpRequest|at System\.|at Microsoft\.",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static bool EsTecnico(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;
            if (TokenTecnico.IsMatch(texto))
                return true;
            if (texto.Contains("API", StringComparison.Ordinal)
                || texto.Contains("Api ", StringComparison.OrdinalIgnoreCase))
                return true;
            if (texto.Contains("SQL", StringComparison.OrdinalIgnoreCase)
                && texto.Contains("script", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        public static string ParaUsuario(string? mensaje, string? fallback = null)
        {
            var amigable = string.IsNullOrWhiteSpace(fallback) ? OperacionNoCompletada : fallback;
            if (string.IsNullOrWhiteSpace(mensaje))
                return amigable;
            if (MostrarDetalleTecnico)
                return mensaje.Trim();
            if (EsTecnico(mensaje))
                return Clasificar(mensaje);
            return mensaje.Trim();
        }

        public static string Combinar(string amigable, string? detalleTecnico)
        {
            if (MostrarDetalleTecnico && !string.IsNullOrWhiteSpace(detalleTecnico))
                return (amigable + " " + detalleTecnico.Trim()).Trim();
            return amigable;
        }

        public static string? OcultarSiTecnico(string? mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                return null;
            if (MostrarDetalleTecnico)
                return mensaje;
            if (EsTecnico(mensaje))
                return null;
            return mensaje;
        }

        private static string Clasificar(string mensaje)
        {
            if (mensaje.Contains("401", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("JWT", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("sesión", StringComparison.OrdinalIgnoreCase))
                return SesionExpirada;
            if (mensaje.Contains("ConnectionError", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("No such host", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("alcanza", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("conexión", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("Timeout", StringComparison.OrdinalIgnoreCase))
                return ServicioNoDisponible;
            if (mensaje.Contains("SMTP", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("correo", StringComparison.OrdinalIgnoreCase))
                return CorreoNoEnviado;
            return OperacionNoCompletada;
        }
    }
}
