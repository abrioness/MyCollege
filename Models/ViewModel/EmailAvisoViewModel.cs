namespace WebColegio.Models.ViewModel
{
    public class EmailAvisoViewModel
    {
        public bool SmtpConfigurado { get; set; }
        public string? UltimaCampana { get; set; }
        public DateTime? FechaProgramada { get; set; }
        public string? MensajeEstado { get; set; }
        public List<WhatsAppAvisoDestino> Destinos { get; set; } = new();
        public List<EmailEnvioLog> Historial { get; set; } = new();
        public int ConCorreo => Destinos.Count(d => !d.SinCorreo);
        public int SinCorreo => Destinos.Count(d => d.SinCorreo);
        public decimal TotalPendiente => Destinos.Sum(d => d.TotalPendiente);
        public string FechaProgramadaInput => (FechaProgramada ?? DateTime.Today).ToString("yyyy-MM-dd");
    }

    public class EmailEnvioLog
    {
        public DateTime Fecha { get; set; }
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; } = "";
        public string? Correo { get; set; }
        public bool Exito { get; set; }
        public string? Detalle { get; set; }
        public string Canal { get; set; } = "email";
    }

    public class EmailCampanaEstado
    {
        public DateTime? FechaProgramada { get; set; }
        public string? UltimaProgramadaYmd { get; set; }
        public DateTime? UltimaCampanaFecha { get; set; }
        public List<EmailEnvioLog> Envios { get; set; } = new();
    }
}
