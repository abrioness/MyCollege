namespace WebColegio.Configuration
{
    public class ApiSettings
    {
        public const string SectionName = "ApiSettings";

        public string? BaseUrl { get; set; }

        /// <summary>
        /// Host IIS del sitio (ej. colegioparroquialsanfranciscojavier.com).
        /// Usar con BaseUrl http://127.0.0.1/... cuando Web y API están en el mismo servidor.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Solo para desarrollo o servidores de prueba con certificado autofirmado.
        /// En producción final debe ser false y la API debe tener un certificado válido.
        /// </summary>
        public bool AllowInvalidCertificate { get; set; }
    }
}
