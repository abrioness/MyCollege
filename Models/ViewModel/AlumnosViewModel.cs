using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class AlumnosViewModel
    {
        public TblAlumno alumnos { get; set; }
        public List<SelectListItem> tipoEvaluacionesSelectListItem { get; set; } 
        public List<SelectListItem> materiasSelectListItem { get; set; } 
        public List<SelectListItem> notasSelectListItem { get; set; } 
        
        public List<SelectListItem> periodoEvaluacionsSelectListItem { get; set; } 
        public List<SelectListItem> recintosSelectListItem { get; set; } 
        public List<SelectListItem> sexosSelectListItem { get; set; }

        public List<SelectListItem> turnosSelectListItem { get; set; } 
        public List<SelectListItem> gruposSelectListItem { get; set; }
        public List<SelectListItem> gradosSelectListItem { get; set; } 
        public List<SelectListItem> modalidadesSelectListItem { get; set; } 



    }
}
