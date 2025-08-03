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
        #endregion
        #region Metodos Post
        Task<bool> PostAlumnosAsync(TblAlumno alumnos);
        #endregion
        #region Metodos Get por Id
        //Task<TblAlumno> GetAlumnoId(int id);
        
        #endregion
    }
}
