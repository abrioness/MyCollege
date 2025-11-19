using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class NotasViewModel
    {
        public TblNotas notas { get; set; }

        public TblAlumno alumnoNotas { get; set; } =new();

        public List<TblNotas> listNotas { get; set; } = new();

        public List<SelectListItem> alumnosSelectListItem { get; set; } = new();

        public List<SelectListItem> tipoEvaluacionesSelectListItem { get; set; } = new();

        public List<SelectListItem> asignaturaSelectListItem { get; set; } = new();

        public List<SelectListItem> modalidadSelectListItem { get; set; } = new();

        public List<SelectListItem> gradosSelectListItem { get; set; } = new();

        public List<SelectListItem> periodoEvaluacionsSelectListItem { get; set; } = new();

        public List<SelectListItem> sexoSelectListItem { get; set; } = new();

        public List<SelectListItem> periodoSelectListItem { get; set; } = new();


    }
}
