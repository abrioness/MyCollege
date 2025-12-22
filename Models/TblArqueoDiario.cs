namespace WebColegio.Models
{
    public class TblArqueoDiario
    {
        public int IdArqueo { get; set; }
        //public int? Concepto { get; set; }
        public int NumeroArqueo { get; set; }

        public int? IdRecinto { get; set; }
        public int? IdPeriodo { get; set; }
        //public int? Cantidad { get; set; }
        public string? Serie { get; set; }
        //public decimal? Monto { get; set; }
        public decimal? TotalIngreso { get; set; }
        public decimal? TotalEgreso { get; set; }
        public string? Detalle { get; set; }

        public decimal? TotalDetalleEfectivo { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }

    }
}
