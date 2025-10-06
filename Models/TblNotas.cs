namespace WebColegio.Models
{
    public class TblNotas
    {
        public int IdNota { get; set; }

        public int IdTipoEvaluacion { get; set; }

        public int IdPeriodo { get; set; }

        public int IdAlumno { get; set; }

        public int IdAsignatura { get; set; }
        public int IdModalidad { get; set; }

        public int IdGrado { get; set; }
        public int? Acumulado1 { get; set; }
        public int? Examen1 { get; set; }

        public decimal? PrimerCorte { get; set; }
        public int? Acumulado2 { get; set; }
        public int? Examen2 { get; set; }
        public decimal? SegundoCorte { get; set; }
        public int? Acumulado3 { get; set; }
        public int? Examen3 { get; set; }
        public decimal? TercerCorte { get; set; }
        public int? Acumulado4 { get; set; }
        public int? Examen4 { get; set; }
        public decimal? CuartoCorte { get; set; }

        public decimal? NotaFinal { get; set; }

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
