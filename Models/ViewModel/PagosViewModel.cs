using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models
{
    public class PagosViewModel
    {
        public TblReciboCaja ReciboCaja { get; set; }
        public TblPago Pago { get; set; }
        public List<SelectListItem> reciboCajas { get; set; }
        public List<SelectListItem> pagosSelectListItem { get; set; }
        public List<SelectListItem> alumnosSelectListItem { get; set; }
        public List<SelectListItem> usuariosSelectListItem { get; set; }
    }
}
