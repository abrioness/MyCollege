using System.Text.Json.Serialization;

namespace Api_Colegio.Models
{
    public class TblCatDiscapacidad
    {
        public int Id_Discapacidad { get; set; }
        public string Discapacidad { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualizo { get; set; }
        public DateTime? FechaActualizo { get; set; }
        [JsonIgnore]
        public virtual ICollection<TblAlumno> TblAlumnos { get; set; } = new List<TblAlumno>();
    }
}
