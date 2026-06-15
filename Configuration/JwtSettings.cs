namespace WebColegio.Configuration
{
    /// <summary>Debe coincidir con la misma sección Jwt en la API de producción.</summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "WebColegio";
        public string Audience { get; set; } = "ColSanFranciscoApi";
        /// <summary>Clave secreta ≥ 32 caracteres. En producción usar variable de entorno Jwt__SecretKey.</summary>
        public string SecretKey { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 480;
    }
}
