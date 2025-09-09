using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class NotasViewModel
    {
        public TblNotas notas { get; set; }
        public TblAlumno alumnoNotas { get; set; }

        public List<TblNotas> listNotas { get; set; }
        public List<SelectListItem> alumnosSelectListItem { get; set; }
        public List<SelectListItem> tipoEvaluacionesSelectListItem { get; set; }
        
        public List<SelectListItem> asignaturaSelectListItem { get; set; }

        public List<SelectListItem> periodoEvaluacionsSelectListItem { get; set; }
        public List<SelectListItem> sexoSelectListItem { get; set; }


    }
}
