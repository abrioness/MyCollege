namespace WebColegio.Models
{
    /// <summary>
    /// Representa un ítem de producto vendido en un recibo de caja para afectar el inventario.
    /// </summary>
    public class DetallePagoCajaItem
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        /// <summary>Opcional: nombre para mostrar en la tabla del modal (no se envía al servidor).</summary>
        public string? NombreProducto { get; set; }
        /// <summary>Opcional: categoría para mostrar (no se envía al servidor).</summary>
        public string? NombreCategoria { get; set; }
    }
}
