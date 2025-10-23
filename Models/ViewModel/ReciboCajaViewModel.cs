using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebColegio.Models.ViewModel
{
    public class ReciboCajaViewModel
    {
        public TblReciboCaja ReciboCaja { get; set; }
        public TblPago Pago { get; set; }

        
        public int SiguienteNumero { get; set; }
        public List<TblReciboCaja> reciboCajas { get; set; }
        public List<TblUsuarios> usuarios { get; set; }
        public List<SelectListItem> pagosSelectListItem { get; set; }
        public List<SelectListItem> estadoPagoSelectListItem { get; set; }
        public List<SelectListItem> alumnosSelectListItem { get; set; }
        public List<SelectListItem> usuariosSelectListItem { get; set; }
        public List<SelectListItem> gradosSelectListItem { get; set; }
        public List<SelectListItem> tipoMovimientoSelectListItem { get; set; }

    }

    public class ReciboPagoViewModel
    {
        public int IdPago { get; set; }
        public int? IdAlumno { get; set; }
        public int IdMes { get; set; }
        public int IdGrado { get; set; }
        public int IdPeriodo { get; set; }
        public int IdMetodoPago { get; set; }
        public int TipoMovimiento { get; set; }
        public int TipoRecibo { get; set; }
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
