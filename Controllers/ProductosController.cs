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
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool existeProducto = await _Iservices.ValidarProductos(producto.tblproducto.CodigoBarra, producto.tblproducto.IdCateProducto);

                if (producto == null)
                    return NoContent();

                producto.tblproducto.Activo = true;
                producto.tblproducto.UsuarioRegistro = idUsuario;
                producto.tblproducto.FechaRegistro = DateTime.Now;

                if (existeProducto)
                {
                    // Ingreso a producto existente: sumar cantidad al StockActual
                    var existente = await _Iservices.GetProductoByCodigoYCategoriaAsync(producto.tblproducto.CodigoBarra!, producto.tblproducto.IdCateProducto);
                    if (existente == null)
                    {
                        TempData["Mensaje"] = "No se encontró el producto existente.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create", "Productos");
                    }
                    // Cantidad que ingresa: usar Existencia Inicial del formulario como "cantidad a sumar"
                    int cantidadEntrante = producto.tblproducto.ExistenciaInicial > 0
                        ? producto.tblproducto.ExistenciaInicial
                        : (producto.tblproducto.StockActual > 0 ? producto.tblproducto.StockActual : 0);
                    if (cantidadEntrante <= 0)
                    {
                        TempData["Mensaje"] = "Indique la cantidad que ingresa (Existencia inicial o Stock actual).";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create", "Productos");
                    }
                    existente.StockActual += cantidadEntrante;
                    existente.ImporteInventario = existente.StockActual * existente.CostoUnitario;
                    existente.UsuarioActualiza = idUsuario;
                    existente.FechaActualiza = DateTime.Now;
                    response = await _Iservices.UpdateProductoAsync(existente);
                    if (response)
                    {
                        // Registrar movimiento de entrada para reportes
                        await _Iservices.PostMovimientoInventarioAsync(new MovimientoInventario
                        {
                            IdProducto = existente.IdProducto,
                            TipoMovimiento = TipoMovimientoInventario.Entrada,
                            Cantidad = cantidadEntrante,
                            FechaMovimiento = DateTime.Now,
                            ReferenciaDocumento = "INGRESO-MANUAL",
                            Descripcion = "Ingreso por registro de inventario",
                            Activo = true,
                            UsuarioRegistro = idUsuario,
                            FechaRegistro = DateTime.Now
                        });
                        TempData["Mensaje"] = $"Se actualizó el producto. Se sumaron {cantidadEntrante} unidades. Stock actual: {existente.StockActual}.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Create", "Productos");
                    }
                }
                else
                {
                    // Producto nuevo: StockActual = Existencia Inicial al dar de alta
                    producto.tblproducto.StockActual = producto.tblproducto.ExistenciaInicial >= 0
                        ? producto.tblproducto.ExistenciaInicial
                        : producto.tblproducto.StockActual;
                    producto.tblproducto.ImporteInventario = producto.tblproducto.StockActual * producto.tblproducto.CostoUnitario;
                    response = await _Iservices.PostProductosAsync(producto.tblproducto);
                    if (response)
                    {
                        // Registrar movimiento de entrada (stock inicial) para reportes
                        var nuevoProducto = await _Iservices.GetProductoByCodigoYCategoriaAsync(producto.tblproducto.CodigoBarra!, producto.tblproducto.IdCateProducto);
                        if (nuevoProducto != null && producto.tblproducto.StockActual > 0)
                        {
                            await _Iservices.PostMovimientoInventarioAsync(new MovimientoInventario
                            {
                                IdProducto = nuevoProducto.IdProducto,
                                TipoMovimiento = TipoMovimientoInventario.Entrada,
                                Cantidad = producto.tblproducto.StockActual,
                                FechaMovimiento = DateTime.Now,
                                ReferenciaDocumento = "ALTA-PRODUCTO",
                                Descripcion = "Stock inicial al dar de alta producto",
                                Activo = true,
                                UsuarioRegistro = idUsuario,
                                FechaRegistro = DateTime.Now
                            });
                        }
                        TempData["Mensaje"] = "Se guardó correctamente el producto.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Create", "Productos");
                    }
                }

                TempData["Mensaje"] = "No se pudo guardar.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create", "Productos");
            }
            catch (Exception)
            {
                TempData["Mensaje"] = "Error al procesar.";
                TempData["Tipo"] = "error";
                return RedirectToAction("Create", "Productos");
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

        /// <summary>Reporte de entradas y salidas de inventario para trazabilidad.</summary>
        [HttpGet]
        public async Task<ActionResult> Movimientos(DateTime? desde, DateTime? hasta, int? idProducto)
        {
            var movimientos = await _Iservices.GetMovimientosInventarioAsync(idProducto, desde, hasta);
            var productos = await _Iservices.GetProductosAsync();
            ViewBag.Productos = productos ?? new List<Productos>();
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.IdProducto = idProducto;
            return View(movimientos ?? new List<MovimientoInventario>());
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
