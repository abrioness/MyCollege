namespace WebColegio.Configuration
{
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? User { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = string.Empty;
        public string FromName { get; set; } = "Colegio Parroquial San Francisco Javier";
        public string NombreColegio { get; set; } = "Colegio Parroquial San Francisco Javier";

        /// <summary>Si es true, el correo no acepta respuesta (Reply-To = noreply).</summary>
        public bool NoReply { get; set; } = true;

        public int HoraEnvio { get; set; } = 8;
    }
}
