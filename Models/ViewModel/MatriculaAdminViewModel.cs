using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;

namespace WebColegio.Models.ViewModel
{
    public class MatriculaListaViewModel
    {
        public List<TblMatricula> Matriculas { get; set; } = new();
        public List<TblAlumno> Alumnos { get; set; } = new();
        public List<CatPeriodo> Periodos { get; set; } = new();
        public List<Grados> Grados { get; set; } = new();
        public List<Modalidades> Modalidades { get; set; } = new();
        public List<Recintos> Recintos { get; set; } = new();
        public List<Turnos> Turnos { get; set; } = new();
        public List<Grupos> Grupos { get; set; } = new();
        public int? IdPeriodoFiltro { get; set; }
        public string? EstadoFiltro { get; set; }
        public string? TextoBusqueda { get; set; }
        public CatPeriodo? PeriodoMatriculaSugerido { get; set; }
        public string? ErrorApi { get; set; }
    }

    public class MatriculaFormViewModel
    {
        public TblMatricula Matricula { get; set; } = new();
        public string NombreAlumno { get; set; } = string.Empty;
        public bool EsEdicion { get; set; }
        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Grados { get; set; } = new();
        public List<SelectListItem> Modalidades { get; set; } = new();
        public List<SelectListItem> Recintos { get; set; } = new();
        public List<SelectListItem> Turnos { get; set; } = new();
        public List<SelectListItem> Grupos { get; set; } = new();
        public List<SelectListItem> Estados { get; set; } = new();

        [Display(Name = "Debe pagar rifa 1 (1.er semestre)")]
        public bool AplicaRifa1 { get; set; } = true;

        [Display(Name = "Debe pagar rifa 2 (2.º semestre)")]
        public bool AplicaRifa2 { get; set; } = true;
    }
}
