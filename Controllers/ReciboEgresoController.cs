using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebColegio.Models;
using WebColegio.Helpers;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class ReciboEgresoController : Controller
    {
        private readonly IServicesApi _Iservices;

        public ReciboEgresoController(IServicesApi services)
        {
            _Iservices = services;

        }
        [Authorize]
        // GET: ReciboEgresoController
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {
            var _egresos = await _Iservices.GetEgresoAsync();           
            var _periodo = await _Iservices.GetPeriodoAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _recinto = await _Iservices.GetRecintosAsync();
            var _usuarios = await _Iservices.GetUsuariosAsync() ?? new List<TblUsuarios>();

            IQueryable<TblEgreso> query = _egresos.AsQueryable();

            var (ini, fin) = ReporteFechaQuery.ResolverRango(Request, fechainicio, fechafin);
            if (ini.HasValue)
                query = query.Where(a => a.FechaRegistro.Date >= ini.Value.Date);
            if (fin.HasValue)
                query = query.Where(a => a.FechaRegistro.Date <= fin.Value.Date);

            var egresosFiltrados = query
                .OrderByDescending(a => a.FechaRegistro)
                .ThenByDescending(a => a.IdEgreso)
                .ToList();

            var VieModelEgresado = new ColeccionCatalogos
            {
                egresos = egresosFiltrados,               
                tipoMovimiento = _tipoMovimiento,
                periodo = _periodo,
                recintos = _recinto,
                usuarios = _usuarios
            };
            if (VieModelEgresado == null)
            {
                TempData["Message"] = "No hay egresos registrados";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Pagos de Egresos encontrados";
                return View(VieModelEgresado);
            }
        }

        // GET: ReciboEgresoController/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int id)
        {
            var reciboPagoCaja = await _Iservices.GetEgresoCajaById(id);
            //var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _periodo = await _Iservices.GetPeriodoAsync();
            //var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var viewModel = new EgresoViewModel
            {
                Egresos = reciboPagoCaja,
                tipoMovimiento = _tipoMovimiento, 
                recintos=(await _Iservices.GetRecintosAsync())
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

        // GET: ReciboEgresoController/Create
        [Authorize]
        public async Task<ActionResult> Create()
        {
            var recibos = await _Iservices.GetEgresoAsync();

            var maxNumero = recibos
                .Where(r => r.Serie == "A")
                .Max(r => (int?)r.NumeroRecibo);

            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 30001;
            var viewmodel = new EgresoViewModel
            {
                SiguienteNumero = siguienteNumero,

               
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
                recintos=(await _Iservices.GetRecintosAsync())
                .Select(r=>new SelectListItem
                {
                    Value=r.IdRecinto.ToString(),
                    Text=r.Recinto.ToString()
                }).ToList(),



            };

            return View(viewmodel);
        }

        // POST: ReciboEgresoController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TblEgreso egresos)
        {
            bool response = false;
            bool validarDuplicado = false;
            //int mensualidad = 640;
            //int total = 0;
          
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var buscarIdGuardado = await _Iservices.GetEgresoAsync();
            var buscarperiodo = await _Iservices.GetPeriodoAsync();
            var periodo = buscarperiodo.Where(r => r.Periodo == DateTime.Now.Year && r.Activo == true && r.Actual == true).FirstOrDefault();
            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroRecibo == egresos.NumeroRecibo && r.Serie == "A" && r.Activo==true);
            if(validarDuplicado)
            {
                TempData["Mensaje"] = "El número de Recibo ya Existe.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }
            try
            {

                //if (numRecibo==null)
                //{
                //    TempData["Mensaje"] = "El numero de Recibo ya existe.";
                //}
                if (egresos != null)
                {
                    //var userId = _userManager.GetUserId(User); // o UserManager.GetUserId(User)
                    egresos.UsuarioRegistro = idUsuario;
                    egresos.Activo = true;
                    egresos.FechaRegistro = DateTime.Now;
                   
                    //await _Iservices.InsertarPagoAsync(nuevoPago);
                    response = await _Iservices.PostEgresoAsync(egresos);
                    if (response)
                    {

                        var idPag = buscarIdGuardado.Max(a => a.IdEgreso);
                        TempData["Mensaje"] = "Se Proceso Correctamente el Pago.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Details", "ReciboEgreso", new { id = idPag +1});
                    }
                    else
                    {
                        TempData["Mensaje"] = "No se proceso el Pago.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create");

                    }
                }
                return NoContent();


            }
            catch
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin,UserSystem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AnularRecibo(int id, DateTime? fechainicio, DateTime? fechafin)
        {
            TblEgreso egreso;
            try
            {
                egreso = await _Iservices.GetEgresoCajaById(id);
            }
            catch
            {
                TempData["Mensaje"] = "Recibo de egreso no encontrado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            if (!egreso.Activo)
            {
                TempData["Mensaje"] = $"El recibo {egreso.NumeroRecibo} ya está anulado.";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            egreso.Activo = false;
            egreso.UsuarioActualizo = idUsuario;
            egreso.FechaActualizo = DateTime.Now;

            if (await _Iservices.UpdateEgreso(egreso))
            {
                TempData["Mensaje"] = $"Recibo de egreso {egreso.NumeroRecibo} anulado correctamente.";
                TempData["Tipo"] = "success";
            }
            else
            {
                TempData["Mensaje"] = $"No se pudo anular el recibo {egreso.NumeroRecibo}. Revise la conexión con la API.";
                TempData["Tipo"] = "warning";
            }

            return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
        }

        // GET: ReciboEgresoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReciboEgresoController/Edit/5
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

        // GET: ReciboEgresoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReciboEgresoController/Delete/5
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
