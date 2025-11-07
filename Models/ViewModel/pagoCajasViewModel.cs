using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class pagoCajasViewModel
    {
        public TblReciboCaja ReciboCaja { get; set; }
        public TblPagoCaja PagosCaja { get; set; } = new();

        public int Mora { get; set; }
        public List<TblPagoCaja> listPagoCaja { get; set; } = new();
        public List<SelectListItem> reciboCajas { get; set; } = new();
        public List<SelectListItem> pagosSelectListItem { get; set; } = new();
        public List<SelectListItem> alumnosSelectListItem { get; set; } = new();
        public List<SelectListItem> usuariosSelectListItem { get; set; } = new();
        public List<SelectListItem> tiposDescuentoSelectListItem { get; set; } = new();
        public List<SelectListItem> MeseselectListItem { get; set; } = new();
        public List<SelectListItem> tipoMovimientoSelectListItem { get; set; } = new();
        public List<SelectListItem> tipoRecibosSelectListItem { get; set; } = new();
        public List<SelectListItem> metodoPagoSelectListItem { get; set; } = new();

        public List<SelectListItem> mesesPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> tipoDescuentoSelectListItem { get; set; } = new();
        public List<SelectListItem> detallePagoSelectListItem { get; set; } = new();
        public List<SelectListItem> estadoPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> mesesSelectListItem { get; set; } = new();
        public List<SelectListItem> gradosSelectListItem { get; set; } = new();
        public List<TblCatMeses> meses { get; set; } = new List<TblCatMeses>();
        public List<CatPeriodo> periodo { get; set; } = new List<CatPeriodo>();
    }
}
