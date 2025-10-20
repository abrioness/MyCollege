using System.Text.Json.Serialization;

namespace Api_Colegio.Models
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
        [JsonIgnore]
        public virtual TblPagos? IdPagosNavigation { get; set; } = null!;
        [JsonIgnore]
        public virtual TblEstadoPago? IdEstadoPagoNavigation { get; set; } = null!;
        [JsonIgnore]
        public virtual TblCatTiposDescuento? IdTipoDescuentoNavigation { get; set; } = null!;

        [JsonIgnore]
        public virtual TblCatMeses? IdMesesNavigation { get; set; } = null!;
    }
}
