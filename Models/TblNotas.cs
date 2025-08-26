namespace WebColegio.Models
{
    public class TblNotas
    {
        public int IdNota { get; set; }

        //public int IdAsignacion { get; set; }

        public int IdTipoEvaluacion { get; set; }

        public int IdPeriodo { get; set; }
        public int IdAlumno { get; set; }
        public int IdAsignatura { get; set; }
        public decimal PrimerCorte { get; set; }
        public decimal SegundoCorte { get; set; }
        public decimal? TercerCorte { get; set; }
        public decimal? CuartoCorte { get; set; }
        public decimal NotaFinal { get; set; }


        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
