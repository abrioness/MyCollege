namespace WebColegio.Models.ViewModel
{
    public class WhatsAppAvisoDestino
    {
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; } = "";
        public string? Grado { get; set; }
        public string? Recinto { get; set; }
        public string? NombreContacto { get; set; }
        public string? OrigenTelefono { get; set; }
        public string? TelefonoE164 { get; set; }
        public string? MesesPendientes { get; set; }
        public decimal SaldoMatricula { get; set; }
        public decimal SaldoMensualidades { get; set; }
        public decimal TotalPendiente { get; set; }
        public string? EstadoPago { get; set; }
        public string Mensaje { get; set; } = "";
        public string? EnlaceWaMe { get; set; }
        public bool SinTelefono => string.IsNullOrWhiteSpace(TelefonoE164);
    }

    public class WhatsAppAvisoViewModel
    {
        public bool ApiConfigurada { get; set; }
        public bool EnvioAutomatico { get; set; }
        public string? UltimaCampana { get; set; }
        public string? MensajeEstado { get; set; }
        public List<WhatsAppAvisoDestino> Destinos { get; set; } = new();
        public List<WhatsAppEnvioLog> Historial { get; set; } = new();
        public int ConTelefono => Destinos.Count(d => !d.SinTelefono);
        public int SinTelefono => Destinos.Count(d => d.SinTelefono);
        public decimal TotalPendiente => Destinos.Sum(d => d.TotalPendiente);
    }

    public class WhatsAppEnvioLog
    {
        public DateTime Fecha { get; set; }
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; } = "";
        public string? Telefono { get; set; }
        public bool Exito { get; set; }
        public string? Detalle { get; set; }
        public string Canal { get; set; } = "api";
    }

    public class WhatsAppCampanaEstado
    {
        public string? UltimaCampanaYm { get; set; }
        public DateTime? UltimaCampanaFecha { get; set; }
        public List<WhatsAppEnvioLog> Envios { get; set; } = new();
    }
}
