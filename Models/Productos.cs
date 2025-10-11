namespace WebColegio.Models
{
    public class Productos
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = null!;

        public string? CodigoBarra { get; set; }

        public string? Descripcion { get; set; }

        public int IdCateProducto { get; set; }

        public int? IdProveedor { get; set; }

        public decimal CostoUnitario { get; set; }

        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public bool Activo { get; set; }

        public int UsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int? UsuarioActualiza { get; set; }

        public DateTime? FechaActualiza { get; set; }

    }
}
