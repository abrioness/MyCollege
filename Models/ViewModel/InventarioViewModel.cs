using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class InventarioViewModel
    {
        public TblInventario Inventario { get; set; } = new();
        public List<TblInventario> ListaInventario { get; set; } = new();
        public List<SelectListItem> ListaProductos { get; set; } = new();
        public List<SelectListItem> ListaMovInventario { get; set; } = new();

    }
}
