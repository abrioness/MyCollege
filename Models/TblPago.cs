namespace WebColegio.Models
{
    public class TblPago
    {
        public int IdPago { get; set; }

        public int? IdAlumno { get; set; }

        public decimal Monto { get; set; }

        public string Concepto { get; set; } = null!;

        public int IdMetodoPago { get; set; }
        public long Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
