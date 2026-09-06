using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebColegio.Models;
using WebColegio.Helpers;
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
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin, int? idRecinto)
        {
            var arqueos = await _Iservices.GetArqueoDiarioAsync() ?? new List<TblArqueoDiario>();
            var recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();

            var (ini, fin) = ReporteFechaQuery.ResolverRango(Request, fechainicio, fechafin);

            // Solo arqueos activos, serie A
            arqueos = arqueos.Where(a => a.Activo && a.Serie == "A").OrderByDescending(a => a.FechaRegistro).ToList();

            if (ini.HasValue)
                arqueos = arqueos.Where(a => a.FechaRegistro.Date >= ini.Value.Date).ToList();
            if (fin.HasValue)
                arqueos = arqueos.Where(a => a.FechaRegistro.Date <= fin.Value.Date).ToList();
            if (idRecinto.HasValue && idRecinto.Value > 0)
                arqueos = arqueos.Where(a => a.IdRecinto == idRecinto.Value).ToList();

            var model = new ArqueoDiarioIndexViewModel
            {
                ListaArqueos = arqueos,
                Recintos = recintos.Where(r => r.Activo).ToList(),
                FechaInicio = ini,
                FechaFin = fin,
                IdRecintoFilter = idRecinto
            };
            return View(model);
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
            var fechaReporte = arqueoDia?.Fecha != default && arqueoDia.Fecha.Year > 1
                ? arqueoDia.Fecha.Date
                : DateTime.Now.Date;
            var idRecPost = arqueoDia?.arqueoDiario?.IdRecinto;
            int? idRecintoCtx = (idRecPost ?? 0) > 0 ? idRecPost : null;

            if (arqueoDia?.arqueoDiario == null)
            {
                TempData["Mensaje"] = "No se recibieron datos del arqueo. Intente de nuevo.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(ArqueoCaja), new { fecha = fechaReporte, idRecinto = idRecintoCtx });
            }

            bool validarDuplicado = false;
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            arqueoDia.arqueoDiario.UsuarioRegistro = idUsuario;
            arqueoDia.arqueoDiario.FechaRegistro = DateTime.Now;
            arqueoDia.arqueoDiario.Activo = true;
            arqueoDia.arqueoDiario.Serie = "A";
            var buscarIdGuardado = await _Iservices.GetArqueoDiarioAsync() ?? new List<TblArqueoDiario>();
            //var buscarperiodo = await _Iservices.GetPeriodoAsync();
            //var periodo = buscarperiodo.Where(r => r.Periodo == DateTime.Now.Year && r.Activo == true && r.Actual == true).FirstOrDefault();
          
            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroArqueo == arqueoDia.arqueoDiario.NumeroArqueo && r.Serie == "A" && r.Activo == true);
            if (validarDuplicado)
            {
                TempData["Mensaje"] = "El número de Arqueo ya existe (quizá ya se guardó). Actualice la página y no envíe dos veces.";
                TempData["Tipo"] = "warning";
                if (idUsuario == 1)
                    return RedirectToAction("estadocuenta", "Pagos");
                if (idUsuario == 2)
                    return RedirectToAction("estadocuenta", "PagoCaja");
                return RedirectToAction(nameof(ArqueoCaja), new { fecha = fechaReporte, idRecinto = idRecintoCtx });
            }

            // Un arqueo activo por recinto y día de registro (evita segundo guardado el mismo día con otro número)
            var idRecGuardar = arqueoDia.arqueoDiario.IdRecinto;
            if (idRecGuardar.HasValue && idRecGuardar.Value > 0)
            {
                var yaGuardadoHoyEsteRecinto = buscarIdGuardado.Any(r =>
                    r.Activo && r.Serie == "A"
                    && r.IdRecinto == idRecGuardar.Value
                    && r.FechaRegistro.Date == DateTime.Today);
                if (yaGuardadoHoyEsteRecinto)
                {
                    var horaCierre = buscarIdGuardado
                        .Where(r => r.Activo && r.Serie == "A" && r.IdRecinto == idRecGuardar.Value && r.FechaRegistro.Date == DateTime.Today)
                        .OrderByDescending(r => r.FechaRegistro)
                        .Select(r => r.FechaRegistro)
                        .FirstOrDefault();
                    TempData["Mensaje"] = horaCierre != default
                        ? $"Ya se registró el arqueo de este recinto hoy ({horaCierre:hh:mm tt}). Los pagos posteriores se incluirán en el arqueo de mañana."
                        : "Ya se registró un arqueo para este recinto hoy. Los pagos posteriores se incluirán en el arqueo de mañana.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(ArqueoCaja), new { fecha = fechaReporte, idRecinto = idRecintoCtx });
                }
            }

            try
            {
                if (arqueoDia != null)
                {
                    var periodo = await _Iservices.GetPeriodoAsync();
                    var periodoRow = periodo?.FirstOrDefault(r => r.Periodo == DateTime.Now.Year && r.Activo == true && r.Actual == true);
                    if (periodoRow == null)
                    {
                        TempData["Mensaje"] = "No hay un período activo para el año actual. Configure el período antes de guardar el arqueo.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction(nameof(ArqueoCaja), new { fecha = fechaReporte, idRecinto = idRecintoCtx });
                    }
                    int periodoActual = periodoRow.IdPeriodo;

                    arqueoDia.arqueoDiario.UsuarioRegistro = idUsuario;
                    arqueoDia.arqueoDiario.FechaRegistro = DateTime.Now;
                    arqueoDia.arqueoDiario.Activo = true;
                    arqueoDia.arqueoDiario.IdPeriodo = periodoActual;
                    arqueoDia.arqueoDiario.Serie = "A";

                    var (ok, errApi) = await _Iservices.PostArqueoDiarioAsync(arqueoDia.arqueoDiario);
                    if (ok)
                    {
                        TempData["Mensaje"] = "Se guardó correctamente el arqueo. Puede imprimir el comprobante.";
                        TempData["Tipo"] = "success";
                        TempData["AbrirImpresion"] = "1";
                        return RedirectToAction(nameof(ArqueoCaja), new { fecha = fechaReporte, idRecinto = idRecintoCtx });
                    }
                    TempData["Mensaje"] = string.IsNullOrWhiteSpace(errApi)
                        ? "No se procesó el arqueo (la API no respondió OK). Revise conexión y datos."
                        : "No se procesó el arqueo. Detalle: " + errApi;
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(ArqueoCaja), new { fecha = fechaReporte, idRecinto = idRecintoCtx });
                }
                return NoContent();
            }
            catch (Exception)
            {
                TempData["Mensaje"] = "Ocurrió un error al guardar el arqueo. Verifique el período activo y los datos.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(ArqueoCaja), new { fecha = fechaReporte, idRecinto = idRecintoCtx });
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
            bool esAdmin = User.IsInRole("Admin");
            var usuarioActual = await _Iservices.GetUsuarioIdAsync(idUsuario);

            var arqueosSerieA = recibos.Where(r => r.Serie == "A").ToList();
            var maxNumero = arqueosSerieA.Count > 0 ? (int?)arqueosSerieA.Max(r => r.NumeroArqueo) : null;
            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 40001;

            var Recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            var arqueo = new ArqueoDiarioViewModel
            {
                Serie = "A",
                Fecha = fecha,
                siguienteNumero = siguienteNumero,
                EsAdministrador = esAdmin
            };

            // Cajero: solo puede ver el arqueo de su recinto; se ignora idRecinto y se usa el del usuario
            int? recintoEfectivo = idRecinto;
            if (!esAdmin)
            {
                recintoEfectivo = usuarioActual?.IdRecinto ?? 0;
                if (recintoEfectivo == 0) recintoEfectivo = null;
            }

            var (inicioVentana, finVentana, arqueoDelDia, inicioEsArqueoAnterior) =
                ArqueoCierreHelper.ResolverVentana(recibos, recintoEfectivo, fecha, DateTime.Now);
            arqueo.InicioVentana = inicioVentana;
            arqueo.FinVentana = finVentana;
            arqueo.ArqueoYaCerrado = arqueoDelDia != null;

            bool EnTurno(DateTime fechaPago) =>
                ArqueoCierreHelper.EstaEnVentana(fechaPago, inicioVentana, finVentana, inicioEsArqueoAnterior);

            // Del último arqueo (p. ej. 3–4 p.m. de ayer) hasta el cierre de hoy. Lo posterior va al día siguiente.
            var todosPagosDia = (await _Iservices.GetPagosAsync())?.Where(r => r.Activo && EnTurno(r.FechaRegistro)).ToList() ?? new List<TblPago>();
            var todosPagosCajaDia = (await _Iservices.GetPagoCajaAsync())?.Where(r => r.Activo && EnTurno(r.FechaRegistro)).ToList() ?? new List<TblPagoCaja>();

            List<TblPago> pagosDelDia;
            List<TblPagoCaja> pagosCajaDia;
            if (esAdmin)
            {
                if (recintoEfectivo.HasValue && recintoEfectivo.Value > 0)
                {
                    pagosDelDia = todosPagosDia.Where(r => r.IdRecinto == recintoEfectivo.Value).ToList();
                    pagosCajaDia = todosPagosCajaDia.Where(r => r.IdRecinto == recintoEfectivo.Value).ToList();
                }
                else
                {
                    pagosDelDia = new List<TblPago>();
                    pagosCajaDia = new List<TblPagoCaja>();
                }
            }
            else
            {
                // Cajero: si el usuario tiene recinto asignado, filtrar por él; si no, todos los pagos del usuario ese día (evita IdRecinto == 0 que no coincide con ningún pago real)
                if (recintoEfectivo.HasValue && recintoEfectivo.Value > 0)
                {
                    pagosDelDia = todosPagosDia.Where(r => r.UsuarioRegistro == idUsuario && r.IdRecinto == recintoEfectivo.Value).ToList();
                    pagosCajaDia = todosPagosCajaDia.Where(r => r.UsuarioRegistro == idUsuario && r.IdRecinto == recintoEfectivo.Value).ToList();
                }
                else
                {
                    pagosDelDia = todosPagosDia.Where(r => r.UsuarioRegistro == idUsuario).ToList();
                    pagosCajaDia = todosPagosCajaDia.Where(r => r.UsuarioRegistro == idUsuario).ToList();
                }
            }

            // Asignar recinto al arqueo y nombre/dirección
            if (recintoEfectivo.HasValue && recintoEfectivo.Value > 0)
            {
                arqueo.arqueoDiario.IdRecinto = recintoEfectivo.Value;
                var recintoSel = Recintos.FirstOrDefault(r => r.IdRecinto == recintoEfectivo.Value);
                if (recintoSel != null)
                {
                    arqueo.Colegio = recintoSel.Recinto?.ToUpper() ?? "RECINTO";
                    arqueo.Direccion = ObtenerDireccionRecinto(recintoEfectivo.Value);
                }
            }
            else if (esAdmin)
            {
                // Admin sin recinto elegido: no preseleccionar; el usuario debe elegir en la vista
                arqueo.arqueoDiario.IdRecinto = null;
                arqueo.Colegio = "SELECCIONE RECINTO";
                arqueo.Direccion = "";
            }
            else
            {
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
            // Egresos del día: Admin por recinto (todos), Cajero solo los suyos en su recinto
            var todosEgresosDia = (await _Iservices.GetEgresoAsync())?
                .Where(p => p.Activo && EnTurno(p.FechaRegistro))
                .ToList() ?? new List<TblEgreso>();
            List<TblEgreso> egresosDelDia;
            if (esAdmin && recintoEfectivo.HasValue && recintoEfectivo.Value > 0)
                egresosDelDia = todosEgresosDia.Where(p => p.IdRecinto == recintoEfectivo.Value).ToList();
            else if (!esAdmin)
                egresosDelDia = recintoEfectivo.HasValue && recintoEfectivo.Value > 0
                    ? todosEgresosDia.Where(p => p.UsuarioRegistro == idUsuario && p.IdRecinto == recintoEfectivo.Value).ToList()
                    : todosEgresosDia.Where(p => p.UsuarioRegistro == idUsuario).ToList();
            else
                egresosDelDia = new List<TblEgreso>();

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
            // Mora registrada en BD: una línea por recibo (evita duplicar al pagar varios meses en el mismo recibo).
            // Se toma el valor del mes cancelado (IdMes más alto), no se recalcula mora en el arqueo.
            var morasDelDia = pagosDelDia
                .Where(p => (p.Mora ?? 0) > 0)
                .GroupBy(p => new
                {
                    p.NumeroRecibo,
                    Serie = p.Serie ?? "A",
                    p.IdAlumno,
                    p.IdPeriodo
                })
                .Select(g =>
                {
                    var pagoMesCancelado = g
                        .Where(x => x.IdMes.HasValue)
                        .OrderByDescending(x => x.IdMes!.Value)
                        .FirstOrDefault();

                    var moraRegistrada = pagoMesCancelado?.Mora ?? g.Max(x => x.Mora ?? 0);

                    return new IngresoDto
                    {
                        Concepto = "Mora / Recargo por atraso",
                        Cantidad = 1,
                        Recibo = g.Key.NumeroRecibo?.ToString() ?? "N/A",
                        Monto = moraRegistrada
                    };
                })
                .Where(x => x.Monto > 0)
                .ToList();

            arqueo.Ingresos.AddRange(morasDelDia);



            // 4️⃣ Armar los egresos
            arqueo.Egresos = (from p in egresosDelDia
                              where p.NumeroRecibo > 0 // suponiendo 2 = egreso
                              //group p by p.NumeroRecibo into g
                              select new EgresoDto
                              {
                                  Detalle = p.Descripcion,
                                  Monto = p.Monto
                              }).ToList();

            // 5️⃣ Totales (pagos + recibos de caja varios)
            arqueo.TotalIngresos = arqueo.Ingresos.Sum(x => x.Monto) + arqueo.IngresoCajaDto.Sum(x => x.Montos);
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

            arqueo.AbrirImpresion = TempData["AbrirImpresion"]?.ToString() == "1";

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
            var entero = (long)Math.Truncate(numero);
            var centavos = (int)Math.Round((numero - entero) * 100);
            var cultura = new System.Globalization.CultureInfo("es");
            var parteEntera = Humanizer.NumberToWordsExtension.ToWords(entero, cultura).ToUpper() + " CÓRDOBAS";
            if (centavos > 0)
                return parteEntera + " CON " + centavos.ToString("00", cultura) + "/100";
            return parteEntera;
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
