using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class ArqueoDiarioViewModel
    {
        public TblArqueoDiario arqueoDiario { get; set; } = new();
        public int siguienteNumero { get; set; }
        public string Colegio { get; set; }
        public string Serie { get; set; }
        public int Numero { get; set; }
        public List<Recintos> TblRecintos { get; set; } = new();
        public List<CatPeriodo> Periodos { get; set; } = new();

        public List<TblArqueoDiario> listaArqueoDiario { get; set; } = new();
        public List<SelectListItem> TimpoMovimiento { get; set; } = new();
        public List<SelectListItem> Pagos { get; set; } = new();
        public List<SelectListItem> Turnos { get; set; } = new();
        public List<SelectListItem> Grados { get; set; } = new();
        public List<SelectListItem> Periodo { get; set; } = new();
        public DateTime Fecha { get; set; }
        public List<TblEgreso> EgresosCaja { get; set; } = new();
        public List<IngresoDto> Ingresos { get; set; } = new();
        public List<IngresoCajaDto> IngresoCajaDto { get; set; } = new();
        public List<EgresoDto> Egresos { get; set; } = new();

        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal TotalEfectivo { get; set; }

        public List<DetalleCordoba> DetalleCordobas { get; set; } = new();
        public List<DetalleDolar> DetalleDolares { get; set; } = new();

        public decimal TotalCordobas { get; set; }
        public decimal TotalDolares { get; set; }
        public decimal EquivalenteCordobas { get; set; }

        public string TotalEnLetras { get; set; }
    }

    public class IngresoDto
    {
        public string? Concepto { get; set; }
        //public int? primerReciboDia { get; set; }
        //public int? ultimoReciboDia { get; set; }
        public string? Recinto { get; set; }
        public string? Recibo { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
    }
    public class IngresoCajaDto
    {
        public string? Conceptos { get; set; }
        //public int? primerReciboDia { get; set; }
        //public int? ultimoReciboDia { get; set; }
        public string? Recinto { get; set; }
        public string? Recibos { get; set; }
        public int Cantidades { get; set; }
        public decimal Montos { get; set; }
    }

    public class EgresoDto
    {
        public string Detalle { get; set; }
        public decimal Monto { get; set; }
    }

    public class DetalleCordoba
    {
        public int Cantidad { get; set; }
        public decimal Denominacion { get; set; }
        public decimal Monto { get; set; }
    }

    public class DetalleDolar
    {
        public int Cantidad { get; set; }
        public decimal Denominacion { get; set; }
        public decimal Monto { get; set; }
    }
}

