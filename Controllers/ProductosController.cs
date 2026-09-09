using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebColegio.Models;
using WebColegio.Helpers;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IServicesApi _Iservices;
        private readonly IMenuPermisoService _menuPermiso;
        public ProductosController (IServicesApi servicesApi, IMenuPermisoService menuPermiso)
        {
            _Iservices = servicesApi;
            _menuPermiso = menuPermiso;
        }

        // GET: ProductosController
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var _productos = await _Iservices.GetProductosAsync();
            var _movInvebtario = await _Iservices.GetMovInventarioAsync();
            var _categoriaProducto = await _Iservices.GetCategoriaProductoAsync();
            var _recintos = await _Iservices.GetRecintosAsync();
            var _proveedores = await _Iservices.GetProveedoresAsync() ?? new List<CatProveedor>();
            var _usuarioId = await _Iservices.GetUsuarioIdAsync(idUsuario);


            IQueryable<Productos> query = _productos.AsQueryable();

            var (ini, fin) = ReporteFechaQuery.ResolverRango(Request, fechainicio, fechafin);
            if (ini.HasValue)
                query = query.Where(a => a.FechaRegistro.Date >= ini.Value.Date);
            if (fin.HasValue)
                query = query.Where(a => a.FechaRegistro.Date <= fin.Value.Date);

            var productosFiltrados = query
                .OrderByDescending(a => a.IdProducto)
                .ThenByDescending(a => a.FechaRegistro)
                .ToList();

            var movimientos = await _Iservices.GetMovimientosInventarioAsync(null, null, null)
                ?? new List<MovimientoInventario>();
            var totalesMovimiento = ArmarTotalesMovimientoInventario(productosFiltrados, movimientos);

            int idRol = _usuarioId?.IdRol ?? 0;
            bool puedeConsultar = User.IsInRole("Admin") || User.IsInRole("Cajero") || User.IsInRole("UserSystem")
                || User.IsInRole("Secretaria") || idRol is 1 or 2 or 5 or 6;
            if (!puedeConsultar)
                return RedirectToAction("SinPermiso", "Login");

            return View(new ColeccionCatalogos
            {
                producto = productosFiltrados,
                categoriasProducto = _categoriaProducto,
                movinventario = _movInvebtario,
                recintos = _recintos ?? new List<Recintos>(),
                proveedores = _proveedores,
                TotalesMovimientoPorProducto = totalesMovimiento,
            });
        }

        // GET: ProductosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductosController/Create
        [Authorize]
        public async Task<ActionResult> Create()
        {
            if (!await PuedeAgregarInventarioAsync())
                return RedirectToAction("SinPermiso", "Login");

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
                SiguienteCodigo = siguienteCodigo,
                tblproducto = new Productos { StockMinimo = InventarioAlertaHelper.StockMinimoPorDefecto }
            };
            await CargarListasProducto(viewmodel);
            return View(viewmodel);
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Json(new List<object>());

            var term = q.Trim();
            var productos = (await _Iservices.GetProductosAsync() ?? new List<Productos>())
                .Where(p => p.Activo && (
                    (!string.IsNullOrWhiteSpace(p.CodigoBarra) && p.CodigoBarra.Contains(term, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(p.NombreProducto) && p.NombreProducto.Contains(term, StringComparison.OrdinalIgnoreCase))))
                .Take(20)
                .Select(p => new
                {
                    id = p.IdProducto,
                    codigo = p.CodigoBarra,
                    nombre = p.NombreProducto,
                    idCategoria = p.IdCateProducto,
                    idRecinto = p.IdRecinto,
                    idMovimiento = p.IdMovInventario,
                    costo = p.CostoUnitario,
                    precioVenta = p.PrecioParaVenta(),
                    stock = p.StockActual,
                    stockMinimo = InventarioAlertaHelper.StockMinimoEfectivo(p.StockMinimo),
                    idProveedor = p.IdProveedor,
                    label = $"{p.CodigoBarra} — {p.NombreProducto} (stock {p.StockActual})"
                })
                .ToList();

            return Json(productos);
        }

        // POST: ProductosController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(productoViewModel producto)
        {
            if (!await PuedeAgregarInventarioAsync())
                return RedirectToAction("SinPermiso", "Login");

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
                producto.tblproducto.StockMinimo = InventarioAlertaHelper.NormalizarStockMinimo(producto.tblproducto.StockMinimo);
                if (producto.tblproducto.PrecioVenta <= 0)
                    producto.tblproducto.PrecioVenta = producto.tblproducto.CostoUnitario;

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
                    existente.IdRecinto = producto.tblproducto.IdRecinto;
                    existente.StockMinimo = InventarioAlertaHelper.NormalizarStockMinimo(
                        producto.tblproducto.StockMinimo > 0 ? producto.tblproducto.StockMinimo : existente.StockMinimo);
                    if (producto.tblproducto.IdProveedor.HasValue && producto.tblproducto.IdProveedor.Value > 0)
                        existente.IdProveedor = producto.tblproducto.IdProveedor;
                    existente.UsuarioActualiza = idUsuario;
                    existente.FechaActualiza = DateTime.Now;
                    var (actualizadoStock, _) = await _Iservices.UpdateProductoAsync(existente);
                    if (actualizadoStock)
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
                        TempData["Mensaje"] = InventarioAlertaHelper.EstaBajoMinimo(existente)
                            ? $"Se sumaron {cantidadEntrante} unidades. Stock actual: {existente.StockActual}. Sigue bajo el mínimo ({InventarioAlertaHelper.StockMinimoEfectivo(existente.StockMinimo)}). Reponer inventario."
                            : $"Se actualizó el producto. Se sumaron {cantidadEntrante} unidades. Stock actual: {existente.StockActual}.";
                        TempData["Tipo"] = InventarioAlertaHelper.EstaBajoMinimo(existente) ? "warning" : "success";
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
                        TempData["Mensaje"] = InventarioAlertaHelper.EstaBajoMinimo(producto.tblproducto)
                            ? $"Producto guardado. El stock ({producto.tblproducto.StockActual}) está en o bajo el mínimo ({producto.tblproducto.StockMinimo}). Reponer inventario."
                            : "Se guardó correctamente el producto.";
                        TempData["Tipo"] = InventarioAlertaHelper.EstaBajoMinimo(producto.tblproducto) ? "warning" : "success";
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
        [Authorize(Roles = "Admin,UserSystem")]
        public async Task<ActionResult> Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            var p = await _Iservices.GetProductoByIdAsync(id);
            if (p == null || p.IdProducto <= 0)
                return NotFound();

            if (p.StockMinimo <= 0)
                p.StockMinimo = InventarioAlertaHelper.StockMinimoPorDefecto;
            var viewmodel = new productoViewModel { tblproducto = p };
            await CargarListasProducto(viewmodel);
            return View(viewmodel);
        }

        // POST: ProductosController/Edit/5
        [Authorize(Roles = "Admin,UserSystem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(productoViewModel producto)
        {
            try
            {
                if (producto?.tblproducto == null || producto.tblproducto.IdProducto <= 0)
                {
                    ModelState.AddModelError("", "Datos de producto no válidos.");
                    return producto != null
                        ? await RepoblarListasProductoEdit(producto)
                        : RedirectToAction(nameof(Index));
                }

                if (!string.IsNullOrWhiteSpace(producto.tblproducto.CodigoBarra))
                {
                    var duplicado = await _Iservices.GetProductoByCodigoYCategoriaAsync(
                        producto.tblproducto.CodigoBarra.Trim(),
                        producto.tblproducto.IdCateProducto);
                    if (duplicado != null && duplicado.IdProducto != producto.tblproducto.IdProducto)
                    {
                        ModelState.AddModelError("", "Ya existe otro producto con el mismo código y categoría.");
                        return await RepoblarListasProductoEdit(producto);
                    }
                }

                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                producto.tblproducto.StockMinimo = InventarioAlertaHelper.NormalizarStockMinimo(producto.tblproducto.StockMinimo);
                if (producto.tblproducto.PrecioVenta <= 0)
                    producto.tblproducto.PrecioVenta = producto.tblproducto.CostoUnitario;
                producto.tblproducto.ImporteInventario = producto.tblproducto.StockActual * producto.tblproducto.CostoUnitario;
                producto.tblproducto.UsuarioActualiza = idUsuario;
                producto.tblproducto.FechaActualiza = DateTime.Now;

                var (ok, apiError) = await _Iservices.UpdateProductosAsync(producto.tblproducto);
                if (ok)
                {
                    TempData["Mensaje"] = InventarioAlertaHelper.EstaBajoMinimo(producto.tblproducto)
                        ? $"Producto actualizado. El stock ({producto.tblproducto.StockActual}) está en o bajo el mínimo ({producto.tblproducto.StockMinimo}). Reponer inventario."
                        : "Producto actualizado correctamente (incluido colegio / recinto).";
                    TempData["Tipo"] = InventarioAlertaHelper.EstaBajoMinimo(producto.tblproducto) ? "warning" : "success";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", apiError ?? "No se pudo actualizar el producto en el servidor.");

                return await RepoblarListasProductoEdit(producto);
            }
            catch (Exception)
            {
                TempData["Mensaje"] = "Error al procesar la edición.";
                TempData["Tipo"] = "error";
                return producto != null
                    ? await RepoblarListasProductoEdit(producto)
                    : RedirectToAction(nameof(Index));
            }
        }

        private async Task CargarListasProducto(productoViewModel vm)
        {
            vm.listCategoriaProducto = (await _Iservices.GetCategoriaProductoAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdCateProducto.ToString(),
                    Text = r.NombreCategoria,
                }).ToList();
            vm.listMovimientoInventario = (await _Iservices.GetMovInventarioAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdMovInventario.ToString(),
                    Text = r.MovimientoInventario,
                }).ToList();
            vm.listRecintos = (await _Iservices.GetRecintosAsync() ?? new List<Recintos>())
                .OrderBy(r => r.Recinto)
                .Select(r => new SelectListItem
                {
                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto,
                }).ToList();
            var proveedores = await _Iservices.GetProveedoresAsync() ?? new List<CatProveedor>();
            var proveedoresVisibles = proveedores.Where(p => p.Activo).ToList();
            if (proveedoresVisibles.Count == 0)
                proveedoresVisibles = proveedores;
            vm.ListaProveedores = proveedoresVisibles
                .OrderBy(p => p.NombreProveedor)
                .Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.NombreProveedor,
                }).ToList();
        }

        private async Task<ViewResult> RepoblarListasProductoEdit(productoViewModel vm)
        {
            await CargarListasProducto(vm);
            return View("Edit", vm);
        }

        /// <summary>Reporte de entradas y salidas de inventario para trazabilidad.</summary>
        [HttpGet]
        public async Task<ActionResult> Movimientos(DateTime? desde, DateTime? hasta, int? idProducto, int? tipoMovimiento)
        {
            var (ini, fin) = ReporteFechaQuery.ResolverRangoPorClaves(Request, desde, hasta, "desde", "hasta");

            int? idProd = idProducto is > 0 ? idProducto : null;
            if (!idProd.HasValue && int.TryParse(Request.Query["idProducto"].FirstOrDefault(), out var idP) && idP > 0)
                idProd = idP;

            int? tipoMov = tipoMovimiento is 1 or 2 ? tipoMovimiento : null;
            if (!tipoMov.HasValue && int.TryParse(Request.Query["tipoMovimiento"].FirstOrDefault(), out var tm) && tm is 1 or 2)
                tipoMov = tm;

            var movimientos = await _Iservices.GetMovimientosInventarioAsync(idProd, ini, fin);
            var lista = (movimientos ?? new List<MovimientoInventario>()).ToList();

            if (ini.HasValue)
                lista = lista.Where(m => m.FechaMovimiento.Date >= ini.Value.Date).ToList();
            if (fin.HasValue)
                lista = lista.Where(m => m.FechaMovimiento.Date <= fin.Value.Date).ToList();
            if (tipoMov.HasValue)
                lista = lista.Where(m => m.TipoMovimiento == tipoMov.Value).ToList();

            lista = lista.OrderByDescending(m => m.FechaMovimiento).ThenByDescending(m => m.IdInventario).ToList();

            var productos = await _Iservices.GetProductosAsync();
            ViewBag.Productos = productos ?? new List<Productos>();
            ViewBag.Desde = ini;
            ViewBag.Hasta = fin;
            ViewBag.IdProducto = idProd;
            ViewBag.TipoMovimiento = tipoMov;
            return View(lista);
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

        private static Dictionary<int, TotalesMovimientoInventario> ArmarTotalesMovimientoInventario(
            IReadOnlyList<Productos> productos,
            IReadOnlyList<MovimientoInventario> movimientos)
        {
            var activos = (movimientos ?? Array.Empty<MovimientoInventario>())
                .Where(m => m.IdProducto > 0 && m.Cantidad > 0);
            var agrupado = activos
                .GroupBy(m => m.IdProducto)
                .ToDictionary(
                    g => g.Key,
                    g => new TotalesMovimientoInventario
                    {
                        Entrada = g.Where(m => m.TipoMovimiento == TipoMovimientoInventario.Entrada).Sum(m => m.Cantidad),
                        Salida = g.Where(m => m.TipoMovimiento == TipoMovimientoInventario.Salida).Sum(m => m.Cantidad)
                    });

            var resultado = new Dictionary<int, TotalesMovimientoInventario>();
            foreach (var p in productos ?? Array.Empty<Productos>())
            {
                if (agrupado.TryGetValue(p.IdProducto, out var tot))
                {
                    resultado[p.IdProducto] = tot;
                    continue;
                }

                int inicial = Math.Max(0, p.ExistenciaInicial);
                int stock = Math.Max(0, p.StockActual);
                resultado[p.IdProducto] = new TotalesMovimientoInventario
                {
                    Entrada = inicial,
                    Salida = Math.Max(0, inicial - stock)
                };
            }

            return resultado;
        }

        private async Task<bool> PuedeAgregarInventarioAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("UserSystem") || User.IsInRole("Secretaria"))
                return true;

            var idRol = IMenuPermisoService.ResolverIdRol(User, HttpContext.Session);
            if (idRol is null or <= 0)
                return false;

            return await _menuPermiso.TieneAsync(idRol.Value, "inventario.agregar");
        }
    }
}
