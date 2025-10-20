using System.Text.Json.Serialization;

namespace WebColegio.Models
{
    public class TblCatDetallePagos
    {
        public int IdDetPago { get; set; }
        public int IdPago { get; set; }
        public int IdMes { get; set; }
        public int Anio { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal MontoPagado { get; set; }
        public int? IdTipoDescuento { get; set; }
        public int IdEstadoPago { get; set; }

        // Navigation properties
       
    }
}
