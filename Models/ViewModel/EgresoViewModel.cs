using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class EgresoViewModel
    {
        public TblEgreso Egresos { get; set; } = new();

        public List<CatTipoMovimiento> tipoMovimiento { get; set; } = new List<CatTipoMovimiento>();            
        public int SiguienteNumero { get; set; }
        public int Mora { get; set; }
        public List<TblEgreso> listEgresos { get; set; } = new();
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
        public List<SelectListItem> turnosSelectListItem { get; set; } = new();
        public List<TblCatMeses> meses { get; set; } = new List<TblCatMeses>();
        public List<SelectListItem> periodo { get; set; } = new();
        public List<SelectListItem> recintos { get; set; } = new();

    }
}
