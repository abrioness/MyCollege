using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class PagosViewModel
    {
        
        public TblPago Pago { get; set; }= new();
        public int SiguienteNumero { get; set; }
        public int Mora { get; set; }
        public decimal MontoTotal { get; set; }
        public int mensualidad { get; set; }
        public int matricula {  get; set; }
        public List<TblAlumno> alumnos { get; set; } = new List<TblAlumno>();
        public List<CatTipoMovimiento> tipoMovimiento { get; set; } = new List<CatTipoMovimiento>();
        public List<CatMetodoPago> metodoPago { get; set; } = new List<CatMetodoPago>();
        public List<Grados> grados { get; set; } = new List<Grados>();
        public List<Turnos> turnos { get; set; } = new List<Turnos>();
        public List<Modalidades> modalidades { get; set; }=new List<Modalidades>();
        public List<Recintos> recinto { get; set; } = new List<Recintos>();

      
        public string cantidadEnLetras { get; set; } = null!;
        /// <summary>Cuando es pago de matrícula, total del recibo (matrícula + mensualidad primer mes mismo recibo).</summary>
        public decimal? MontoTotalRecibo { get; set; }
        /// <summary>Monto total del recibo en letras (para matrícula o mensualidades).</summary>
        public string? CantidadEnLetrasTotalRecibo { get; set; }
        /// <summary>Detalle de meses en el recibo (ej. "Enero, Febrero, Marzo" o "3 mensualidades").</summary>
        public string? DetalleMesesRecibo { get; set; }
        /// <summary>Desglose para recibos de mensualidad.</summary>
        public decimal? MontoMensualidadRecibo { get; set; }
        public decimal? MoraRecibo { get; set; }
        public decimal? TotalPagarRecibo { get; set; }
        /// <summary>Líneas del mismo recibo (mensualidad completa + abono al mes siguiente).</summary>
        public List<ReciboLineaDetalle> LineasRecibo { get; set; } = new();
        public List<TblReciboCaja> reciboCajas { get; set; } = new List<TblReciboCaja>();
        public List<TblPago> listPagos {get;set;} = new();

        public List<CatPeriodo> listPeriodos { get; set; } = new();
       
        public List<SelectListItem> pagosSelectListItem { get; set; } = new();
        public List<SelectListItem> alumnosSelectListItem { get; set; } = new();
        public List<SelectListItem> modalidadSelectListItem { get; set; } = new();
        public List<SelectListItem> usuariosSelectListItem { get; set; } = new();
        public List<SelectListItem> tiposDescuentoSelectListItem { get; set; } = new();
        public List<SelectListItem> MeseselectListItem { get; set; } = new();    
        public List<SelectListItem> tipoMovimientoSelectListItem { get; set; } = new();
        public List<SelectListItem> tipoRecibosSelectListItem { get; set; } = new();
        public List<SelectListItem> metodoPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> recintos { get; set; } = new();
        public List<SelectListItem> mesesPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> tipoDescuentoSelectListItem { get; set; } = new();
        public List<SelectListItem> detallePagoSelectListItem { get; set; } = new();
        public List<SelectListItem> estadoPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> mesesSelectListItem { get; set; } = new();
        public List<SelectListItem> gradosSelectListItem { get; set; } = new();
        public List<TblCatMeses>? meses { get; set; } = new List<TblCatMeses>();
        public List<SelectListItem> periodo { get; set; } = new ();
        public int? PreloadIdAlumno { get; set; }
        public string? PreloadNombreAlumno { get; set; }
        public int? PreloadIdRecinto { get; set; }
        public int? PreloadIdGrado { get; set; }
        public int? PreloadIdModalidad { get; set; }
        public int? PreloadIdPeriodo { get; set; }
        public string? PreloadMeses { get; set; }
        public bool EsCobroTraslado { get; set; }
        public bool CobraMatriculaTraslado { get; set; }
        public int? PreloadMesIngreso { get; set; }
    }

    public class ReciboLineaDetalle
    {
        public string Etiqueta { get; set; } = "";
        public decimal Monto { get; set; }
        public bool EsAbono { get; set; }
    }
}
