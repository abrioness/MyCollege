namespace WebColegio.Models
{
    public class TblArqueoDiario
    {
        public int IdArqueo { get; set; }
        public int Concepto { get; set; }
        public string NumeroRecibo { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public decimal TotalIngreso { get; set; }
        public string Detalle { get; set; } = null!;
        public int Cordobas { get; set; }
        public int Dolares { get; set; }
        public decimal TotalDolares { get; set; }
        public decimal TotalCordobas { get; set; }
        public decimal EquivalenteEn { get; set; }
        public decimal TotalDetalleDinero { get; set; }
        public decimal TotalDetalleEfectivo { get; set; }
        public decimal TotalViaTransferencia { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool UsuarioActualizo { get; set; }
        public DateTime FechaActualizo { get; set; }

    }
}
