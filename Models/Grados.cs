namespace WebColegio.Models
{
    public class Grados
    {
        public int IdGrado { get; set; }

        public string NombreGrado { get; set; } = null!;

        public string? NivelEducativo { get; set; }

        public bool Activo { get; set; }

        public int UsuarioResgistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }

    }
}
