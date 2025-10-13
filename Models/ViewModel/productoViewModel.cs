using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class productoViewModel
    {
        public Productos tblproducto { get; set; } = new();
        public string SiguienteCodigo { get; set; }
        public List<Productos> listaproducto { get; set; } = new();
        public List<SelectListItem> listCategoriaProducto { get; set; } = new();
        public List<SelectListItem> ListaProveedores { get; set; } = new();
        public List<SelectListItem> ListaUsuarios { get; set; } = new();
        
    }
}
