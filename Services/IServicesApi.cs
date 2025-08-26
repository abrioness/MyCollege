using WebColegio.Models;

namespace WebColegio.Services
{
    public interface IServicesApi
    {
        #region Metodos Get
        Task<List<TblAlumno>> GetAlumnosAsync();
        Task<List<Recintos>> GetRecintosAsync();
        Task<List<Sexos>> GetSexosAsync();
        Task<List<Modalidades>> GetModalidadesAsync();
        Task<List<Grupos>> GetGruposAsync();
        Task<List<Turnos>> GetTurnosAsync();
        Task<List<Grados>> GetGradosAsync();
        Task<List<TipoEvaluacion>> GetTipEvaluacionAsync();
        Task<List<PeriodoEvaluacion>> GetPeriodoEvaluacionAsync();
        Task<List<Asignaturas>> GetAsignaturaAsync();
        Task<List<TblNotas>> GetNotasAsync();
        Task<TblAlumno> V_alumnoNotas(int idnota);
        #endregion
        #region Metodos Post
        Task<bool> PostAlumnosAsync(TblAlumno alumnos);
        Task<bool> PostNotasAsync(TblNotas notas);
        #endregion
        #region Metodos Put
        Task<bool> UpdateAlumnos(TblAlumno alumno);
        Task<bool> UpdateNotas(TblNotas nota);
        #endregion
        #region Metodos Get por Id
        Task<TblAlumno> GetAlumnoIdAsync(int id);
        Task<TblNotas> GetNotasById(int id);
        #endregion
        #region Metodos de Busqueda
        Task<List<TblAlumno>> searchAlumnosAsync();
        #endregion

    }
}
