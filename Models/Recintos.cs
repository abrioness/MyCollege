namespace WebColegio.Models
{
    public class Recintos
    {
        public int IdRecinto { get; set; }

        public string Recinto { get; set; } = null!;

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
