namespace WebColegio.Models
{
    public class AsignarProfesorMateria
    {
        public int IdAsignacion { get; set; }

        public int IdProfesor { get; set; }

        public int IdMateria { get; set; }

        public int IdGrado { get; set; }

        public int AnyoAcademico { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
