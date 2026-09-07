namespace WebColegio.Configuration
{
    public class WhatsAppSettings
    {
        public const string SectionName = "WhatsApp";

        /// <summary>Activa el envío automático el día 1 de cada mes.</summary>
        public bool EnvioAutomatico { get; set; }

        /// <summary>Hora local (0-23) a partir de la cual corre la campaña del día 1.</summary>
        public int HoraEnvio { get; set; } = 8;

        public string CodigoPais { get; set; } = "505";
        public string NombreColegio { get; set; } = "Colegio Parroquial San Francisco Javier";

        /// <summary>Token permanente o temporal de Meta WhatsApp Cloud API.</summary>
        public string? AccessToken { get; set; }

        /// <summary>Phone Number ID del número de WhatsApp Business.</summary>
        public string? PhoneNumberId { get; set; }

        public string ApiVersion { get; set; } = "v21.0";

        /// <summary>
        /// Plantilla aprobada en Meta (obligatoria para escribir el día 1 si el tutor no ha chateado).
        /// Si está vacía se envía texto libre (solo funciona dentro de la ventana de 24 h).
        /// </summary>
        public string? TemplateName { get; set; }

        public string TemplateLanguage { get; set; } = "es";
    }
}
