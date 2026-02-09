using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;

namespace WebColegio.Models.ViewModel
{
    public class pagoCajasViewModel
    {
        //public TblReciboCaja ReciboCaja { get; set; }
        public TblPagoCaja PagosCaja { get; set; } = new();

        /// <summary>Productos vendidos en este recibo; al procesar el pago se descuenta el StockActual de cada uno.</summary>
        public List<DetallePagoCajaItem> DetalleItems { get; set; } = new();

        public List<CatTipoMovimiento> tipoMovimiento { get; set; } = new List<CatTipoMovimiento>();
        public List<CatMetodoPago> metodoPago { get; set; } = new List<CatMetodoPago>();
        public List<Grados> grados { get; set; } = new List<Grados>();
        public List<Turnos> turnos { get; set; } = new List<Turnos>();
        public int SiguienteNumero { get; set; }
        public int Mora { get; set; }
        
        public List<TblPagoCaja> listPagoCaja { get; set; } = new();
        public List<SelectListItem> reciboCajas { get; set; } = new();
        public List<SelectListItem> pagosSelectListItem { get; set; } = new();
        public List<SelectListItem> alumnosSelectListItem { get; set; } = new();
        public List<SelectListItem> usuariosSelectListItem { get; set; } = new();
        public List<SelectListItem> tiposDescuentoSelectListItem { get; set; } = new();
        public List<SelectListItem> MeseselectListItem { get; set; } = new();
        public List<SelectListItem> tipoMovimientoSelectListItem { get; set; } = new();
        public List<SelectListItem> tipoRecibosSelectListItem { get; set; } = new();
        public List<SelectListItem> metodoPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> recintos { get; set; } = new();
        //public List<Recintos> recinto { get; set; } = new();
        public List<SelectListItem> mesesPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> tipoDescuentoSelectListItem { get; set; } = new();
        public List<SelectListItem> detallePagoSelectListItem { get; set; } = new();
        public List<SelectListItem> estadoPagoSelectListItem { get; set; } = new();
        public List<SelectListItem> mesesSelectListItem { get; set; } = new();
        public List<SelectListItem> gradosSelectListItem { get; set; } = new();
        public List<SelectListItem> turnosSelectListItem { get; set; } = new();
        public List<TblCatMeses> meses { get; set; } = new List<TblCatMeses>();
        public List<SelectListItem> periodo { get; set; } =  new();

        /// <summary>Para el modal de productos: listado de categorías (ej. Librería, Uniforme).</summary>
        public List<CategoriaProducto> CategoriasProducto { get; set; } = new();
        /// <summary>Para el modal de productos: listado de productos y filtrar por categoría.</summary>
        public List<Productos> Productos { get; set; } = new();
    }
}
