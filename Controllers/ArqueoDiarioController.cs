using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebColegio.Controllers
{
    public class ArqueoDiarioController : Controller
    {

        private readonly IServicesApi _Iservices;
        public ArqueoDiarioController(IServicesApi services)
        {
            _Iservices = services;
        }
        [Authorize]
        // GET: ArqueoDiarioController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ArqueoDiarioController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ArqueoDiarioController/Create
        public async Task<ActionResult> Create()
        {
            
            var recibos = await _Iservices.GetArqueoDiarioAsync();

            var maxNumero = recibos
                .Where(r => r.Serie == "A")
                .Max(r => (int?)r.NumeroArqueo);
            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 40001;
            return View(siguienteNumero);
        }

        // POST: ArqueoDiarioController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ArqueoDiarioViewModel arqueoDia)
        {

            bool response = false;
            bool validarDuplicado = false;
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            arqueoDia.arqueoDiario.UsuarioRegistro = idUsuario;
            arqueoDia.arqueoDiario.FechaRegistro = DateTime.Now;
            arqueoDia.arqueoDiario.Activo = true;
            arqueoDia.arqueoDiario.Serie = "A";
            //int mensualidad = 640;
            //int total = 0;
            var buscarIdGuardado = await _Iservices.GetArqueoDiarioAsync();
            //var buscarperiodo = await _Iservices.GetPeriodoAsync();
            //var periodo = buscarperiodo.Where(r => r.Periodo == DateTime.Now.Year && r.Activo == true && r.Actual == true).FirstOrDefault();
          
            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroArqueo == arqueoDia.arqueoDiario.NumeroArqueo && r.Serie == "A" && r.Activo == true);
            if (validarDuplicado)
            {
                TempData["Mensaje"] = "El número de Arqueo ya Existe.";
                TempData["Tipo"] = "warning";
                if (idUsuario == 1)
                {
                    return RedirectToAction("estadocuenta", "Pagos");
                }
                if (idUsuario == 2)
                {
                    return RedirectToAction("estadocuenta","PagoCaja");
                }
                
            }
            try
            {

                //if (numRecibo==null)
                //{
                //    TempData["Mensaje"] = "El numero de Recibo ya existe.";
                //}
                if (arqueoDia != null)
                {
                    //var userId = _userManager.GetUserId(User); // o UserManager.GetUserId(User)

                    var periodo = await _Iservices.GetPeriodoAsync();
                   int periodoActual = periodo.Where(r => r.Periodo == DateTime.Now.Year && r.Activo == true && r.Actual == true).FirstOrDefault().IdPeriodo;

                    //await _Iservices.InsertarPagoAsync(nuevoPago);
                    arqueoDia.arqueoDiario.UsuarioRegistro = idUsuario;
                    arqueoDia.arqueoDiario.FechaRegistro = DateTime.Now;
                    arqueoDia.arqueoDiario.Activo = true;
                    arqueoDia.arqueoDiario.IdPeriodo = periodoActual;
                    arqueoDia.arqueoDiario.Serie = "A";

                    response = await _Iservices.PostArqueoDiarioAsync(arqueoDia.arqueoDiario);
                    if (response)
                    {

                        //var idPag = buscarIdGuardado.Max(a => a.IdArqueo);
                        TempData["Mensaje"] = "Se Guardo Correctamente el Arqueo del Día.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("ArqueoCaja");
                    }
                    else
                    {
                        TempData["Mensaje"] = "No se proceso el Arqueo.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("ArqueoCaja");

                    }
                }
                return NoContent();


            }
            catch
            {
                return View();
            }

        }
        [Authorize]
        public async Task<ActionResult> ArqueoCaja(DateTime fecha, int? idRecinto = null)
        {
            // Si no viene fecha (ej. entrada directa a la URL), usar hoy
            if (fecha == default)
                fecha = DateTime.Now.Date;

            var recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            var recibos = await _Iservices.GetArqueoDiarioAsync() ?? new List<TblArqueoDiario>();
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var arqueosSerieA = recibos.Where(r => r.Serie == "A").ToList();
            var maxNumero = arqueosSerieA.Count > 0 ? (int?)arqueosSerieA.Max(r => r.NumeroArqueo) : null;
            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 40001;

            var Recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            var arqueo = new ArqueoDiarioViewModel
            {
                Serie = "A",
                Fecha = fecha,
                siguienteNumero = siguienteNumero
            };

            var pagosDelDia = (await _Iservices.GetPagosAsync())?.Where(r => r.UsuarioRegistro == idUsuario && r.FechaRegistro.Date == fecha && r.Activo).ToList() ?? new List<TblPago>();
            var pagosCajaDia = (await _Iservices.GetPagoCajaAsync())?.Where(r => r.UsuarioRegistro == idUsuario && r.FechaRegistro.Date == fecha && r.Activo).ToList() ?? new List<TblPagoCaja>();

            // Filtrar por recinto cuando el usuario eligió uno
            if (idRecinto.HasValue && idRecinto.Value > 0)
            {
                pagosDelDia = pagosDelDia.Where(r => r.IdRecinto == idRecinto.Value).ToList();
                pagosCajaDia = pagosCajaDia.Where(r => r.IdRecinto == idRecinto.Value).ToList();
                arqueo.arqueoDiario.IdRecinto = idRecinto.Value;
                // Asignar nombre y dirección del recinto
                var recintoSel = Recintos.FirstOrDefault(r => r.IdRecinto == idRecinto.Value);
                if (recintoSel != null)
                {
                    arqueo.Colegio = recintoSel.Recinto?.ToUpper() ?? "RECINTO";
                    arqueo.Direccion = ObtenerDireccionRecinto(idRecinto.Value);
                }
            }
            else
            {
                // Sin filtro: preseleccionar primer recinto con datos
                var primerRecintoConDatos = pagosDelDia.FirstOrDefault()?.IdRecinto ?? pagosCajaDia.FirstOrDefault()?.IdRecinto;
                if (primerRecintoConDatos.HasValue && primerRecintoConDatos.Value > 0)
                {
                    arqueo.arqueoDiario.IdRecinto = primerRecintoConDatos.Value;
                    var recintoSel = Recintos.FirstOrDefault(r => r.IdRecinto == primerRecintoConDatos.Value);
                    if (recintoSel != null)
                    {
                        arqueo.Colegio = recintoSel.Recinto?.ToUpper() ?? "RECINTO";
                        arqueo.Direccion = ObtenerDireccionRecinto(primerRecintoConDatos.Value);
                    }
                }
            }

            // Inicializar el viewmodel del arqueo


            // 1️⃣ Obtener los pagos del día (ingresos y egresos)0


            //pagosDelDia = pagosDelDia
            //    .Where(p => p.FechaRegistro.Date == fecha.Date && p.Activo)
            //    .ToList();

            //pagosCajaDia= pagosCajaDia
            //    .Where(p => p.FechaRegistro.Date == fecha.Date && p.Activo)
            //    .ToList();

            var ordenarNumeroRecibo= pagosDelDia.OrderBy(p => p.NumeroRecibo).ToList();
           
            //var ordenarNumeroRecibo.pagosCajaDia.OrderBy(pc => pc.NumeroRecibo).ToList();
            var primerRecibo = ordenarNumeroRecibo.FirstOrDefault();
            var ultimoRecibo = ordenarNumeroRecibo.LastOrDefault();
            // Egresos del día (filtrar por recinto si se eligió uno)
            var egresosDelDia = (await _Iservices.GetEgresoAsync())?
                .Where(p => p.UsuarioRegistro == idUsuario && p.FechaRegistro.Date == fecha.Date && p.Activo)
                .ToList() ?? new List<TblEgreso>();
            if (idRecinto.HasValue && idRecinto.Value > 0)
                egresosDelDia = egresosDelDia.Where(p => p.IdRecinto == idRecinto.Value).ToList();

            // 2️⃣ Obtener los tipos movimientos relacionados
            var tipmov = (await _Iservices.GetTipoMovimientoAsync())?
                .Where(r => r.Activo == true && r.Concepto != string.Empty)
                .ToList() ?? new List<CatTipoMovimiento>();
            // 2️⃣ Obtener los metodo pago relacionados
            var metpago = (await _Iservices.GetMetodoPagoAsync())?
                .Where(r => r.Activo == true && r.MetodoPago != string.Empty)
                .ToList() ?? new List<CatMetodoPago>();

            // 2️⃣ Obtener los recibos relacionados
            //var recibos = await _Iservices.GetRecibosCajaAsync();
            //recibos = recibos
            //    .Where(r => r.Activo == true && r.NumeroRecibo > 0)
            //    .ToList();
            // 3️⃣ Armar los ingresos
            arqueo.Ingresos =
                
                (
                    from p in pagosDelDia
                    join r in recintos on p.IdRecinto equals r.IdRecinto into pr
                    from r in pr.DefaultIfEmpty()

                    join tm in tipmov on p.IdTipoMovimiento equals tm.IdTipoMovimiento into tmm
                    from tm in tmm.DefaultIfEmpty()

                    join mp in metpago on p.IdMetodoPago equals mp.IdMetodoPago into mpp
                    from mp in mpp.DefaultIfEmpty()

                    where p != null //&& p.IdTipoMovimiento == r.IdTipoMovimiento // 1 = Ingreso
                    group new { p, tm, mp,r } by new
                    {
                        Concepto = tm != null ? tm.Concepto : "Sin concepto",
                        Recibo = p != null ? p.NumeroRecibo.ToString() : "N/A",
                        Recinto= r != null ? r.Recinto.ToString() : "N/A",
                    }
                    into g
                    select new IngresoDto
                    {
                        Concepto = g.Key.Concepto,
                        Cantidad = g.Count(),
                        Recibo = g.Key.Recibo,
                        //primerReciboDia = primerRecibo?.NumeroRecibo,
                        //ultimoReciboDia=ultimoRecibo?.NumeroRecibo,
                        Monto = g.Sum(x => x.p.Monto)
                    }
                ).ToList();
            arqueo.IngresoCajaDto=
                 (
                    from pc in pagosCajaDia
                        //join r in recibos on p.IdPago equals r.IdPago into pr
                        //from r in pr.DefaultIfEmpty()

                    join tmc in tipmov on pc.Concepto equals tmc.IdTipoMovimiento into tmmc
                    from tmc in tmmc.DefaultIfEmpty()

                    //join mp in metpago on p.i equals mp.IdMetodoPago into mpp
                    //from mp in mpp.DefaultIfEmpty()

                    where pc != null //&& p.IdTipoMovimiento == r.IdTipoMovimiento // 1 = Ingreso
                    group new { pc, tmc } by new
                    {
                        Concepto = tmc != null ? tmc.Concepto : "Sin concepto",
                        Recibo = pc != null ? pc.NumeroRecibo.ToString() : "N/A",

                    }
                    into gc
                    select new IngresoCajaDto
                    {
                        Conceptos = gc.Key.Concepto,
                        Cantidades = gc.Count(),
                        Recibos = gc.Key.Recibo,
                        //primerReciboDia = primerRecibo?.NumeroRecibo,
                        //ultimoReciboDia=ultimoRecibo?.NumeroRecibo,
                        Montos = gc.Sum(x => x.pc.Monto)
                    }
                ).ToList();




            // 4️⃣ Armar los egresos
            arqueo.Egresos = (from p in egresosDelDia
                              where p.NumeroRecibo > 0 // suponiendo 2 = egreso
                              //group p by p.NumeroRecibo into g
                              select new EgresoDto
                              {
                                  Detalle = p.Descripcion,
                                  Monto = p.Monto
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

            // 9️⃣ Lista de recintos para que el usuario elija cuál arquear
            arqueo.TblRecintos = Recintos?.Where(r => r.Activo).ToList() ?? new List<Recintos>();

            return View(arqueo);
        }

        /// <summary>Devuelve la dirección conocida del recinto (1 y 2 configurados; otros vacío).</summary>
        private static string ObtenerDireccionRecinto(int idRecinto)
        {
            return idRecinto switch
            {
                1 => "Ciudad Sandino, Plaza Padre Miguel, 2c. al Norte, Zona #4 Managua",
                2 => "Zona Once",
                _ => ""
            };
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

        // GET: ArqueoDiarioController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }
        [Authorize]
        // POST: ArqueoDiarioController/Edit/5
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

        // GET: ArqueoDiarioController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }
        [Authorize]
        // POST: ArqueoDiarioController/Delete/5
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
