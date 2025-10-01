using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class FacturacionViewModel
    {
      
        public FacturaColegiatura Facturacion { get; set; }
        public TblAlumno FacturaAlumno { get; set; }

        public List<SelectListItem> tipoColegiaturas { get; set; }
        public List<SelectListItem> estadoPagos { get; set; }
        public List<SelectListItem> alumnos { get; set; }

      
    }

}
