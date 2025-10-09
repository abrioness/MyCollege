using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class UsuarioViewModel
    {
        public TblUsuarios usuario { get; set; }
        public string Password { get; set; }
        public List<SelectListItem> SubsistemasSelectList { get; set; }
        public List<SelectListItem> RolSelectList { get; set; }
    }
}
