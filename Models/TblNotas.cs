namespace WebColegio.Models
{
    public class TblNotas
    {
        public int IdNota { get; set; }

        public int IdAsignacion { get; set; }

        public int IdTipoEvaluacion { get; set; }

        public int IdPeriodo { get; set; }

        public decimal Calificaciones { get; set; }

        public string? Descripcion { get; set; }
    }
}
