using System.Text.Json.Serialization;

namespace Api_Colegio.Models
{
    public class TblCatMovimientoInventario
    {
        public int IdMovInventario { get; set; }
        public string MovimientoInventario { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
        [JsonIgnore]
        public virtual ICollection<TblInventario> TblInventario { get; set; } = new List<TblInventario>();
    }
}
