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
    }
}
