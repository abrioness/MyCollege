namespace WebColegio.Models
{
    public class CatProveedor
    {
        public int IdProveedor { get; set; }

        public string NombreProveedor { get; set; } = null!;

        public string? ContactoPrincipal { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
