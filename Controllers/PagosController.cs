using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


            var VieModelNotas = new ColeccionCatalogos
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
            if (VieModelNotas == null)
            {
                TempData["Message"] = "No hay notas registradas";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Notas encontradas";
                return View(VieModelNotas);
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
                                   //.Select(r => new SelectListItem
                                   //{
                                   //    Value = r.IdMes.ToString(),
                                   //    Text = r.Mes,
                                   //    //Selected = r.IdPregunta == respuestas.IdPregunta
                                   //}).ToList(),


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
                //validarDuplicado = await _Iservices.ValidarNotas(pagos.MesPagado, pagos.IdTipoMovimiento, pagos.IdAlumno);
                //if (validarDuplicado == true)
                //{
                //    TempData["Mensaje"] = "El Alumno ya Posee un Registro de Nota con la Asignatura Seleccionada.";
                //    TempData["Tipo"] = "warning";
                //    return RedirectToAction("Create");
                //}

                if (pagos != null)
                {
                   if (!string.IsNullOrEmpty(MesesSeleccionados))//MesesSeleccionados != null && MesesSeleccionados.Any())
                    {
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

                                // 2️⃣ Verificar si hay mora (mes anterior sin pagar)
                                bool tienePendientes = listpagos
                                    .Any(p => p.IdAlumno == pagos.Pago.IdAlumno
                                                && p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento
                                                && p.IdMes < idMes) == false;

                                

                                if (tienePendientes)
                                {
                                    pagos.Mora = 10; // Aplica mora fija de 10
                                    aplicoMora = true;
                                }
                                else
                                {
                                    pagos.Mora = 0; // Aplica mora fija de 10
                                    aplicoMora = true;
                                }

                                // ejemplo: crear un pago por cada mes
                                var nuevoPago = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    IdMes = idMes,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = pagos.Pago.IdTipoMovimiento,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    Mora = pagos.Mora,
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
