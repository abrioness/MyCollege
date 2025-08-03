namespace WebColegio.Models
{
    public class Sexos
    {
        public int IdSexo { get; set; }
        public string Sexo { get; set; } 
        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }

    }
}
