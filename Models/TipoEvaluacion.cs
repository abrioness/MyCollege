namespace WebColegio.Models
{
    public class TipoEvaluacion
    {
        public int IdTipoEvaluacion { get; set; }

        public string NombreTipEvaluacion { get; set; } = null!;

        public decimal Porcentaje { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
