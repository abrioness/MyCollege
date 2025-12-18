using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class UsuarioViewModel
    {
        public TblUsuarios usuarios { get; set; } = new TblUsuarios();
        public List<TblUsuarios> ListaUsuarios { get; set; }= new List<TblUsuarios>();
        public string Password { get; set; }
        public List<SelectListItem> SubsistemasSelectList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> RolSelectList { get; set; } = new List<SelectListItem>();
    }
}
