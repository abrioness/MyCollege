using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;

namespace WebColegio.Models.ViewModel
{
    public class CostosAdminViewModel
    {
        public List<TblCostoMatricula> CostosMatricula { get; set; } = new();
        public List<TblCostoMensualidad> CostosMensualidad { get; set; } = new();
        public List<CatPeriodo> Periodos { get; set; } = new();
        public List<Recintos> Recintos { get; set; } = new();
        public List<Modalidades> Modalidades { get; set; } = new();
        public List<Grados> Grados { get; set; } = new();

        public int? IdPeriodoFiltro { get; set; }
        public CatPeriodo? PeriodoActual { get; set; }
        public CatPeriodo? PeriodoSiguiente { get; set; }
        public int AnioCicloSiguiente { get; set; }
        public bool VentanaMatriculaSiguienteCiclo { get; set; }
        public bool PeriodoSiguienteExiste { get; set; }
        public bool CicloSiguienteCreadoAutomaticamente { get; set; }
        public int CantidadCostosMatriculaSiguiente { get; set; }
        public int CantidadCostosMensualidadSiguiente { get; set; }
        public string? AvisoCiclo { get; set; }
    }

    public class CostoMatriculaFormViewModel
    {
        public TblCostoMatricula Costo { get; set; } = new();
        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Recintos { get; set; } = new();
        public List<SelectListItem> Modalidades { get; set; } = new();
        public bool EsEdicion { get; set; }
    }

    public class CostoMensualidadFormViewModel
    {
        public TblCostoMensualidad Costo { get; set; } = new();
        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Recintos { get; set; } = new();
        public List<SelectListItem> Modalidades { get; set; } = new();
        public List<SelectListItem> Grados { get; set; } = new();
        public bool EsEdicion { get; set; }
    }
}
