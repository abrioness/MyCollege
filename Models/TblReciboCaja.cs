namespace WebColegio.Models
{
    public class TblReciboCaja
    {

        public int IdRecibo { get; set; }

        //public int? IdPago { get; set; }
        public decimal Monto { get; set; }
        public int IdAlumno { get; set; } 

        public string Serie { get; set; } = null!;

        public int NumeroRecibo { get; set; }
        public int IdMetodoPago { get; set; }
        public int IdPago { get; set; }
        public int IdGrado { get; set; }
        public int IdTipoMovimiento { get; set; } 
        public bool? Activo { get; set; }

        public int? UsuarioRegistro { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public int UsuarioActualizo { get; set; }

        public DateTime FechaActualizo { get; set; }

    }
}
