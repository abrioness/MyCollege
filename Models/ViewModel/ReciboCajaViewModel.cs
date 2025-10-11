using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class ReciboCajaViewModel
    {
        public TblReciboCaja ReciboCaja { get; set; }
        public TblPago Pago { get; set; }
        public int SiguienteNumero { get; set; }
        public List<TblReciboCaja> reciboCajas { get; set; }
        public List<TblUsuarios> usuarios { get; set; }
        public List<SelectListItem> pagosSelectListItem { get; set; }
        public List<SelectListItem> estadoPagoSelectListItem { get; set; }
        public List<SelectListItem> alumnosSelectListItem { get; set; }
        public List<SelectListItem> usuariosSelectListItem { get; set; }
        public List<SelectListItem> gradosSelectListItem { get; set; }
        public List<SelectListItem> tipoMovimientoSelectListItem { get; set; }

    }
}
