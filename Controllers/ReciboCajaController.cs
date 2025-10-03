using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class ReciboCajaController : Controller
    {
        private readonly IServicesApi _Iservices;
        public ReciboCajaController(IServicesApi services)
        {
            _Iservices = services;
        }

        // GET: ReciboCajaController
        public async Task<ActionResult> Index()
        {
            var _recibocaja  = await _Iservices.GetRecibosCajaAsync();            
            var _grados = await _Iservices.GetGradosAsync();
            var _usuario= await _Iservices.GetUsuariosAsync();
            var _pagos = await _Iservices.GetPagosAsync();
            //var _estadoPago = await _Iservices.GetEstadoPagoAsync();
            

            var VieModelReciboCaja = new ColeccionCatalogos
            {
                reciboCajas = _recibocaja,               
                grados = _grados,
                usuarios = _usuario,
                pagos = _pagos,
                //estadoPagos=_estadoPago
            };

            return View(VieModelReciboCaja);
        }
        // GET: ReciboCaja/Imprimir/5
        public async Task<IActionResult> Imprimir(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reciboCaja = await  _Iservices.GetReciboCajaById(id.Value);


            if (reciboCaja == null)
            {
                return NotFound();
            }

            return View(reciboCaja);
        }

        // GET: ReciboCajaController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            
            var listrecibos = await _Iservices.GetReciboCajaById(id);
            var viewModel = new ReciboCajaViewModel
            {
                ReciboCaja = listrecibos,
                

                gradosSelectListItem = (await _Iservices.GetGradosAsync())
        .Select(r => new SelectListItem
        {
            Value = r.IdGrado.ToString(),
            Text = r.NombreGrado
        }).ToList(),
                
            };

            if (listrecibos == null)
            {
                return NotFound();
            }

            return View(viewModel);
            
        }

        // GET: ReciboCajaController/Create
        public async Task<ActionResult> Create()
        {
            var recibos = await _Iservices.GetRecibosCajaAsync();

            var maxNumero = recibos
                .Where(r => r.Serie == "A")
                .Max(r => (int?)r.NumeroRecibo);

            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 10001;

            var viewmodel = new ReciboCajaViewModel
            {
                SiguienteNumero = siguienteNumero,

                alumnosSelectListItem = (await _Iservices.GetAlumnosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdAlumno.ToString(),
                                       Text = r.Nombre + r.Apellido,
                                       
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                pagosSelectListItem = (await _Iservices.GetPagosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdPago.ToString(),
                                       Text = r.Concepto,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                usuariosSelectListItem = (await _Iservices.GetUsuariosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdUsuario.ToString(),
                                       Text = r.NombreUsuario,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                gradosSelectListItem= (await _Iservices.GetGradosAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdGrado.ToString(),
                                       Text = r.NombreGrado,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),

            };

            return View(viewmodel);
        }

        // POST: ReciboCajaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ReciboCajaViewModel recibo)
        {
            bool response = false;
            
            //bool validarDuplicado = false;
            try
            {
                //validarDuplicado = await _Iservices.ValidarNotas(notas.IdAsignatura, notas.IdPeriodo, notas.IdAlumno);
                //if (validarDuplicado == true)
                //{
                //    TempData["Mensaje"] = "El Alumno ya Posee un Registro de Nota con la Asignatura Seleccionada.";
                //    TempData["Tipo"] = "warning";
                //    return RedirectToAction("Create");
                //}
              
                if (recibo != null)
                {

                    response = await _Iservices.PostReciboCajaAsync(recibo.ReciboCaja);
                    if (response)
                    {
                        TempData["Mensaje"] = "Se registro recibo de caja Correctamente.";
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

        // GET: ReciboCajaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReciboCajaController/Edit/5
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

        // GET: ReciboCajaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReciboCajaController/Delete/5
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
