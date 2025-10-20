namespace Api_Colegio.Models.Vistas
{
    public class VistasAlumnoNotas
    {
        public List<TblAlumno> listAlumno {  get; set; }
        public List<TblNota> listNota { get; set; } = new List<TblNota>();

    }
}
