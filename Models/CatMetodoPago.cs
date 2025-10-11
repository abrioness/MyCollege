namespace WebColegio.Models
{
    public class CatMetodoPago
    {
        public int IdMetodoPago { get; set; }

        public string MetodoPago { get; set; } = null!;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public DateOnly FehaRegistro { get; set; }

        public int IdUsuarioRegistra { get; set; }

        public DateOnly? FechaActualiza { get; set; }

        public int? IdUsuarioActualiza { get; set; }
    }
}
