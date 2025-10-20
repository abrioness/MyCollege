using System.Text.Json.Serialization;

namespace Api_Colegio.Models
{
    public class TblCatTiposDescuento
    {
        public int IdTipoDescuento { get; set; }
        public string NombreDescuento { get; set; }
        public decimal Porcentaje { get; set; }
        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualizo { get; set; }

        public DateTime? FechaActualizo { get; set; }
        [JsonIgnore]
        public virtual ICollection<TblPagos> TblCatPagos { get; set; }
       
        [JsonIgnore]
        public virtual ICollection<TblCatDetallePagos> TblCatDetallePagos { get; set; }
    }
}
