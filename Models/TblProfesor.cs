namespace WebColegio.Models
{
    public class TblProfesor
    {
        public int IdProfesor { get; set; }

        public string Nombre { get; set; } = null!;

        public string Apellido { get; set; } = null!;

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public bool Activo { get; set; }

        public int IdUsuario { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
