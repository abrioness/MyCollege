using System.Text.Json.Serialization;

namespace Api_Colegio.Models
{
    public class TblCatMeses
    {
        public int IdMes { get; set; }
        public string Mes { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }

        public DateTime? FechaActualizo { get; set; }
        [JsonIgnore]
        public virtual ICollection<TblCatDetallePagos> TblCatDetallePagos { get; set; }
        [JsonIgnore]
        public virtual ICollection<TblCatMesesPendientes> TblCatMesesPendientes{ get; set; }
    }
}
