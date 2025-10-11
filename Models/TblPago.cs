namespace WebColegio.Models
{
    public class TblPago
    {
        public int IdPago { get; set; }

        public int? IdAlumno { get; set; }

        public decimal Monto { get; set; }

        public int Concepto { get; set; }
        public string MesPagado { get; set; }

        public int? Mora { get; set; }

        public int IdMetodoPago { get; set; }

        public int IdTipoMovimiento { get; set; }


        public int IdTipoRecibo { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualizo { get; set; }

        public DateTime? FechaActualizo { get; set; }
    }
}
