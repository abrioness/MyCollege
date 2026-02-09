namespace WebColegio.Models
{
    /// <summary>
    /// Registro de entrada o salida de inventario para trazabilidad y reportes.
    /// </summary>
    public class MovimientoInventario
    {
        public int IdInventario { get; set; }
        public int IdProducto { get; set; }
        /// <summary>1 = Entrada, 2 = Salida</summary>
        public int TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string? ReferenciaDocumento { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioActualiza { get; set; }
        public DateTime? FechaActualiza { get; set; }
    }

    public static class TipoMovimientoInventario
    {
        public const int Entrada = 1;
        public const int Salida = 2;
    }
}
