using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
            //var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            //var _meses = await _Iservices.GetMesesAsync();
            //var _modalidad = await _Iservices.GetModalidadesAsync();
            //var _grados = await _Iservices.GetGradosAsync();
            IQueryable<TblPagoCaja> query = _pagoscaja.AsQueryable();

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
            var pagosFiltrados = query.ToList();

            var VieModelPagoCaja = new ColeccionCatalogos
            {
                pagoCajas = pagosFiltrados,
                grados = _grados,
                turnos = _turnos,
                tipoMovimiento = _tipoMovimiento,
                periodo = _periodo,
                recintos = _recinto,

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



            };

            return View(viewmodel);
        }
        [Authorize]
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
            var buscarperiodo = await _Iservices.GetPeriodoAsync();
            var periodo = buscarperiodo.Where(r=>r.Periodo==DateTime.Now.Year && r.Activo==true && r.Actual==true).FirstOrDefault();
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroRecibo == pagoscaja.NumeroRecibo  && r.Serie == "A" && r.Activo == true);
            if (validarDuplicado)
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
                if (pagoscaja != null)
                {

                   

                    //var userId = _userManager.GetUserId(User); // o UserManager.GetUserId(User)
                    pagoscaja.UsuarioRegistro = idUsuario;
                    pagoscaja.Activo = true;
                    pagoscaja.FechaRegistro = DateTime.Now;
                    pagoscaja.IdPeriodo = periodo.IdPeriodo;


                    //await _Iservices.InsertarPagoAsync(nuevoPago);
                    response = await _Iservices.PostPagosCajaAsync(pagoscaja);
                    if (response)
                    {
                       
                        var idPag = buscarIdGuardado.Max(a=>a.IdPagoCaja);
                        TempData["Mensaje"] = "Se Proceso Correctamente el Pago.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Details","PagoCaja", new {id= idPag+1 });
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

            var _pagos = await _Iservices.GetPagosAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _tipoRecibo = await _Iservices.GetTipoReciboAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var _meses = await _Iservices.GetMesesAsync();
            var _notas = await _Iservices.GetNotasAsync();
            var _periodo = await _Iservices.GetPeriodoAsync();
            var _grados = await _Iservices.GetGradosAsync();


            var VieModelEstadoCuenta = new ColeccionCatalogos
            {
                pagos = _pagos,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses = _meses,
                periodo = _periodo,
                grados = _grados

            };
            if (VieModelEstadoCuenta == null)
            {
                TempData["Message"] = "No hay estado de cuenta registradas";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Estado de Cuenta encontradas";
                return View(VieModelEstadoCuenta);
            }
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
