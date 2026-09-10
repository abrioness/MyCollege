using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Helpers;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class PagoCajaController : Controller
    {
        private readonly IServicesApi _Iservices;
        
        public PagoCajaController(IServicesApi services)
        {
            _Iservices = services;
           
        }
        [Authorize]
        // GET: PagoCajaController
        public async Task<ActionResult> Index(DateTime? fechainicio,DateTime? fechafin)
        {

            var _pagoscaja = await _Iservices.GetPagoCajaAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _turnos = await _Iservices.GetTurnosAsync();
            var _periodo = await _Iservices.GetPeriodoAsync();
            var _tipoMovimiento=await _Iservices.GetTipoMovimientoAsync();
            var _recinto = await _Iservices.GetRecintosAsync();
            var _usuarios = await _Iservices.GetUsuariosAsync() ?? new List<TblUsuarios>();
            //var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            //var _meses = await _Iservices.GetMesesAsync();
            //var _modalidad = await _Iservices.GetModalidadesAsync();
            //var _grados = await _Iservices.GetGradosAsync();
            IQueryable<TblPagoCaja> query = _pagoscaja.AsQueryable();

            var (ini, fin) = ReporteFechaQuery.ResolverRango(Request, fechainicio, fechafin);
            if (ini.HasValue)
                query = query.Where(a => a.FechaRegistro.Date >= ini.Value.Date);
            if (fin.HasValue)
                query = query.Where(a => a.FechaRegistro.Date <= fin.Value.Date);

            var pagosFiltrados = query
                .OrderByDescending(a => a.FechaRegistro)
                .ThenByDescending(a => a.IdPagoCaja)
                .ToList();

            var VieModelPagoCaja = new ColeccionCatalogos
            {
                pagoCajas = pagosFiltrados,
                grados = _grados,
                turnos = _turnos,
                tipoMovimiento = _tipoMovimiento,
                periodo = _periodo,
                recintos = _recinto,
                usuarios = _usuarios,
            };
            if (VieModelPagoCaja == null)
            {
                TempData["Message"] = "No hay Pagos registrados";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Pagos encontrados";
                return View(VieModelPagoCaja);
            }

        }
        [Authorize]
        // GET: PagoCajaController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var reciboPagoCaja = await _Iservices.GetPagoCajaById(id);
            //var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _turnos = await _Iservices.GetTurnosAsync();
            //var _recinto = await _Iservices.GetRecintosAsync();
            var viewModel = new pagoCajasViewModel
            {
                PagosCaja = reciboPagoCaja,
                tipoMovimiento = _tipoMovimiento,
                turnos=_turnos,
                grados = _grados,
                recintos =  (await _Iservices.GetRecintosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdRecinto.ToString(),
                                       Text = r.Recinto,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),

            };

            //if (listrecibos == null)
            //{
            //    return NotFound();
            //}

            return View(viewModel);
        }
        //numeros en letra
        private string NumeroALetras(decimal numero)
        {
            return Humanizer.NumberToWordsExtension.ToWords((long)numero, new System.Globalization.CultureInfo("es"))
                .ToUpper() + " CÓRDOBAS";
        }

        // GET: PagoCajaController/Create
        [Authorize]
        public async Task<ActionResult> Create()
        {
            var recibos = await _Iservices.GetPagoCajaAsync();

            var maxNumero = recibos
                .Where(r => r.Serie == "A")
                .Max(r => (int?)r.NumeroRecibo);

            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 20001;
            var viewmodel = new pagoCajasViewModel
            {
                SiguienteNumero=siguienteNumero,

               gradosSelectListItem = (await _Iservices.GetGradosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdGrado.ToString(),
                                       Text = r.NombreGrado,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                turnosSelectListItem = (await _Iservices.GetTurnosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTurno.ToString(),
                                       Text = r.NombreTurno,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                tipoMovimientoSelectListItem = (await _Iservices.GetTipoMovimientoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoMovimiento.ToString(),
                                       Text = r.Concepto,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),

                periodo = (await _Iservices.GetPeriodoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdPeriodo.ToString(),
                                      Text = r.Periodo.ToString(),
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                recintos = (await _Iservices.GetRecintosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdRecinto.ToString(),
                                       Text = r.Recinto.ToString(),
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),

                DetalleItems = new List<DetallePagoCajaItem>(),
                CategoriasProducto = (await _Iservices.GetCategoriaProductoAsync()).Where(c => c.Activo).ToList(),
                Productos = (await _Iservices.GetProductosAsync()).Where(p => p.Activo).ToList(),

            };

            return View(viewmodel);
        }
        [Authorize]
        // POST: PagoCajaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Prefix = "PagosCaja")] TblPagoCaja pagoscaja,
            [FromForm] List<DetallePagoCajaItem>? DetalleItems,
            [FromForm] int idAlumno = 0,
            [FromForm] int semestreRifa = 0)
        {
            var buscarIdGuardado = await _Iservices.GetPagoCajaAsync() ?? new List<TblPagoCaja>();
            var buscarperiodo = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodo = buscarperiodo
                .FirstOrDefault(r => r.Periodo == DateTime.Now.Year && r.Activo && r.Actual)
                ?? buscarperiodo.FirstOrDefault(r => r.Activo);
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (buscarIdGuardado.Any(r => r.NumeroRecibo == pagoscaja.NumeroRecibo && r.Serie == "A" && r.Activo))
            {
                TempData["Mensaje"] = "El número de Recibo ya Existe.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }

            var itemsValidos = (DetalleItems ?? new List<DetallePagoCajaItem>())
                .Where(x => x.IdProducto > 0 && x.Cantidad > 0)
                .GroupBy(x => x.IdProducto)
                .Select(g => new DetallePagoCajaItem { IdProducto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToList();

            var lineasProducto = new List<(int Cantidad, string Nombre)>();
            if (itemsValidos.Count > 0)
            {
                decimal totalCalculado = 0m;
                foreach (var item in itemsValidos)
                {
                    var producto = await _Iservices.GetProductoByIdAsync(item.IdProducto);
                    if (producto == null)
                    {
                        TempData["Mensaje"] = $"No se encontró el producto con Id {item.IdProducto}.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create");
                    }
                    if (producto.StockActual < item.Cantidad)
                    {
                        TempData["Mensaje"] = $"Stock insuficiente para «{producto.NombreProducto}». Disponible: {producto.StockActual}.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create");
                    }
                    totalCalculado += producto.PrecioParaVenta() * item.Cantidad;
                    lineasProducto.Add((item.Cantidad, producto.NombreProducto ?? $"Producto {item.IdProducto}"));
                }
                pagoscaja.Monto = totalCalculado;
            }

            if (pagoscaja.Monto <= 0)
            {
                TempData["Mensaje"] = "Indique el monto a pagar o agregue productos al recibo.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }

            try
            {
                pagoscaja.UsuarioRegistro = idUsuario;
                pagoscaja.Activo = true;
                pagoscaja.FechaRegistro = DateTime.Now;
                if (pagoscaja.IdPeriodo <= 0 && periodo != null)
                    pagoscaja.IdPeriodo = periodo.IdPeriodo;
                if (lineasProducto.Count > 0)
                {
                    pagoscaja.Descripcion = EstadoCuentaCalculoHelper.CombinarDescripcionVisible(
                        pagoscaja.Descripcion,
                        EstadoCuentaCalculoHelper.TextoLineasProducto(lineasProducto));
                }
                pagoscaja.Descripcion = EstadoCuentaCalculoHelper.AdjuntarMarcaAlumno(pagoscaja.Descripcion, idAlumno);

                var tiposMovCaja = await _Iservices.GetTipoMovimientoAsync() ?? new List<CatTipoMovimiento>();
                var idsRifaCaja = EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMovCaja, "rifa", "rifas");
                idsRifaCaja.Add(EstadoCuentaCalculoHelper.TipoRifa);
                if (idsRifaCaja.Contains(pagoscaja.Concepto))
                {
                    TblAlumno alumnoRifa = idAlumno > 0
                        ? await _Iservices.GetAlumnoIdAsync(idAlumno) ?? new TblAlumno { Nombre = pagoscaja.Nombre }
                        : new TblAlumno { Nombre = pagoscaja.Nombre };
                    var pagosRifaTodos = alumnoRifa.IdAlumno > 0
                        ? await _Iservices.GetPagosAsync() ?? new List<TblPago>()
                        : new List<TblPago>();
                    var matriculaRifa = alumnoRifa.IdAlumno > 0 && pagoscaja.IdPeriodo > 0
                        ? await _Iservices.GetMatriculaAlumnoPeriodoAsync(alumnoRifa.IdAlumno, pagoscaja.IdPeriodo)
                        : null;
                    var tiposMatRifa = EstadoCuentaCalculoHelper.TiposPagoMatricula(tiposMovCaja);
                    int anioRifa = buscarperiodo.FirstOrDefault(p => p.IdPeriodo == pagoscaja.IdPeriodo)?.Periodo
                        ?? DateTime.Now.Year;
                    DateTime? fechaIngresoRifa = TrasladoHelper.LeerFechaTraslado(matriculaRifa?.Observaciones)
                        ?? TrasladoHelper.LeerFechaTraslado(alumnoRifa.Observaciones);
                    var evalRifa = EstadoCuentaCalculoHelper.EvaluarRifaAlumno(
                        alumnoRifa.IdAlumno > 0 ? alumnoRifa : null,
                        matriculaRifa,
                        pagosRifaTodos,
                        buscarIdGuardado,
                        idsRifaCaja,
                        tiposMatRifa,
                        tiposMovCaja,
                        pagoscaja.IdPeriodo,
                        anioRifa,
                        pagoscaja.IdRecinto ?? alumnoRifa.IdRecinto,
                        fechaIngresoOverride: fechaIngresoRifa);
                    int siguienteRifa = evalRifa.SiguienteSemestreACobrar();
                    if (siguienteRifa == 0)
                    {
                        TempData["Mensaje"] = evalRifa.MensajeSinCobro();
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create");
                    }
                    semestreRifa = siguienteRifa;
                    pagoscaja.Descripcion = EstadoCuentaCalculoHelper.AdjuntarMarcaRifa(pagoscaja.Descripcion, semestreRifa);
                }

                bool response = await _Iservices.PostPagosCajaAsync(pagoscaja);
                if (!response)
                {
                    TempData["Mensaje"] = "No se procesó el pago en la API.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }

                var listaActualizada = await _Iservices.GetPagoCajaAsync() ?? new List<TblPagoCaja>();
                var reciboGuardado = listaActualizada
                    .Where(r => r.NumeroRecibo == pagoscaja.NumeroRecibo && r.Serie == pagoscaja.Serie && r.Activo)
                    .OrderByDescending(r => r.IdPagoCaja)
                    .FirstOrDefault();

                int idNuevoRecibo = reciboGuardado?.IdPagoCaja ?? 0;
                if (idNuevoRecibo <= 0)
                    idNuevoRecibo = listaActualizada.Max(a => (int?)a.IdPagoCaja) ?? 0;

                if (itemsValidos.Count > 0 && idNuevoRecibo > 0)
                {
                    var erroresInventario = new List<string>();
                    var avisosBajoMinimo = new List<string>();
                    foreach (var item in itemsValidos)
                    {
                        var producto = await _Iservices.GetProductoByIdAsync(item.IdProducto);
                        if (producto == null || producto.StockActual < item.Cantidad)
                        {
                            erroresInventario.Add(producto?.NombreProducto ?? $"Id {item.IdProducto}");
                            continue;
                        }

                        producto.StockActual -= item.Cantidad;
                        producto.ImporteInventario = producto.StockActual * producto.CostoUnitario;
                        producto.UsuarioActualiza = idUsuario;
                        producto.FechaActualiza = DateTime.Now;

                        var (ok, detalle) = await _Iservices.UpdateProductoAsync(producto);
                        if (!ok)
                        {
                            erroresInventario.Add($"{producto.NombreProducto}: {detalle ?? "no se pudo actualizar stock"}");
                            continue;
                        }

                        await _Iservices.PostMovimientoInventarioAsync(new MovimientoInventario
                        {
                            IdProducto = item.IdProducto,
                            TipoMovimiento = TipoMovimientoInventario.Salida,
                            Cantidad = item.Cantidad,
                            FechaMovimiento = DateTime.Now,
                            ReferenciaDocumento = $"RECIBO-{idNuevoRecibo}",
                            Descripcion = "Venta recibo caja varios",
                            Activo = true,
                            UsuarioRegistro = idUsuario,
                            FechaRegistro = DateTime.Now
                        });

                        if (InventarioAlertaHelper.EstaBajoMinimo(producto))
                            avisosBajoMinimo.Add(InventarioAlertaHelper.TextoAlerta(producto));
                    }

                    if (erroresInventario.Count > 0)
                    {
                        TempData["Mensaje"] = "Pago registrado, pero hubo problemas al descontar inventario: " + string.Join("; ", erroresInventario);
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Details", "PagoCaja", new { id = idNuevoRecibo });
                    }

                    if (avisosBajoMinimo.Count > 0)
                    {
                        TempData["Mensaje"] = "Pago registrado. Inventario bajo, reponer: " + string.Join("; ", avisosBajoMinimo);
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Details", "PagoCaja", new { id = idNuevoRecibo });
                    }
                }

                TempData["Mensaje"] = "Se procesó correctamente el pago.";
                TempData["Tipo"] = "success";
                return RedirectToAction("Details", "PagoCaja", new { id = idNuevoRecibo });
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al procesar: " + ex.Message;
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }
        }
        //public async Task<ActionResult> EstadoCuenta()
        //{

        //var _pagosCaja = await _Iservices.GetPagoCajaAsync();            
        //var _turnos = await _Iservices.GetTurnosAsync();           
        //var _periodo = await _Iservices.GetPeriodoAsync();
        //var _grados = await _Iservices.GetGradosAsync();


        //var VieModelEstadoCuenta = new ColeccionCatalogos
        //{
        //    pagoCajas = _pagosCaja,
        //    turnos = _turnos,                
        //    periodo = _periodo,
        //    grados = _grados

        //};
        //if (VieModelEstadoCuenta == null)
        //{
        //    TempData["Message"] = "No hay estado de cuenta registradas";
        //    TempData["Tipo"] = "warning";

        //    return View("NotFound"); // Redirige a una vista de error o no encontrado
        //}
        //else
        //{
        //    //TempData["Message"] = "Estado de Cuenta encontradas";
        //    //TempData["Tipo"] = "success";
        //    return View(VieModelEstadoCuenta);
        //}

        //}
        [Authorize]
        public async Task<ActionResult> EstadoCuenta()
        {
            var pagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            var alumnos = (await _Iservices.GetAlumnosAsync())?.Where(a => a.Activo != false).ToList()
                ?? new List<TblAlumno>();
            var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoRef = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos);
            if (periodoRef == null)
            {
                TempData["Message"] = "No hay período lectivo activo configurado.";
                return View(new EstadoCuentaViewModel
                {
                    MensajePeriodo = "No hay período lectivo activo. Configure un período en catálogo."
                });
            }

            int idPeriodoRef = periodoRef.IdPeriodo;
            var matriculasCiclo = (await _Iservices.GetMatriculasAsync() ?? new List<TblMatricula>())
                .Where(m => m.Activo)
                .ToList();

            int mesReq = EstadoCuentaSolvenciaHelper.ObtenerMesMensualidadRequerido();
            var tiposMov = await _Iservices.GetTipoMovimientoAsync() ?? new List<CatTipoMovimiento>();
            var pagosCaja = await _Iservices.GetPagoCajaAsync() ?? new List<TblPagoCaja>();
            var filas = EstadoCuentaCalculoHelper.ConstruirFilas(
                alumnos,
                pagos,
                await _Iservices.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>(),
                await _Iservices.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>(),
                matriculasCiclo,
                await _Iservices.GetGradosAsync() ?? new List<Grados>(),
                await _Iservices.GetRecintosAsync() ?? new List<Recintos>(),
                await _Iservices.GetMesesAsync() ?? new List<TblCatMeses>(),
                idPeriodoRef,
                periodoRef.Periodo,
                periodos,
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "mensualidad"),
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "rifa", "rifas"),
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "promoc"),
                pagosCaja,
                tiposMov);

            return View(new EstadoCuentaViewModel
            {
                AnioPeriodoReferencia = periodoRef.Periodo,
                MesMensualidadRequerido = mesReq,
                Filas = filas,
                MensajePeriodo = $"Período {periodoRef.Periodo}."
            });
        }

        [Authorize(Roles = "Admin,UserSystem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AnularRecibo(int id, DateTime? fechainicio, DateTime? fechafin)
        {
            TblPagoCaja recibo;
            try
            {
                recibo = await _Iservices.GetPagoCajaById(id);
            }
            catch
            {
                TempData["Mensaje"] = "Recibo de caja no encontrado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            if (!recibo.Activo)
            {
                TempData["Mensaje"] = $"El recibo {recibo.NumeroRecibo} ya está anulado.";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            recibo.Activo = false;
            recibo.UsuarioActualizo = idUsuario;
            recibo.FechaActualizo = DateTime.Now;

            if (await _Iservices.UpdatePagoCaja(recibo))
            {
                TempData["Mensaje"] = $"Recibo de caja {recibo.NumeroRecibo} anulado correctamente.";
                TempData["Tipo"] = "success";
            }
            else
            {
                TempData["Mensaje"] = $"No se pudo anular el recibo {recibo.NumeroRecibo}. Revise la conexión con la API.";
                TempData["Tipo"] = "warning";
            }

            return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
        }

        // GET: PagoCajaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PagoCajaController/Edit/5
        [Authorize]
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

        // GET: PagoCajaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PagoCajaController/Delete/5
        [Authorize]
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
