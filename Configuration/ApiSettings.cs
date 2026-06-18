namespace WebColegio.Configuration
{
    public class ApiSettings
    {
        public const string SectionName = "ApiSettings";

        public string? BaseUrl { get; set; }

        /// <summary>
        /// Solo para desarrollo o servidores de prueba con certificado autofirmado.
        /// En producción final debe ser false y la API debe tener un certificado válido.
        /// </summary>
        public bool AllowInvalidCertificate { get; set; }
    }
}
