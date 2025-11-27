using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebColegio.Models;
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
        // GET: ReciboEgresoController
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {
            var _egresos = await _Iservices.GetEgresoAsync();           
            var _periodo = await _Iservices.GetPeriodoAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _recinto = await _Iservices.GetRecintosAsync();

            var query = _egresos;
            if (fechainicio.HasValue)
            {
                query = _egresos.Where(a => a.FechaRegistro >= fechainicio.Value).ToList();
            }
            if (fechafin.HasValue)
            {
                query = _egresos.Where(a => a.FechaRegistro <= fechafin.Value).ToList();
            }

            var VieModelEgresado = new ColeccionCatalogos
            {
                egresos = query,               
                tipoMovimiento = _tipoMovimiento,
                periodo = _periodo,
                recintos=_recinto
                
            };
            if (VieModelEgresado == null)
            {
                TempData["Message"] = "No existen registros";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }

            return View(VieModelEgresado);
        }

        // GET: ReciboEgresoController/Details/5
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
                

            };

            //if (listrecibos == null)
            //{
            //    return NotFound();
            //}

            return View(viewModel);
        }

        // GET: ReciboEgresoController/Create
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
                    egresos.IdPeriodo = periodo.IdPeriodo;
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
