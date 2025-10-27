using System.Diagnostics.Metrics;
using System.Linq;
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
    public class PagosController : Controller
    {
        private readonly IServicesApi _Iservices;
        public PagosController(IServicesApi services)
        {
            _Iservices = services;
        }
        // GET: PagosController
        public async Task<ActionResult> Index()
        {

            var _pagos = await _Iservices.GetPagosAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _tipoRecibo = await _Iservices.GetTipoReciboAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var _meses = await _Iservices.GetMesesAsync();
            //var _modalidad = await _Iservices.GetModalidadesAsync();
            //var _grados = await _Iservices.GetGradosAsync();


            var VieModelPagos = new ColeccionCatalogos
            {
                pagos=_pagos,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses=_meses
                //modalidades = _modalidad,
                //grados = _grados    

            };
            if (VieModelPagos == null)
            {
                TempData["Message"] = "No hay notas registradas";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Pagos encontrados";
                return View(VieModelPagos);
            }
        }
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
        // GET: PagosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PagosController/Create
        public async Task<ActionResult> Create()
        {
            var viewmodel = new PagosViewModel
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
                gradosSelectListItem=(await _Iservices.GetGradosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdGrado.ToString(),
                    Text = r.NombreGrado,
                    //Selected = r.IdPregunta == respuestas.IdPregunta
                }).ToList(),


            };

            return View(viewmodel);
        }

        // POST: PagosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PagosViewModel pagos, string MesesSeleccionados)//List<int> MesesSeleccionados)
        {
            bool response = false;
            bool validarDuplicado = false;
            int mensualidad = 640;
            int total = 0;
            try
            {
                int periodo= await _Iservices.GetPeriodoAsync().ContinueWith(p=>p.Result.FirstOrDefault(a=>a.Activo && a.Actual)?.IdPeriodo) ?? 0;
                pagos.Pago.IdPeriodo = periodo;
                //int tipoMov = await _Iservices.GetTipoMovimientoAsync().ContinueWith(p => p.Result.FirstOrDefault(a => a.Activo && a.IdTipoMovimiento==pagos.Pago.IdTipoMovimiento)?.IdTipoMovimiento) ?? 0;

                //validarDuplicado = await _Iservices.ValidarNotas(pagos.MesPagado, pagos.IdTipoMovimiento, pagos.IdAlumno);
                //if (ExistePagoDuplicado(pagos))
                //{
                //    TempData["Mensaje"] = "El Alumno ya tiene un registro de pago con el concepto actual.";
                //    TempData["Tipo"] = "warning";
                //    return RedirectToAction("Create");
                //}

                //if(pagos.Pago.IdMes)
                //{

                //}

                if (pagos != null)
                {
                   if (!string.IsNullOrEmpty(MesesSeleccionados))//MesesSeleccionados != null && MesesSeleccionados.Any())
                    {

                        if(BecaCompleta(pagos.Pago.IdAlumno,pagos.Pago.IdPeriodo).Result)
                        {
                            TempData["Mensaje"] = "El Estudiante Posee Beca Completa.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        if (MediaBeca(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo).Result && pagos.Pago.Monto>320)
                        {
                            TempData["Mensaje"] = "El Estudiante Cuenta con Media Beca.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }


                        var ids = MesesSeleccionados.Split(',').Select(int.Parse).ToList();
                        var listpagos = await _Iservices.GetPagosAsync();                      
                        
                        bool aplicoMora = false;
                        bool duplicado = false;
                        //total = ids * mensualidad;
                        //if((ids* mensualidad)=pagos.Pago.Monto)
                        

                        foreach (var idMes in ids)
                        {
                            // 1️⃣ Validar duplicado

                            bool existePago = listpagos.Any(p => p.IdAlumno == pagos.Pago.IdAlumno
                                            && p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento
                                            && p.IdMes == idMes);
                            
                            if (existePago)
                            {
                                // Evita duplicado
                                duplicado = true;

                                TempData["Mensaje"] = $"El mes {idMes} ya fue pagado por este alumno.";
                                TempData["Tipo"] = "warning";
                                continue;
                            }
                            var todosLosMeses = Enumerable.Range(1, 12); // o hasta el mes actual


                            // 2️⃣ Verificar si hay mora (mes anterior sin pagar)
                            //var tienePendientes = listpagos
                            //        .Where(p => p.IdAlumno == pagos.Pago.IdAlumno
                            //                    && p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento
                            //                    && p.IdPeriodo == periodo
                            //                    && p.IdMes < idMes);
                            //for(int a=0;a<12-1;a++)
                            //{
                            //    tienePendientes.FirstOrDefault
                            //}

                            int anioActual = pagos.Pago.IdPeriodo;
                            var mesesPendientes = await _Iservices.GetPagosAsync();
                            var pendientes = mesesPendientes
                            .Where(p => p.IdAlumno == pagos.Pago.IdAlumno &&
                                       p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento &&
                                       p.IdPeriodo == pagos.Pago.IdPeriodo &&
                                       p.IdMes < idMes)
                            .Select(p => p.IdMes).ToList();
                            // 2. Obtener todos los meses que deberían estar pagados
                            var todosMesesRequeridos = Enumerable.Range(1, idMes - 1).ToList();
                            var mesesFaltantes = todosMesesRequeridos.Except(pendientes).ToList();
                            
                             if (mesesFaltantes.Any())
                            {
                                var primerMesFaltante = mesesFaltantes.Min();
                                if (idMes != primerMesFaltante)
                                {
                                    
                                    TempData["Mensaje"] = $"No puede pagar el mes de {Mes(idMes).Result}. Debe pagar primero el mes de {Mes(primerMesFaltante).Result}.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                            }

                            int montoMora = 0;
                            if (mesesFaltantes.Any())
                            {
                                int montoBase = 10; // Obtener de configuración
                                //decimal porcentajeMora = 0.10m;
                                montoMora = mesesFaltantes.Count * (montoBase); //* porcentajeMora);
                            }
                            //else
                            //    {
                            //        pagos.Mora = 0; // Aplica mora fija de 10

                            //        aplicoMora = false;
                            //    }

                                // ejemplo: crear un pago por cada mes
                                var nuevoPago = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    IdMes = idMes,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = pagos.Pago.IdTipoMovimiento,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    IdGrado=pagos.Pago.IdGrado,
                                    IdPeriodo=pagos.Pago.IdPeriodo,
                                    Mora = montoMora,
                                    Monto = pagos.Pago.Monto,
                                    Activo = true,
                                    UsuarioRegistro = 1,
                                    FechaRegistro = DateTime.Now
                                    // otros campos...
                                };

                                //await _Iservices.InsertarPagoAsync(nuevoPago);
                                response = await _Iservices.PostPagosAsync(nuevoPago);
                            }
                        // 4️⃣ Mensaje final
                        if (duplicado)
                        {
                            TempData["Mensaje"] = "Algunos meses ya estaban registrados y fueron omitidos.";
                            TempData["Tipo"] = "warning";
                        }
                        else if (aplicoMora)
                        {
                            TempData["Mensaje"] = "Pago registrado con mora de C$ 10 aplicada.";
                            TempData["Tipo"] = "info";
                        }
                        else
                        {
                            TempData["Mensaje"] = "Pago registrado correctamente.";
                            TempData["Tipo"] = "success";
                        }

                        return RedirectToAction("Create");

                    }
                    else
                    {
                        pagos.Pago.IdMes = 0;
                        response = await _Iservices.PostPagosAsync(pagos.Pago);
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
                   
                    

                }
                return NoContent();
            }
            catch
            {
                return View();
            }
        }

        //Obtener nombre del mes.
        public async Task<string> Mes(int idmes)
        {
            string mes = await _Iservices.GetMesesAsync().ContinueWith(a => a.Result.FirstOrDefault(m => m.IdMes == idmes)?.Mes ?? "No hay Mes");

            return mes;
        }
        
        public async Task<bool>BecaCompleta(int idalumno,int periodo)
        {
            bool becaCompleta = await _Iservices.GetAlumnosAsync().ContinueWith(a => a.Result.FirstOrDefault(b => b.IdAlumno==idalumno && b.BecaCompleta==true && b.IdPeriodo==periodo)?.BecaCompleta ?? false);
            
            if(becaCompleta)
            {
                return true;
            }
                return false;
           
        }
        public async Task<bool>MediaBeca(int idalumno,int periodo)
        {
            
            bool mediaBeca = await _Iservices.GetAlumnosAsync().ContinueWith(a => a.Result.FirstOrDefault(b => b.IdAlumno == idalumno && b.MediaBeca == true && b.IdPeriodo == periodo)?.MediaBeca ?? false);
            
            if (mediaBeca)
            {
                return true;
            }
            
                return false;
           
        }
        public bool ExistePagoDuplicado(PagosViewModel nuevoPago)
        {

            var query =  _Iservices.GetPagosAsync().ContinueWith(
                         q => q.Result.Where(p => p.IdAlumno == nuevoPago.Pago.IdAlumno &&
                        p.IdPeriodo == nuevoPago.Pago.IdPeriodo &&
                        p.IdTipoMovimiento == nuevoPago.Pago.IdTipoMovimiento));

            if(!query.Result.Any())
            {
                return false;
            }
            

            return true;
        }
    //    public (bool tienePendientes, List<int> mesesPendientes, decimal moraAcumulada, bool puedePagar)
    //ValidarPagoConMora(int idAlumno, int idTipoMovimiento, int periodo, int mesDeseado)
    //    {
    //        var pagosRealizados = listpagos
    //            .Where(p => p.IdAlumno == idAlumno &&
    //                       p.IdTipoMovimiento == idTipoMovimiento &&
    //                       p.IdPeriodo == periodo &&
    //                       p.IdMes <= mesDeseado) // Solo meses hasta el deseado
    //            .Select(p => p.IdMes)
    //            .OrderBy(m => m)
    //            .ToList();

    //        // Encontrar todos los meses que deberían estar pagados (1 hasta mesDeseado-1)
    //        var todosMesesRequeridos = Enumerable.Range(1, mesDeseado - 1).ToList();

    //        // Meses pendientes son los que no están en pagosRealizados
    //        var mesesPendientes = todosMesesRequeridos.Except(pagosRealizados).ToList();

    //        // Validar si puede pagar el mes deseado
    //        bool puedePagar = !mesesPendientes.Any() ||
    //                         (mesesPendientes.Count > 0 && mesesPendientes.Max() == mesDeseado - 1);

    //        // Calcular mora
    //        decimal montoBase = 1000; // Obtener de tu configuración
    //        decimal mora = mesesPendientes.Count * (montoBase * 0.10m);

    //        return (mesesPendientes.Any(), mesesPendientes, mora, puedePagar);
    //    }

        // GET: PagosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PagosController/Edit/5
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

        // GET: PagosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PagosController/Delete/5
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
