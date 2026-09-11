using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;

namespace WebColegio.Models.ViewModel
{
    /// <summary>ViewModel para la lista de arqueos (Index) con filtros por fecha y recinto.</summary>
    public class ArqueoDiarioIndexViewModel
    {
        public List<TblArqueoDiario> ListaArqueos { get; set; } = new();
        public List<Recintos> Recintos { get; set; } = new();
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdRecintoFilter { get; set; }
        public bool PuedeElegirRecinto { get; set; } = true;
    }

    public class ArqueoDiarioViewModel
    {
        public TblArqueoDiario arqueoDiario { get; set; } = new();
        public int siguienteNumero { get; set; }
        public string Colegio { get; set; }
        public string Direccion { get; set; }
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
        public DateTime InicioVentana { get; set; }
        public DateTime FinVentana { get; set; }
        public bool ArqueoYaCerrado { get; set; }
        public bool CajaCerrada { get; set; }
        public DateTime? FechaCierreCaja { get; set; }
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

        /// <summary>True si el usuario es Admin y puede elegir recinto; false para Cajero (solo su recinto).</summary>
        public bool EsAdministrador { get; set; }

        /// <summary>Tras guardar con éxito, abrir el diálogo de impresión al cerrar el mensaje.</summary>
        public bool AbrirImpresion { get; set; }
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

    public class CierreCajaViewModel
    {
        public DateTime Fecha { get; set; }
        public int? IdRecinto { get; set; }
        public string? NombreRecinto { get; set; }
        public bool EsAdministrador { get; set; }
        public List<Recintos> Recintos { get; set; } = new();
        public bool CajaCerrada { get; set; }
        public DateTime? FechaCierre { get; set; }
        public DateTime InicioTurno { get; set; }
        public string EstadoCierre { get; set; } = "Abierta";
    }
}

