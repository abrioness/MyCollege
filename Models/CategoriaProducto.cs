namespace WebColegio.Models
{
    public class CategoriaProducto
    {
        public int IdCateProducto { get; set; }

        public string NombreCategoria { get; set; } = null!;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }
    }
}
