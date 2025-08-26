namespace WebColegio.Models.ViewModel
{
    public class ColeccionCatalogos
    {
       public List<TipoEvaluacion> tipoEvaluaciones { get; set; } = new List<TipoEvaluacion>();
        public List<TblMaterias> materias { get; set; } = new List<TblMaterias>();
        public List<TblNotas> notas { get; set; } = new List<TblNotas>();
        public List<TblAlumno> alumno { get; set; } = new List<TblAlumno>();
        public List<PeriodoEvaluacion> periodoEvaluacions { get; set; } = new List<PeriodoEvaluacion>();
        public List<Recintos> recintos { get; set; } = new List<Recintos>();
        public List<Sexos> sexos { get; set; } = new List<Sexos>();

        public List<Turnos> turnos { get; set; } = new List<Turnos>();
        public List<Grupos> grupos { get; set; } = new List<Grupos>();
        public List<Grados> grados { get; set; } = new List<Grados>();
        public List<Modalidades> modalidades { get; set; } = new List<Modalidades>();
        public List<Asignaturas> asignaturas { get; set; } = new List<Asignaturas>();



    }
}
