namespace WebColegio.Models
{
    public class Grupos
    {
        public int IdGrupo { get; set; }

        public string NombreGrupo { get; set; } = null!;

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
        
    }
}
