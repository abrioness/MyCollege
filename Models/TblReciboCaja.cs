namespace WebColegio.Models
{
    public class TblReciboCaja
    {

        public int IdRecibo { get; set; }

        //public int? IdPago { get; set; }
        public decimal Monto { get; set; }
        public string RecibeDe { get; set; } = null!;

        public string Serie { get; set; } = null!;

        public int NumeroRecibo { get; set; }
        public string Concepto { get; set; } = null!;
        public int IdGrado { get; set; }
        public string TipoMovimiento { get; set; } = null!;
        public bool? Activo { get; set; }

        public int? UsuarioRegistro { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public int UsuarioActualizo { get; set; }

        public DateTime FechaActualizo { get; set; }

    }
}
