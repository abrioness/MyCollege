namespace WebColegio.Models
{
    public class PagoColegiatura
    {
        public int IdPago { get; set; }

        public int IdFactura { get; set; }

        public DateTime FechaPago { get; set; }

        public decimal MontoPagado { get; set; }

        public int IdMetodoPago { get; set; }

        public string? ReferenciaPago { get; set; }

        public string? Descripcion { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
