using System.Text.Json.Serialization;

namespace WebColegio.Models
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
        
    }
}
