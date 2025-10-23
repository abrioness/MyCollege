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
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var viewModel = new ReciboCajaViewModel
            {
                ReciboCaja = listrecibos,                
                alumnos=_alumnos,
                metodoPago=_metodoPago,
                tipoMovimiento=_tipoMovimiento,
                cantidadEnLetras= NumeroALetras(listrecibos.Monto),
                grados=_grados
                
            };

            if (listrecibos == null)
            {
                return NotFound();
            }

            return View(viewModel);
            
        }

        // GET: ReciboCajaController/Create
        public async Task<ActionResult> Create(int id)
        {
            var recibos = await _Iservices.GetRecibosCajaAsync();

            var maxNumero = recibos
                .Where(r => r.Serie == "A")
                .Max(r => (int?)r.NumeroRecibo);

            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 10001;
            var verpago = await _Iservices.GetPagoById(id);

            var listrecibos = await _Iservices.GetReciboCajaById(id);
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var viewModel = new ReciboCajaViewModel
            {
                SiguienteNumero = siguienteNumero,
                Pago = verpago,
                ReciboCaja = listrecibos,
                alumnos = _alumnos,
                metodoPago = _metodoPago,
                tipoMovimiento = _tipoMovimiento,
                cantidadEnLetras = NumeroALetras(listrecibos.Monto),
                grados = _grados

            };

            if (verpago == null)
            {
                TempData["Mensaje"] = "El pago no existe.";
                return RedirectToAction("Index");
            }

            //var viewmodel = new ReciboCajaViewModel
            //{
            //    SiguienteNumero = siguienteNumero,
            //    Pago=verpago
                
                
                 
        //alumnosSelectListItem = (await _Iservices.GetAlumnosAsync())
        //                   .Select(r => new SelectListItem
        //                   {
        //                       Value = r.IdAlumno.ToString(),
        //                       Text = r.Nombre + r.Apellido,

        //                       //Selected = r.IdPregunta == respuestas.IdPregunta
        //                   }).ToList(),
        //tipoMovimientoSelectListItem = (await _Iservices.GetTipoMovimientoAsync())
        //                   .Select(r => new SelectListItem
        //                   {
        //                       Value = r.IdTipoMovimiento.ToString(),
        //                       Text = r.Concepto,
        //                       //Selected = r.IdPregunta == respuestas.IdPregunta
        //                   }).ToList(),
        //usuariosSelectListItem = (await _Iservices.GetUsuariosAsync())
        //                   .Select(r => new SelectListItem
        //                   {
        //                       Value = r.IdUsuario.ToString(),
        //                       Text = r.NombreUsuario,
        //                       //Selected = r.IdPregunta == respuestas.IdPregunta
        //                   }).ToList(),
        //gradosSelectListItem= (await _Iservices.GetGradosAsync())
        //                   .Select(r => new SelectListItem
        //                   {
        //                       Value = r.IdGrado.ToString(),
        //                       Text = r.NombreGrado,
        //                       //Selected = r.IdPregunta == respuestas.IdPregunta
        //                   }).ToList(),

    //};

            return View(viewModel);
        }

        // POST: ReciboCajaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ReciboPagoViewModel recibo)
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
        //Arqueo de Caja

        public async Task<ActionResult> ArqueoCaja(DateTime fecha)
        {
            // Inicializar el viewmodel del arqueo
            var arqueo = new ArqueoCajaViewModel
            {
                Colegio = "Colegio Ejemplo",
                Serie = "A",
                Fecha = fecha
            };
           
            // 1️⃣ Obtener los pagos del día (ingresos y egresos)
            var pagosDelDia = await _Iservices.GetPagosAsync();
            pagosDelDia=pagosDelDia
                .Where(p => p.FechaRegistro.Date == fecha.Date && p.Activo)
                .ToList();

            // 2️⃣ Obtener los tipos movimientos relacionados
            var tipmov = await _Iservices.GetTipoMovimientoAsync();
            tipmov = tipmov
                .Where(r => r.Activo == true && r.Concepto!=string.Empty)
                .ToList();
            // 2️⃣ Obtener los metodo pago relacionados
            var metpago = await _Iservices.GetMetodoPagoAsync();
            metpago = metpago
                .Where(r => r.Activo == true && r.MetodoPago != string.Empty)
                .ToList();

            // 2️⃣ Obtener los recibos relacionados
            var recibos = await _Iservices.GetRecibosCajaAsync();
            recibos = recibos
                .Where(r => r.Activo == true && r.NumeroRecibo > 0)
                .ToList();
            // 3️⃣ Armar los ingresos
            arqueo.Ingresos = (
                    from p in pagosDelDia
                    join r in recibos on p.IdPago equals r.IdPago into pr
                    from r in pr.DefaultIfEmpty()

                    join tm in tipmov on p.IdTipoMovimiento equals tm.IdTipoMovimiento into tmm
                    from tm in tmm.DefaultIfEmpty()

                    join mp in metpago on p.IdMetodoPago equals mp.IdMetodoPago into mpp
                    from mp in mpp.DefaultIfEmpty()

                    where r != null && p.IdTipoMovimiento == r.IdTipoMovimiento // 1 = Ingreso
                    group new { p, r, tm, mp } by new
                    {
                        Concepto = tm != null ? tm.Concepto : "Sin concepto",
                        Recibo = r != null ? r.NumeroRecibo.ToString() : "N/A",

                        MetodoPago = mp != null ? mp.MetodoPago : "No especificado"
                    }
                    into g
                    select new IngresoDto
                    {
                        Concepto = g.Key.Concepto,
                        Recibo = g.Key.Recibo,
                        Cantidad = g.Count(),
                        Monto = g.Sum(x => x.p.Monto)
                    }
                ).ToList();


            // 4️⃣ Armar los egresos
            arqueo.Egresos = (from p in pagosDelDia
                              where p.IdTipoMovimiento == 2 // suponiendo 2 = egreso
                              group p by p.IdTipoMovimiento into g
                              select new EgresoDto
                              {
                                  Detalle = "Egreso varios",
                                  Monto = g.Sum(x => x.Monto)
                              }).ToList();

            // 5️⃣ Totales
            arqueo.TotalIngresos = arqueo.Ingresos.Sum(x => x.Monto);
            arqueo.TotalEgresos = arqueo.Egresos.Sum(x => x.Monto);
            arqueo.TotalEfectivo = arqueo.TotalIngresos - arqueo.TotalEgresos;

            // 6️⃣ (Opcional) Detalle por denominación (si lo llenas manualmente desde vista)
            arqueo.DetalleCordobas = new List<DetalleCordoba>();
            arqueo.DetalleDolares = new List<DetalleDolar>();

            arqueo.TotalCordobas = arqueo.DetalleCordobas.Sum(x => x.Monto);
            arqueo.TotalDolares = arqueo.DetalleDolares.Sum(x => x.Monto);

            // 7️⃣ Equivalente (si deseas calcular en una sola moneda)
            decimal tipoCambio = 36.50m; // ejemplo
            arqueo.EquivalenteCordobas = arqueo.TotalDolares * tipoCambio + arqueo.TotalCordobas;

            // 8️⃣ Convertir total en letras
            arqueo.TotalEnLetras = NumeroALetras(arqueo.TotalEfectivo);

            return View(arqueo);
        }



        //Convertir de numero a letras
        //private string NumeroEnLetras(decimal numero)
        //{
        //    return new System.Globalization.CultureInfo("es-NI")
        //        .TextInfo.ToTitleCase($"{numero:N2} córdobas".ToLower());
        //}

        private string NumeroALetras(decimal numero)
        {
            return Humanizer.NumberToWordsExtension.ToWords((long)numero, new System.Globalization.CultureInfo("es"))
                .ToUpper() + " CÓRDOBAS";
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
