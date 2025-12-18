namespace WebColegio.Models
{
    public class TblUsuarios
    {
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = null!;

        public byte[] Password { get; set; } = null!;

        public string? Correo { get; set; }

        public string NombreCompleto { get; set; } = null!;

        public string? Cedula { get; set; }

        public int IdRol { get; set; }

        public bool Bloqueo { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
