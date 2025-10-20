using System.Text.Json.Serialization;

namespace Api_Colegio.Models
{
    public class TblCatMesesPendientes
    {
        public int IdMesPendiente { get; set; }
        public int IdAlumno { get; set; }
        public int IdMes { get; set; }
        public int Anio { get; set; }
        public decimal Monto { get; set; }
        public int IdEstadoPago { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        [JsonIgnore]
        public virtual TblCatMeses? IdMesesNavigation { get; set; }
        [JsonIgnore]
        public virtual TblAlumno? IdAlumnoNavigation { get; set; }
        [JsonIgnore]
        public virtual TblEstadoPago? IdEstadoPagoNavigation { get; set; }
    }
}
