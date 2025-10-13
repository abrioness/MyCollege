using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class AlumnosViewModel
    {
        public TblAlumno alumnos { get; set; } = new();
        public string codigoestudiante { get; set; }
        public List<SelectListItem> tipoEvaluacionesSelectListItem { get; set; } = new();
        public List<SelectListItem> materiasSelectListItem { get; set; } = new();
        public List<SelectListItem> notasSelectListItem { get; set; } = new();

        public List<SelectListItem> periodoEvaluacionsSelectListItem { get; set; } = new();
        public List<SelectListItem> recintosSelectListItem { get; set; } = new();
        public List<SelectListItem> sexosSelectListItem { get; set; } = new();

        public List<SelectListItem> turnosSelectListItem { get; set; } = new();
        public List<SelectListItem> gruposSelectListItem { get; set; } = new();
        public List<SelectListItem> gradosSelectListItem { get; set; } = new();
        public List<SelectListItem> modalidadesSelectListItem { get; set; } = new();
        public List<SelectListItem> discapacidadSelectListItem { get; set; } = new();



    }
}
