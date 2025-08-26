namespace WebColegio.Models
{
    public class Asignaturas
    {
        public int IdAsignatura { get; set; }

        public string NombreAsignatura { get; set; } = null!;

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
