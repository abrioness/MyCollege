using System.Text.Json.Serialization;

namespace Api_Colegio.Models
{
    public class TblCatTipoMovimiento
    {
        public int IdTipoMovimiento { get; set; }
        public string Concepto { get; set; } = null!;
        public string? Descripcion { get; set; } 
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
        [JsonIgnore]
        public virtual ICollection<TblPagos> tblPagos { get; set; } = new List<TblPagos>();
    }
}
