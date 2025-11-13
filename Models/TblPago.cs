namespace WebColegio.Models
{
    public class TblPago
    {
        public int IdPago { get; set; }

        public int IdAlumno { get; set; }
       
        public decimal Monto { get; set; }
        public int? NumeroRecibo { get; set; }
        public string Serie { get; set; } = null!;
        public string Anyo { get; set; } = null!;
        public int IdMes { get; set; }

        public int? Mora { get; set; }
        public string Descripcion { get; set; } = null!;
        public int IdMetodoPago { get; set; }

        public int IdTipoMovimiento { get; set; }

        public int IdTipoRecibo { get; set; }
        public int IdPeriodo { get; set; }
        public int IdGrado { get; set; }
        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualizo { get; set; }

        public DateTime? FechaActualizo { get; set; }
    }
}
