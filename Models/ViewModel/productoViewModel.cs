using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class productoViewModel
    {
        public Productos tblproducto { get; set; } = new();
        public List<Productos> listaproducto { get; set; } = new();
        public List<SelectListItem> ListaCategoriaProducto { get; set; } = new();
        public List<SelectListItem> ListaProveedores { get; set; } = new();
        public List<SelectListItem> ListaUsuarios { get; set; } = new();
    }
}
