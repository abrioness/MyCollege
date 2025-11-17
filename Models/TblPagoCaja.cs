namespace WebColegio.Models
{
    public class TblPagoCaja
    {
        public int IdPagoCaja { get; set; }
        public string Nombre { get; set; } = null!;
        public int NumeroRecibo { get; set; }
        public string Serie { get; set; } = null!;
        public int IdGrado { get; set; }
        public int IdTurno { get; set; }
        public int IdPeriodo { get; set; }
        public int Anyo { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Monto { get; set; }
        public int Concepto { get; set; }
        public string Descripcion { get; set; } = null!;
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
