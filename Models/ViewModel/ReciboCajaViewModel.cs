using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class ReciboCajaViewModel
    {
        public TblReciboCaja ReciboCaja { get; set; } = new TblReciboCaja();
        public TblPago Pago { get; set; }= new TblPago();
        public List<TblAlumno> alumnos { get; set; } = new List<TblAlumno>();
        public List<CatTipoMovimiento> tipoMovimiento { get; set; } = new List<CatTipoMovimiento>();
        public List<CatMetodoPago> metodoPago { get; set; } = new List<CatMetodoPago>();
        public List<Grados> grados { get; set; } = new List<Grados>();
        public int SiguienteNumero { get; set; }
        public string cantidadEnLetras { get; set; } = null!;
        public List<TblReciboCaja> reciboCajas { get; set; }= new List<TblReciboCaja>();
        public List<TblUsuarios> usuarios { get; set; } = new List<TblUsuarios>();
        public List<SelectListItem> pagosSelectListItem { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> estadoPagoSelectListItem { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> alumnosSelectListItem { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> usuariosSelectListItem { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> gradosSelectListItem { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> tipoMovimientoSelectListItem { get; set; } = new List<SelectListItem>();

    }

    public class ReciboPagoViewModel
    {
        public int IdPago { get; set; }
        public int? IdAlumno { get; set; }
        public int IdMes { get; set; }
        public int IdGrado { get; set; }
        public int IdPeriodo { get; set; }
        public int IdMetodoPago { get; set; }
        public int IdTipoMovimiento { get; set; }
        public int IdTipoRecibo { get; set; }
        public int? Mora { get; set; }
        public string Serie { get; set; }
        public int SiguienteNumero { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        //public string Observacion { get; set; }
        public TblReciboCaja ReciboCaja { get; set; }
        //// Datos adicionales opcionales
        //public string NombreAlumno { get; set; }
        //public string MesNombre { get; set; }
        //public string GradoNombre { get; set; }
        //public string PeriodoNombre { get; set; }
    }
}
