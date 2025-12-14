namespace WebColegio.Models
{
    public class FacturaColegiatura
    {
        public int IdFactura { get; set; }

        public int IdAlumno { get; set; }

        public int IdTipoColegiatura { get; set; }

        public DateTime FechaEmision { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public decimal MontoTotal { get; set; }

        public int IdEstado { get; set; }

        public string MesFacturado { get; set; } = null!;

        public int AnyoFacturado { get; set; }

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
