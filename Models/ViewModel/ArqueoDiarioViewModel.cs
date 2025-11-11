using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class ArqueoDiarioViewModel
    {
        public TblArqueoDiario arqueoDiario { get; set; }
        public List<TblArqueoDiario> listaArqueoDiario { get; set; }
        public List<SelectListItem> TimpoMovimiento { get; set; } = new();
        public List<SelectListItem> Pagos { get; set; } = new();
        public List<SelectListItem> Turnos { get; set; } = new();
        public List<SelectListItem> Grados { get; set; } = new();
        public List<SelectListItem> Periodo { get; set; } = new();
    }
}
