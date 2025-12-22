using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IServicesApi _Iservices;
        public ProductosController (IServicesApi servicesApi)
        {
            _Iservices = servicesApi;
        }

        // GET: ProductosController
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var _productos = await _Iservices.GetProductosAsync();
            var _movInvebtario = await _Iservices.GetMovInventarioAsync();
            var _categoriaProducto = await _Iservices.GetCategoriaProductoAsync();
            var _usuarioId = await _Iservices.GetUsuarioIdAsync(idUsuario);


            IQueryable<Productos> query = _productos.AsQueryable();

            // Aplicar filtros de manera acumulativa sin ejecutar la consulta
            if (fechainicio.HasValue)
            {
                // Normalizar la fecha de inicio al inicio del día (00:00:00)
                var fechaInicioNormalizada = fechainicio.Value.Date;
                query = query.Where(a => a.FechaRegistro >= fechaInicioNormalizada);
            }
            if (fechafin.HasValue)
            {
                // Normalizar la fecha de fin al final del día (23:59:59)
                var fechaFinNormalizada = fechafin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.FechaRegistro <= fechaFinNormalizada);
            }

            // Ejecutar la consulta SOLO al final, después de aplicar todos los filtros
            var productosFiltrados = query.ToList();

            if (_usuarioId.IdRol == 2 || _usuarioId.IdRol == 5)
            {


                var viewModel = new ColeccionCatalogos
                {

                    producto = query.Where(r => r.UsuarioRegistro == idUsuario).ToList(),
                    categoriasProducto = _categoriaProducto,
                    movinventario = _movInvebtario
                };
                return View(viewModel);
            }
            if(_usuarioId.IdRol==1)
            {
                var viewModel = new ColeccionCatalogos
                {

                    producto = productosFiltrados,
                    categoriasProducto = _categoriaProducto,
                    movinventario = _movInvebtario
                };
                return View(viewModel);
            }
            return RedirectToAction("SinPermiso", "Login");
        }

        // GET: ProductosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductosController/Create
        public async Task<ActionResult> Create()
        {
            var productos = await _Iservices.GetProductosAsync();

            int? maxNumero = productos
             .Where(r => !string.IsNullOrEmpty(r.CodigoBarra))
             .Select(r =>
             {
                 // Extraer los últimos 3 dígitos (por ejemplo "CORLAP-001" → 1)
                 var partes = r.CodigoBarra.Split('-');
                 if (partes.Length > 1 && int.TryParse(partes.Last(), out int num))
                     return num;
                 return 0;
             })
             .DefaultIfEmpty(0)
             .Max();

            var siguienteCodigo = (maxNumero.HasValue ? maxNumero.Value + 1 : 1).ToString("D5");
            var viewmodel = new productoViewModel
            {
                SiguienteCodigo=siguienteCodigo,

                listCategoriaProducto = (await _Iservices.GetCategoriaProductoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdCateProducto.ToString(),
                                       Text = r.NombreCategoria,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                listMovimientoInventario = (await _Iservices.GetMovInventarioAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdMovInventario.ToString(),
                                       Text = r.MovimientoInventario,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList()
            };
            return View(viewmodel);
        }

        // POST: ProductosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(productoViewModel producto)
        {
            bool response = false;
            bool ExisteProducto = false;
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                ExisteProducto = await _Iservices.ValidarProductos(producto.tblproducto.CodigoBarra,producto.tblproducto.IdCateProducto);
                if (ExisteProducto)
                {
                    TempData["Mensaje"] = "El Producto ya Posee un Registro.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create", "Productos");
                }

                if (producto != null)
                {
                    producto.tblproducto.Activo = true;
                    producto.tblproducto.UsuarioRegistro = idUsuario;
                    producto.tblproducto.FechaRegistro = DateTime.Now;
                    //producto.tblproducto.CodigoBarra = await GenerarCodigoAutomaticoAsync(producto.tblproducto.NombreProducto);
                    response = await _Iservices.PostProductosAsync(producto.tblproducto);
                    if (response)
                    {
                        TempData["Mensaje"] = "Se Guardo Correctamente el Producto.";
                        TempData["Tipo"] = "info";

                        return  RedirectToAction("Create","Productos");
                    }

                }
                return NoContent();
            }
            catch
            {
                return View();
            }
        }
        
        // GET: ProductosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
