using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
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
        // GET: PagoCajaController
        public async Task<ActionResult> Index()
        {
            var _pagoscaja = await _Iservices.GetPagoCajaAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _tipoRecibo = await _Iservices.GetTipoReciboAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var _meses = await _Iservices.GetMesesAsync();
            //var _modalidad = await _Iservices.GetModalidadesAsync();
            //var _grados = await _Iservices.GetGradosAsync();


            var VieModelPagoCaja = new ColeccionCatalogos
            {
                pagoCajas = _pagoscaja,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses = _meses
                //modalidades = _modalidad,
                //grados = _grados    

            };
            if (VieModelPagoCaja == null)
            {
                TempData["Message"] = "No existen registros";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            
                return View(VieModelPagoCaja);
            
        }

        // GET: PagoCajaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PagoCajaController/Create
        public async Task<ActionResult> Create()
        {
            var viewmodel = new pagoCajasViewModel
            {

                tipoMovimientoSelectListItem = (await _Iservices.GetTipoMovimientoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoMovimiento.ToString(),
                                       Text = r.Concepto,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                tipoRecibosSelectListItem = (await _Iservices.GetTipoReciboAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoRecibo.ToString(),
                                       Text = r.TipoRecibo,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                metodoPagoSelectListItem = (await _Iservices.GetMetodoPagoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdMetodoPago.ToString(),
                                       Text = r.MetodoPago,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                meses = await _Iservices.GetMesesAsync(),
                periodo = await _Iservices.GetPeriodoAsync(),
                gradosSelectListItem = (await _Iservices.GetGradosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdGrado.ToString(),
                    Text = r.NombreGrado,
                    //Selected = r.IdPregunta == respuestas.IdPregunta
                }).ToList(),


            };

            return View(viewmodel);
        }

        // POST: PagoCajaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(pagoCajasViewModel pagoscaja)
        {
            bool response = false;
            bool validarDuplicado = false;
            int mensualidad = 640;
            int total = 0;
            try
            {
                
                if (pagoscaja != null)
                {

                    //await _Iservices.InsertarPagoAsync(nuevoPago);
                    response = await _Iservices.PostPagosCajaAsync(pagoscaja.PagosCaja);
                    if (response)
                    {
                        TempData["Mensaje"] = "Se Proceso Correctamente el Pago.";
                        return RedirectToAction("Create");
                    }
                    else
                    {
                        TempData["Mensaje"] = "No se proceso el Pago.";
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

        // GET: PagoCajaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PagoCajaController/Edit/5
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
