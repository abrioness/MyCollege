using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
            var _grados = await _Iservices.GetGradosAsync();
            var _turnos = await _Iservices.GetTurnosAsync();
            var _periodo = await _Iservices.GetPeriodoAsync();
            var _tipoMovimiento=await _Iservices.GetTipoMovimientoAsync();
            //var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            //var _meses = await _Iservices.GetMesesAsync();
            //var _modalidad = await _Iservices.GetModalidadesAsync();
            //var _grados = await _Iservices.GetGradosAsync();


            var VieModelPagoCaja = new ColeccionCatalogos
            {
                pagoCajas = _pagoscaja,
                grados = _grados,
                turnos = _turnos,
                tipoMovimiento = _tipoMovimiento,
                periodo = _periodo,
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
        public async Task<ActionResult> Details(int id)
        {
            var reciboPagoCaja = await _Iservices.GetPagoCajaById(id);
            //var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _turnos = await _Iservices.GetTurnosAsync();
            //var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var viewModel = new pagoCajasViewModel
            {
                PagosCaja = reciboPagoCaja,
                tipoMovimiento = _tipoMovimiento,
                turnos=_turnos,
                grados = _grados

            };

            //if (listrecibos == null)
            //{
            //    return NotFound();
            //}

            return View(viewModel);
        }

        // GET: PagoCajaController/Create
        public async Task<ActionResult> Create()
        {
            var recibos = await _Iservices.GetPagoCajaAsync();

            var maxNumero = recibos
                .Where(r => r.Serie == "A")
                .Max(r => (int?)r.NumeroRecibo);

            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 10001;
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



            };

            return View(viewmodel);
        }

        // POST: PagoCajaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TblPagoCaja pagoscaja)
        {
            bool response = false;
            bool validarDuplicado = false;
            //int mensualidad = 640;
            //int total = 0;
            var buscarIdGuardado = await _Iservices.GetPagoCajaAsync();
            
            try
            {

                //if (numRecibo==null)
                //{
                //    TempData["Mensaje"] = "El numero de Recibo ya existe.";
                //}
                if (pagoscaja != null)
                {
                    //var userId = _userManager.GetUserId(User); // o UserManager.GetUserId(User)
                    pagoscaja.UsuarioRegistro =1;
                    pagoscaja.Activo = true;
                    pagoscaja.FechaRegistro = DateTime.Now;
                    //await _Iservices.InsertarPagoAsync(nuevoPago);
                    response = await _Iservices.PostPagosCajaAsync(pagoscaja);
                    if (response)
                    {
                       
                        var idPag = buscarIdGuardado.Max(a=>a.IdPagoCaja);
                        TempData["Mensaje"] = "Se Proceso Correctamente el Pago.";
                        
                        return RedirectToAction("Details","PagoCaja", new {id= idPag });
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
        public async Task<ActionResult> EstadoCuenta()
        {

            var _pagosCaja = await _Iservices.GetPagoCajaAsync();            
            var _turnos = await _Iservices.GetTurnosAsync();           
            var _periodo = await _Iservices.GetPeriodoAsync();
            var _grados = await _Iservices.GetGradosAsync();


            var VieModelEstadoCuenta = new ColeccionCatalogos
            {
                pagoCajas = _pagosCaja,
                turnos = _turnos,                
                periodo = _periodo,
                grados = _grados

            };
            if (VieModelEstadoCuenta == null)
            {
                TempData["Message"] = "No hay estado de cuenta registradas";
                TempData["Tipo"] = "warning";

                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                //TempData["Message"] = "Estado de Cuenta encontradas";
                //TempData["Tipo"] = "success";
                return View(VieModelEstadoCuenta);
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
