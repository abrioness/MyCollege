namespace WebColegio.Models
{
    public class TblEgreso
    {
        public int IdEgreso { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int Concepto { get; set; }
        public int NumeroRecibo { get; set; }
        public string Serie { get; set; } = null!;
        public decimal Monto { get; set; }
        public DateTime? FechaEmision { get; set; }
        public int? Anyo { get; set; }
        public int IdPeriodo { get; set; }
        public int? IdRecinto { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
    }
}
