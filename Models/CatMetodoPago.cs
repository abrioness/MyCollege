namespace WebColegio.Models
{
    public class CatMetodoPago
    {
        public int IdMetodoPago { get; set; }

        public string MetodoPago { get; set; } = null!;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateTime FehaRegistro { get; set; }

        public int IdUsuarioRegistra { get; set; }

        public DateTime? FechaActualiza { get; set; }

        public int? IdUsuarioActualiza { get; set; }
    }
}
