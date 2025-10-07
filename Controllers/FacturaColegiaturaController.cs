using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class FacturaColegiaturaController : Controller
    {
        private readonly IServicesApi _Iservices;
        public FacturaColegiaturaController(IServicesApi services)
        {
            _Iservices = services;
        }

        // GET: FacturaColegiaturaController
        public async Task<ActionResult> Index()
        {
            var _tipoColegiatura = await _Iservices.GetTipoColegiatuuraAsync();
            var _estadoPago = await _Iservices.GetEstadoPagoAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _facturacion = await _Iservices.GetFacturacionAsync();

            var viewModel = new ColeccionCatalogos
            {

                tipoColegiaturas = _tipoColegiatura,
                estadoPagos=_estadoPago,
                alumno=_alumnos,
                facturacion=_facturacion
        

            };
            return View(viewModel);
            }
        // GET: FacturaColegiaturaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FacturaColegiaturaController/Create
        public async Task<ActionResult> Create()
        {
            var viewModel = new FacturacionViewModel
            {

        tipoColegiaturas = (await _Iservices.GetTipoColegiatuuraAsync())
        .Select(r => new SelectListItem
        {
            Value = r.IdTipoColegiatura.ToString(),
            Text = r.NombreConcepto
        }).ToList(),
        estadoPagos = (await _Iservices.GetEstadoPagoAsync())
        .Select(r => new SelectListItem
        {
            Value = r.IdEstadoPago.ToString(),
            Text = r.EstadoPago
        }).ToList(),

            };
            return View(viewModel);
        }

        // POST: FacturaColegiaturaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FacturaColegiatura facturacion)
        {
            bool response = false;
            bool validarDuplicado = false;
            try
            {
                //validarDuplicado = await _Iservices.ValidarNotas(notas.IdAsignatura, notas.IdPeriodo, notas.IdAlumno);
                //if (validarDuplicado == true)
                //{
                //    TempData["Mensaje"] = "El Alumno ya Posee un Registro de Nota con la Asignatura Seleccionada.";
                //    TempData["Tipo"] = "warning";
                //    return RedirectToAction("Create");
                //}

                if (facturacion != null)
                {

                    response = await _Iservices.PostFacturacionAsync(facturacion);
                    if (response)
                    {
                        TempData["Mensaje"] = "Se registro el pago correctamente.";
                        return RedirectToAction(nameof(Index));
                    }

                }
                return NoContent();
            }
            catch
            {
                return View();
            }
        }

        // GET: FacturaColegiaturaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FacturaColegiaturaController/Edit/5
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

        // GET: FacturaColegiaturaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }


        //arqueo
        public async Task<IActionResult> ArqueoCaja(int id)
        {
            //var arqueo = await _Iservices.GetArqueoById(id); // Llama a tu API
            return View(); // Retorna el ViewModel a la vista
        }

        // POST: FacturaColegiaturaController/Delete/5
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
