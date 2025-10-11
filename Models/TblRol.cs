namespace WebColegio.Models
{
    public class TblRol
    {
        public int IdRol { get; set; }

        public string NombreRol { get; set; } = null!;

        public string? Descripcion { get; set; }

        public int Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
