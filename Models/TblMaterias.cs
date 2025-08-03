namespace WebColegio.Models
{
    public class TblMaterias
    {
        public int IdMateria { get; set; }

        public string NombreMateria { get; set; } = null!;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
