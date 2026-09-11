namespace WebColegio.Models
{
    public class TblCierreCaja
    {
        public const string EstadoCerrado = "Cerrado";

        public int IdCierreCaja { get; set; }
        public int IdUsuarioCaja { get; set; }
        public int? IdRecinto { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime FechaCierre { get; set; }
        public decimal MontoIniCaja { get; set; }
        public decimal? MontoFinCaja { get; set; }
        public decimal? DiferenciaEfectivo { get; set; }
        public decimal TotalVentasEfectivo { get; set; }
        public decimal TotalVentasTarjetas { get; set; }
        public decimal TotalVentasOtros { get; set; }
        public string? Descripcion { get; set; }
        public string EstadoCierre { get; set; } = EstadoCerrado;
        public bool Activo { get; set; } = true;
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualiza { get; set; }
        public DateTime? FechaActualiza { get; set; }
    }
}
