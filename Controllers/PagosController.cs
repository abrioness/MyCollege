using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebColegio.Models;
using WebColegio.Helpers;
using WebColegio.Models.ViewModel;
using WebColegio.Services;
using static System.Net.Mime.MediaTypeNames;

namespace WebColegio.Controllers
{
    public class PagosController : Controller
    {
        private readonly IServicesApi _Iservices;
        public const int MoraPorMes = 10;
        public PagosController(IServicesApi services)
        {
            _Iservices = services;
        }
        // GET: PagosController
        
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {

            var _pagos = await _Iservices.GetPagosAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _tipoRecibo = await _Iservices.GetTipoReciboAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var _meses = await _Iservices.GetMesesAsync();
            var _recinto = await _Iservices.GetRecintosAsync();
            //var _modalidad = await _Iservices.GetModalidadesAsync();
            //var _grados = await _Iservices.GetGradosAsync();
            
            // Convertir a IQueryable para aplicar filtros de manera eficiente
            IQueryable<TblPago> query = _pagos.AsQueryable();

            var (ini, fin) = ReporteFechaQuery.ResolverRango(Request, fechainicio, fechafin);
            if (ini.HasValue)
                query = query.Where(a => a.FechaRegistro.Date >= ini.Value.Date);
            if (fin.HasValue)
                query = query.Where(a => a.FechaRegistro.Date <= fin.Value.Date);

            var pagosFiltrados = query
                .OrderByDescending(a => a.FechaRegistro)
                .ThenByDescending(a => a.IdPago)
                .ToList();
   
            var VieModelPagos = new ColeccionCatalogos
            {
                pagos = pagosFiltrados,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses = _meses,
                recintos = _recinto
                //modalidades = _modalidad,
                //grados = _grados    

            };
            if (VieModelPagos == null)
            {
                TempData["Message"] = "No hay Pagos registrados";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Pagos encontrados";
                return View(VieModelPagos);
            }
        }
        // Tipos de movimiento alineados con registro de pagos (mensualidad / matrícula)
        private const int TipoMovimientoMensualidad = 1;
        private const int TipoMovimientoMatricula = 2;
        private const int TipoMovimientoMatriculaAbono = 4;

        private static string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalizado)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Mes calendario (1–12) de la primera matrícula del alumno en el período (fecha de emisión o registro).
        /// Con matrícula prematura (p. ej. en abril), no se exige pagar febrero/marzo antes de abril.
        /// </summary>
        private static int ObtenerMesCalendarioPrimeraMatricula(IEnumerable<TblPago> pagos, int idAlumno, int idPeriodo)
        {
            var p = pagos
                .Where(x => x.IdAlumno == idAlumno && x.IdPeriodo == idPeriodo && x.Activo
                    && (x.IdTipoMovimiento == TipoMovimientoMatricula || x.IdTipoMovimiento == TipoMovimientoMatriculaAbono))
                .OrderBy(x => x.FechaRegistro)
                .FirstOrDefault();
            if (p == null)
                return 1;
            var m = p.FechaEmision?.Month ?? p.FechaRegistro.Month;
            return Math.Clamp(m, 1, 12);
        }

        [Authorize]
        public async Task<ActionResult> EstadoCuenta()
        {
            var pagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            var alumnos = (await _Iservices.GetAlumnosAsync())?.Where(a => a.Activo != false).ToList()
                ?? new List<TblAlumno>();
            var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoRef = ObtenerPeriodoLectivoActual(periodos);
            if (periodoRef == null)
            {
                TempData["Message"] = "No hay período lectivo activo configurado.";
                return View(new EstadoCuentaViewModel
                {
                    MensajePeriodo = "No hay período lectivo activo. Configure un período en catálogo con Activo y marque como Actual."
                });
            }

            int idPeriodoRef = periodoRef.IdPeriodo;
            int anioPeriodo = periodoRef.Periodo;

            var costosMen = await _Iservices.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>();
            var costosMat = await _Iservices.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>();
            var grados = await _Iservices.GetGradosAsync() ?? new List<Grados>();
            var recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            var mesesCatalog = await _Iservices.GetMesesAsync() ?? new List<TblCatMeses>();

            var culturaEs = new CultureInfo("es-NI");
            var nombresMes = Enumerable.Range(1, 12).Select(m =>
                mesesCatalog.FirstOrDefault(x => x.IdMes == m)?.Mes?.Trim()
                ?? culturaEs.DateTimeFormat.GetMonthName(m)).ToArray();

            int mesMensualidadRequerido = EstadoCuentaSolvenciaHelper.ObtenerMesMensualidadRequerido();
            string nombreMesRequerido = nombresMes[mesMensualidadRequerido - 1];

            var filas = new List<EstadoCuentaFilaAlumno>();

            foreach (var alumno in alumnos.OrderBy(a => a.Apellido).ThenBy(a => a.Nombre))
            {
                if (!alumno.IdGrado.HasValue || !alumno.IdRecinto.HasValue || !alumno.IdModalidad.HasValue)
                {
                    filas.Add(new EstadoCuentaFilaAlumno
                    {
                        IdAlumno = alumno.IdAlumno,
                        NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}".Trim(),
                        AnioPeriodo = anioPeriodo,
                        DatosIncompletos = true,
                        MotivoIncompleto = "Falta grado, recinto o modalidad en la ficha del alumno."
                    });
                    continue;
                }

                int idG = alumno.IdGrado!.Value;
                int idR = alumno.IdRecinto!.Value;
                int idMod = alumno.IdModalidad!.Value;

                bool becaCompleta = alumno.BecaCompleta == true && alumno.IdPeriodo == idPeriodoRef;
                bool mediaBeca = alumno.MediaBeca == true && alumno.IdPeriodo == idPeriodoRef;

                var costoMen = ResolverFilaCostoMensualidad(
                    costosMen, idR, idG, idPeriodoRef, idMod);
                decimal montoMensual = costoMen != null ? (decimal)costoMen.CostoMensualidad : 0m;
                if (becaCompleta)
                    montoMensual = 0m;
                else if (mediaBeca && montoMensual > 0)
                    montoMensual *= 0.5m;

                var costoMat = costosMat.FirstOrDefault(c =>
                    c.IdRecinto == idR && c.IdModalidad == idMod && c.IdPeriodo == idPeriodoRef && c.Activo);
                decimal montoMat = costoMat != null ? (decimal)costoMat.CostoMatricula : 0m;

                var pagosAlum = pagos.Where(p => p.IdAlumno == alumno.IdAlumno && p.IdPeriodo == idPeriodoRef && p.Activo).ToList();

                decimal pagadoMat = pagosAlum
                    .Where(p => p.IdTipoMovimiento == TipoMovimientoMatricula || p.IdTipoMovimiento == TipoMovimientoMatriculaAbono)
                    .Sum(p => p.Monto);
                decimal saldoMat = Math.Max(0m, montoMat - pagadoMat);
                bool matCancelada = montoMat <= 0m || saldoMat <= 0.01m;

                var meses = new EstadoCuentaMesCelda[12];
                decimal totalSaldoMeses = 0m;

                for (int m = 1; m <= 12; m++)
                {
                    decimal pagadoMes = pagosAlum
                        .Where(p => p.IdTipoMovimiento == TipoMovimientoMensualidad && p.IdMes == m)
                        .Sum(p => p.Monto);
                    decimal esperado = montoMensual;
                    decimal saldo = Math.Max(0m, esperado - pagadoMes);
                    bool cancelado = esperado <= 0m || saldo <= 0.01m;
                    totalSaldoMeses += cancelado ? 0m : saldo;

                    meses[m - 1] = new EstadoCuentaMesCelda
                    {
                        Mes = m,
                        NombreMes = nombresMes[m - 1],
                        MontoEsperado = esperado,
                        MontoPagado = pagadoMes,
                        Saldo = cancelado ? 0m : saldo,
                        Cancelado = cancelado
                    };
                }

                var ultimoPago = pagosAlum.OrderByDescending(p => p.IdPago).FirstOrDefault();
                string estadoPagoMensualidad = EstadoCuentaSolvenciaHelper.EvaluarEstadoPagoMensualidad(meses);
                var mesesPendientesSolvencia = EstadoCuentaSolvenciaHelper.ObtenerMesesPendientes(meses);
                string? textoMesesPendientes = mesesPendientesSolvencia.Count > 0
                    ? string.Join(", ", mesesPendientesSolvencia)
                    : null;
                int? ultimoMesPagado = EstadoCuentaSolvenciaHelper.ObtenerMesHastaPagadoParaMostrar(meses);
                string? nombreUltimoMesPagado = ultimoMesPagado.HasValue
                    ? nombresMes[ultimoMesPagado.Value - 1]
                    : null;

                filas.Add(new EstadoCuentaFilaAlumno
                {
                    IdAlumno = alumno.IdAlumno,
                    NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}".Trim(),
                    NombreGrado = grados.FirstOrDefault(g => g.IdGrado == idG)?.NombreGrado,
                    Recinto = recintos.FirstOrDefault(r => r.IdRecinto == idR)?.Recinto,
                    AnioPeriodo = anioPeriodo,
                    MensualidadReferencia = montoMensual,
                    MatriculaReferencia = montoMat,
                    TotalPagadoMatricula = pagadoMat,
                    SaldoMatricula = matCancelada ? 0m : saldoMat,
                    MatriculaCancelada = matCancelada,
                    Meses = meses,
                    TotalSaldoMensualidades = totalSaldoMeses,
                    GranTotalPendiente = (matCancelada ? 0m : saldoMat) + totalSaldoMeses,
                    IdPagoParaEnlace = ultimoPago?.IdPago,
                    SinTarifaMensualidad = costoMen == null && !becaCompleta && !mediaBeca,
                    EstadoPagoMensualidad = estadoPagoMensualidad,
                    MesMensualidadRequerido = mesMensualidadRequerido,
                    NombreMesMensualidadRequerido = nombreMesRequerido,
                    MesesPendientesSolvencia = textoMesesPendientes,
                    UltimoMesPagado = ultimoMesPagado,
                    NombreUltimoMesPagado = nombreUltimoMesPagado
                });
            }

            TempData["Message"] = "Estado de cuenta por período lectivo actual y tarifas de catálogo.";
            return View(new EstadoCuentaViewModel
            {
                AnioPeriodoReferencia = anioPeriodo,
                MesMensualidadRequerido = mesMensualidadRequerido,
                NombreMesMensualidadRequerido = nombreMesRequerido,
                MensajePeriodo = $"Período lectivo de referencia: {anioPeriodo} (Id {idPeriodoRef}). Insolvente si debe algún mes anterior al mes actual; la columna Estado pago indica hasta qué mes tiene pagada la mensualidad (incluye pagos adelantados del año).",
                Filas = filas
            });
        }

        private static CatPeriodo? ObtenerPeriodoLectivoActual(IEnumerable<CatPeriodo>? periodos)
        {
            var lista = periodos?.Where(p => p.Activo).ToList() ?? new List<CatPeriodo>();
            return lista.FirstOrDefault(p => p.Actual)
                   ?? lista.OrderByDescending(p => p.Periodo).ThenByDescending(p => p.IdPeriodo).FirstOrDefault();
        }
        //Formato Matricula


        // GET: PagosController/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int id, bool imprimir = false)
        {

            var listpagos = await _Iservices.GetPagoById(id);
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _turnos = await _Iservices.GetTurnosAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var viewModel = new PagosViewModel
            {
                Pago = listpagos,
                alumnos = _alumnos,
                metodoPago = _metodoPago,
                tipoMovimiento = _tipoMovimiento,
                cantidadEnLetras = NumeroALetras(listpagos.Monto),
                grados = _grados,
                turnos=_turnos,
                recintos = (await _Iservices.GetRecintosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto,
                    //Selected = r.IdPregunta == respuestas.IdPregunta
                }).ToList(),

            };

            if (listpagos == null)
            {
                return NotFound();
            }

            // Si es pago de matrícula (IdTipoMovimiento 2), sumar matrícula + mensualidad del mismo recibo para mostrar el total
            const int idTipoMatricula = 2;
            const int idTipoMensualidad = 1;
            if (listpagos.NumeroRecibo.HasValue && !string.IsNullOrEmpty(listpagos.Serie))
            {
                var todosPagos = await _Iservices.GetPagosAsync();
                if (listpagos.IdTipoMovimiento == idTipoMatricula)
                {
                    var pagosMismoRecibo = todosPagos
                        .Where(p => p.NumeroRecibo == listpagos.NumeroRecibo
                            && p.Serie == listpagos.Serie
                            && p.IdAlumno == listpagos.IdAlumno
                            && p.Activo)
                        .Where(p => p.IdTipoMovimiento == idTipoMatricula || p.IdTipoMovimiento == idTipoMensualidad)
                        .ToList();
                    decimal totalRecibo = pagosMismoRecibo.Sum(p => p.Monto);
                    if (totalRecibo > 0)
                    {
                        viewModel.MontoTotalRecibo = totalRecibo;
                        viewModel.CantidadEnLetrasTotalRecibo = NumeroALetras(totalRecibo);
                    }
                }
                else if (listpagos.IdTipoMovimiento == idTipoMensualidad)
                {
                    // Mensualidad: sumar todas las mensualidades del mismo recibo (varios meses en un solo recibo)
                    var pagosMismoRecibo = todosPagos
                        .Where(p => p.NumeroRecibo == listpagos.NumeroRecibo
                            && p.Serie == listpagos.Serie
                            && p.IdAlumno == listpagos.IdAlumno
                            && p.IdPeriodo == listpagos.IdPeriodo
                            && p.IdTipoMovimiento == idTipoMensualidad
                            && p.Activo)
                        .ToList();
                    decimal totalRecibo = pagosMismoRecibo.Sum(p => p.Monto);
                    if (totalRecibo > 0)
                    {
                        viewModel.MontoTotalRecibo = totalRecibo;
                        viewModel.CantidadEnLetrasTotalRecibo = NumeroALetras(totalRecibo);
                        var mesesCatalog = await _Iservices.GetMesesAsync();
                        var idsMeses = pagosMismoRecibo.Where(p => p.IdMes.HasValue).Select(p => p.IdMes!.Value).OrderBy(m => m).Distinct().ToList();
                        if (idsMeses.Any())
                        {
                            var nombresMeses = idsMeses.Select(id => mesesCatalog.FirstOrDefault(m => m.IdMes == id)?.Mes ?? id.ToString()).ToList();
                            viewModel.DetalleMesesRecibo = string.Join(", ", nombresMeses);
                        }
                        else
                            viewModel.DetalleMesesRecibo = pagosMismoRecibo.Count > 1 ? $"{pagosMismoRecibo.Count} mensualidades" : null;
                    }
                }
            }

            // Pasar el parámetro de impresión a la vista
            ViewBag.Imprimir = imprimir;

            return View(viewModel);
        }
        //numeros en letra
        private string NumeroALetras(decimal numero)
        {
            return Humanizer.NumberToWordsExtension.ToWords((long)numero, new System.Globalization.CultureInfo("es"))
                .ToUpper() + " CÓRDOBAS";
        }

        // GET: PagosController/Create
        [Authorize]
        public async Task<ActionResult> Create()
        {
            var recibos = await _Iservices.GetPagosAsync();
            //var periodos = await _Iservices.GetPeriodoAsync();
            var maxNumero = recibos
               .Where(r => r.Serie == "A")
               .Max(r => (int?)r.NumeroRecibo);

            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 10001;
            var viewmodel = new PagosViewModel
            {
                //listPeriodos = periodos,
                SiguienteNumero = siguienteNumero,
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
                meses = (await _Iservices.GetMesesAsync()),

                periodo = ( _Iservices.GetPeriodoAsync().Result.Where(a => a.Activo == true && a.Actual==true))
                .Select(r => new SelectListItem
                {

                    Value = r.IdPeriodo.ToString(),
                    Text = r.Periodo.ToString(),
                }
                ).ToList(),
                gradosSelectListItem = (await _Iservices.GetGradosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdGrado.ToString(),
                    Text = r.NombreGrado,
                    //Selected = r.IdPregunta == respuestas.IdPregunta
                }).ToList(),
                recintos = (await _Iservices.GetRecintosAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto.ToString(),
                }
                ).ToList(),
                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdModalidad.ToString(),
                    Text = r.Modalidad.ToString(),
                }
                ).ToList()


            };

            return View(viewmodel);
        }

        // POST: PagosController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PagosViewModel pagos, string MesesSeleccionados)//List<int> MesesSeleccionados)
        {
            bool response = false;
            bool validarDuplicado = false;
           
            // Validar que pagos y pagos.Pago no sean null primero
            if (pagos == null || pagos.Pago == null)
            {
                TempData["Mensaje"] = "Error: Los datos del pago no fueron recibidos correctamente.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }

            // Validar campos críticos manualmente antes de validar el ModelState
            var erroresManuales = new List<string>();
            
            if (pagos.Pago.IdAlumno == 0)
            {
                erroresManuales.Add("Debe seleccionar un alumno");
            }
            
            if (pagos.Pago.IdTipoMovimiento == 0)
            {
                erroresManuales.Add("Debe seleccionar un tipo de movimiento");
            }
            
            if (pagos.Pago.IdTipoRecibo == 0)
            {
                erroresManuales.Add("Debe seleccionar un tipo de recibo");
            }
            
            if (pagos.Pago.IdMetodoPago == 0)
            {
                erroresManuales.Add("Debe seleccionar un método de pago");
            }
            
            if (pagos.Pago.IdRecinto == 0)
            {
                erroresManuales.Add("Debe seleccionar un centro de estudio");
            }
            
            if (pagos.Pago.Monto <= 0)
            {
                erroresManuales.Add("El monto debe ser mayor a cero");
            }

            if (erroresManuales.Any())
            {
                TempData["Mensaje"] = "Error de validación: " + string.Join(", ", erroresManuales);
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }

            // Limpiar errores del ModelState para campos opcionales que pueden causar problemas
            ModelState.Remove("Pago.Anyo");
            ModelState.Remove("Pago.IdMes");
            ModelState.Remove("Pago.IdModalidad"); // Puede ser opcional según el tipo de movimiento
            ModelState.Remove("Pago.IdGrado"); // Puede ser opcional según el tipo de movimiento
            ModelState.Remove("Pago.Serie"); // Se establece automáticamente en el código
            ModelState.Remove("cantidadEnLetras"); // Campo calculado, no requerido
            ModelState.Remove("MesesSeleccionados"); // Parámetro opcional del método
         
            // Validar el modelo solo para campos críticos
            if (!ModelState.IsValid)
            {
                var erroresValidacion = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();
                
                // Solo mostrar errores si son de campos críticos
                var erroresCriticos = erroresValidacion
                    .Where(e => !e.Contains("Anyo") && 
                               !e.Contains("IdMes") && 
                               !e.Contains("IdModalidad") && 
                               !e.Contains("IdGrado"))
                    .ToList();
                
                if (erroresCriticos.Any())
                {
                    TempData["Mensaje"] = "Error de validación: " + string.Join("; ", erroresCriticos);
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }
            }

            var buscarIdGuardado = await _Iservices.GetPagosAsync();
           
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var tiposMovimientoCatalogo = await _Iservices.GetTipoMovimientoAsync();
                var tipoMovimientoSeleccionado = tiposMovimientoCatalogo
                    .FirstOrDefault(t => t.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento);
                var conceptoTipoMovimiento = NormalizarTexto(tipoMovimientoSeleccionado?.Concepto);
                bool esMatriculaCompleta = pagos.Pago.IdTipoMovimiento == TipoMovimientoMatricula
                    || (conceptoTipoMovimiento.Contains("matricula") && conceptoTipoMovimiento.Contains("completa"));
                
                // Solo usar el período actual como fallback si el usuario no seleccionó ninguno
                // Si el usuario seleccionó un período en la vista, usar ese
                if (pagos.Pago.IdPeriodo == 0)
                {
                    int periodoActual = await _Iservices.GetPeriodoAsync().ContinueWith(p=>p.Result.FirstOrDefault(a=>a.Activo && a.Actual)?.IdPeriodo) ?? 0;
                    pagos.Pago.IdPeriodo = periodoActual;
                }
                // Si ya tiene un valor (seleccionado por el usuario), mantenerlo


                if (pagos != null && pagos.Pago != null)
                {
                    pagos.Pago.UsuarioRegistro = idUsuario;
                    pagos.Pago.FechaRegistro = DateTime.Now;
                    pagos.Pago.Serie = "A";
                    pagos.Pago.Activo = true;
                    if (pagos.Pago.IdTipoMovimiento == 10)
                    {
                        int mes = Convert.ToInt32(MesesSeleccionados);
                        pagos.Pago.IdMes = mes;
                        if (string.IsNullOrEmpty(MesesSeleccionados))
                        {
                            TempData["Mensaje"] = "Debe Ingresar el Mes para hacer el pago de la Rifa";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        var existePagoRifa = buscarIdGuardado.Where(a => a.IdAlumno == pagos.Pago.IdAlumno && a.IdMes <=6  && a.IdMes >= 6 && a.IdTipoMovimiento == 10).ToList();
                        if (existePagoRifa.Count > 0)
                        {

                            TempData["Mensaje"] = "Ya existe el pago de la Rifa para este semestre";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        response = await _Iservices.PostPagosAsync(pagos.Pago);
                        if (response)
                        {
                            var idPag = buscarIdGuardado.Max(a => a.IdPago);


                            TempData["Mensaje"] = "Pago registrado correctamente.";
                            TempData["Tipo"] = "success";
                            return RedirectToAction("Details", "Pagos", new { id = idPag + 1, imprimir = true });
                        }
                        else
                        {
                            TempData["Mensaje"] = "No se proceso el Pago.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                    }

                    
                    if (!string.IsNullOrEmpty(MesesSeleccionados) && pagos.Pago.IdTipoMovimiento == 1)//MesesSeleccionados != null && MesesSeleccionados.Any())
                    {
                        var ids = MesesSeleccionados.Split(',').Select(int.Parse).ToList();
                        var listpagos = await _Iservices.GetPagosAsync();

                        bool aplicoMora = false;
                        bool duplicado = false;
                        
                        if (BecaCompleta(pagos.Pago.IdAlumno,pagos.Pago.IdPeriodo).Result && pagos.Pago.IdTipoMovimiento==1)
                        {
                            TempData["Mensaje"] = "El Estudiante Posee Beca Completa.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        if (MediaBeca(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo).Result && pagos.Pago.Monto > 320 && pagos.Pago.IdTipoMovimiento==1)
                        {
                            TempData["Mensaje"] = "El Estudiante Cuenta con Media Beca.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        
                        //total = ids * mensualidad;
                        //if((ids* mensualidad)=pagos.Pago.Monto)

                        //crear meses pagados previos (mensualidad tipo 1)
                        var mesesPagadosBD = listpagos
                        .Where(p => p.IdAlumno == pagos.Pago.IdAlumno &&
                                    p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento &&
                                    p.IdPeriodo == pagos.Pago.IdPeriodo &&
                                    p.IdMes.HasValue)
                        .Select(p => p.IdMes!.Value)
                        .ToHashSet();
                        int primerMesPagadoMensual = mesesPagadosBD.Any() ? mesesPagadosBD.Min() : 1;
                        // Matrícula prematura: mes calendario de la primera matrícula (p. ej. abril) — la secuencia obligatoria
                        // empieza en ese mes, no desde enero aunque enero esté pagado con la matrícula.
                        int mesCalendarioMatricula = ObtenerMesCalendarioPrimeraMatricula(listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                        int inicioSecuenciaMensual = Math.Max(primerMesPagadoMensual, mesCalendarioMatricula);
                        //crear meses pagados virtualmente
                        var mesesPagadosAcumulados = new HashSet<int?>(mesesPagadosBD.Select(x => (int?)x));

                        var idsOrdenados = ids.OrderBy(m => m).ToList();
                        if (duplicado)
                        {
                            TempData["Mensaje"] = "Pago del mes seleccionado, ya fue registrado.";
                            TempData["Tipo"] = "warning";
                        }
                        if (aplicoMora)
                        {
                            TempData["Mensaje"] = "Pago registrado con mora de C$ 10 aplicada.";
                            TempData["Tipo"] = "info";
                        }
                        
                        foreach (var idMes in idsOrdenados)
                        {
                            // 1️⃣ Validar duplicado

                            
                            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroRecibo == pagos.Pago.NumeroRecibo && r.IdMes ==idMes && r.IdPeriodo==pagos.Pago.IdPeriodo && r.Serie == "A" && r.Activo == true);
                            if (validarDuplicado)
                            {
                                TempData["Mensaje"] = "El número de Recibo ya Existe.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }

                            if (mesesPagadosAcumulados.Contains(idMes))
                            {
                                // Evita duplicado
                                //duplicado = true;

                                TempData["Mensaje"] = $"El mes {Mes(idMes).Result} ya fue pagado por este alumno.";
                                TempData["Tipo"] = "warning";
                                continue;
                            }

                            if (idMes < mesCalendarioMatricula && !mesesPagadosBD.Contains(idMes))
                            {
                                TempData["Mensaje"] =
                                    $"Con matrícula en {Mes(mesCalendarioMatricula).Result}, no corresponde pagar por separado el mes de {Mes(idMes).Result} (matrícula prematura).";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }

                            // Meses intermedios exigidos solo entre el inicio de secuencia y el mes a pagar (no feb/mar si matrícula es abril)
                            List<int> mesesPendientes;
                            if (idMes <= inicioSecuenciaMensual)
                                mesesPendientes = new List<int>();
                            else
                            {
                                var entre = Enumerable.Range(inicioSecuenciaMensual, idMes - inicioSecuenciaMensual).ToList();
                                mesesPendientes = entre
                                    .Where(m => !mesesPagadosAcumulados.Contains(m))
                                    .ToList();
                            }

                            if (mesesPendientes.Any())
                            {
                                var primerMesFaltante = mesesPendientes.Min();

                                TempData["Mensaje"] = $"No puede pagar el mes de {Mes(idMes).Result}. Debe pagar primero el mes de {Mes(primerMesFaltante).Result}.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                
                            }
                            var hoy = DateTime.Now;
                           // if(mesesPendientes.Count > 0)
                           // {
                           // int moraPorMes = 10;
                           // decimal moraTotal = mesesPendientes.Count * moraPorMes;
                           // pagos.Mora =(int) moraTotal;
                           // pagos.MontoTotal = mensualidad + moraTotal;
                           //}
                            
                                // ejemplo: crear un pago por cada mes
                                var nuevoPago = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    
                                    NumeroRecibo=pagos.Pago.NumeroRecibo,
                                    Anyo=pagos.Pago.Anyo,
                                    IdMes = idMes,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = 1,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    IdGrado=pagos.Pago.IdGrado,
                                    IdPeriodo=pagos.Pago.IdPeriodo,
                                    IdRecinto=pagos.Pago.IdRecinto,
                                    FechaEmision=pagos.Pago.FechaEmision,
                                    Mora = pagos.Pago.Mora,
                                    Monto = pagos.Pago.Monto,
                                    Descripcion=pagos.Pago.Descripcion,
                                    UsuarioRegistro=pagos.Pago.UsuarioRegistro,
                                    Activo=pagos.Pago.Activo,
                                    FechaRegistro=pagos.Pago.FechaRegistro,
                                    Serie=pagos.Pago.Serie
                                  
                                    // otros campos...
                                };

                                //await _Iservices.InsertarPagoAsync(nuevoPago);
                                response = await _Iservices.PostPagosAsync(nuevoPago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = $"No se logro procesar el pago del mes {Mes(idMes).Result}.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                mesesPagadosAcumulados.Add(idMes);
                        }
                        
                        // Si todos los meses se procesaron correctamente, obtener el ID del último pago
                        var pagosActualizados = await _Iservices.GetPagosAsync();
                        var idPag = pagosActualizados.Max(a => a.IdPago);
                        TempData["Mensaje"] = "Pago registrado correctamente.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Details", "Pagos", new { id = idPag, imprimir = true });
                        // 4️⃣ Mensaje final
                       
                        
                        

                    }
                    else
                    {
                        if (esMatriculaCompleta)
                        {
                            int periodoMatricula = pagos.Pago.IdPeriodo;
                            
                            // Validar que los campos requeridos estén presentes
                            if (pagos.Pago.IdGrado == 0)
                            {
                                TempData["Mensaje"] = "Debe seleccionar el Nivel (Grado) para procesar la matrícula completa.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }
                            
                            if (!pagos.Pago.IdModalidad.HasValue || pagos.Pago.IdModalidad.Value == 0)
                            {
                                TempData["Mensaje"] = "Debe seleccionar la Modalidad para procesar la matrícula completa.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }
                            
                            // Mensualidad que acompaña a matrícula completa: siempre se registra como ENERO (IdMes = 1).
                            // No usar el mes calendario de la fecha de matrícula (ej. matricular en marzo no implica IdMes=3).
                            const int idMesMensualidadConMatricula = 1;
                            
                            decimal restarMensualidad = await ObtenerMensualidadDecimal(
                                pagos.Pago.IdRecinto, pagos.Pago.IdGrado, periodoMatricula, pagos.Pago.IdModalidad);
                            decimal obtenerMat = await ObtenerMatriculaDecimal(pagos.Pago.IdRecinto, pagos.Pago.IdModalidad, periodoMatricula);
                            
                            // Validar que se obtuvieron los valores correctamente
                            if (obtenerMat == 0)
                            {
                                TempData["Mensaje"] = "No se encontró el costo de matrícula para los parámetros seleccionados.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            var totalMatricula = Convert.ToDecimal(pagos.Pago.Monto);
                            
                            // Calcular cuánto se ha pagado ya de matrícula (tipo 2 o 4)
                            var pagosMatriculaPrevios = _Iservices.GetPagosAsync().Result
                                .Where(a => a.IdAlumno == pagos.Pago.IdAlumno && 
                                           (a.IdTipoMovimiento == 2 || a.IdTipoMovimiento == 4) && 
                                           a.IdPeriodo == periodoMatricula &&
                                           a.Activo == true)
                                .ToList();
                            
                            decimal totalPagadoMatricula = pagosMatriculaPrevios.Sum(p => p.Monto);
                            decimal faltaPorPagar = obtenerMat - totalPagadoMatricula;

                            if (restarMensualidad <= 0)
                            {
                                TempData["Mensaje"] = "No hay mensualidad configurada para este recinto/grado/período; no se puede separar el pago de enero con la matrícula.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            // Si ya se pagó el total completo de matrícula, no permitir más pagos
                            if (faltaPorPagar <= 0.01m)
                            {
                                TempData["Mensaje"] = "La matrícula ya está completamente pagada.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            // El monto ingresado es matrícula + mensualidad de enero (IdMes=1), no meses calendario (feb/mar).
                            // parteMatricula = lo que corresponde solo a matrícula después de descontar la mensualidad de enero.
                            decimal parteMatricula = totalMatricula - restarMensualidad;

                            if (totalMatricula + 0.01m < restarMensualidad)
                            {
                                TempData["Mensaje"] = $"El monto (C$ {totalMatricula:N2}) debe cubrir al menos la mensualidad de enero (C$ {restarMensualidad:N2}) que se registra junto a este pago.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            if (parteMatricula > faltaPorPagar + 0.01m)
                            {
                                decimal maxPermitido = faltaPorPagar + restarMensualidad;
                                TempData["Mensaje"] = $"La parte destinada a matrícula (C$ {parteMatricula:N2}) supera lo pendiente (C$ {faltaPorPagar:N2}). " +
                                    $"Monto ingresado: C$ {totalMatricula:N2} (matrícula pendiente máx. C$ {faltaPorPagar:N2} + mensualidad enero C$ {restarMensualidad:N2}; máximo permitido C$ {maxPermitido:N2}). " +
                                    $"Total matrícula en catálogo: C$ {obtenerMat:N2}, ya pagado matrícula: C$ {totalPagadoMatricula:N2}.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            if (parteMatricula < -0.01m)
                            {
                                TempData["Mensaje"] = "El monto no es coherente con la mensualidad de enero configurada.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            // Cierra matrícula cuando la parte matrícula iguala exactamente lo que falta (típico: falta + mensualidad en un solo recibo)
                            bool esMontoCompleto = Math.Abs(parteMatricula - faltaPorPagar) < 0.01m;
                            
                            // Si el monto completa el pago pendiente de matrícula, procesar como matrícula completa
                            if (esMontoCompleto)
                            {
                                // El monto ingresado completa lo que falta, restar la mensualidad
                                decimal valormatricula = totalMatricula - restarMensualidad;
                                pagos.Pago.Monto = valormatricula;
                                pagos.Pago.IdPeriodo = periodoMatricula;
                                response = await _Iservices.PostPagosAsync(pagos.Pago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el pago de matrícula.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                // Obtener el ID del pago de matrícula recién guardado para mostrar su recibo (no el del primer mes)
                                var pagosDespuesMatricula = await _Iservices.GetPagosAsync();
                                int idPagoMatricula = pagosDespuesMatricula
                                    .Where(p => p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento && p.NumeroRecibo == pagos.Pago.NumeroRecibo && p.IdAlumno == pagos.Pago.IdAlumno)
                                    .OrderByDescending(p => p.IdPago)
                                    .Select(p => p.IdPago)
                                    .FirstOrDefault();
                                
                                var pagoprimermes = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    NumeroRecibo = pagos.Pago.NumeroRecibo,
                                    Anyo = pagos.Pago.Anyo,
                                    IdMes = idMesMensualidadConMatricula,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = 1,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    IdGrado = pagos.Pago.IdGrado,
                                    IdPeriodo = periodoMatricula,
                                    IdRecinto = pagos.Pago.IdRecinto,
                                    FechaEmision = pagos.Pago.FechaEmision,
                                    Mora = 0,
                                    Monto = restarMensualidad,
                                    Descripcion = pagos.Pago.Descripcion,
                                    UsuarioRegistro = pagos.Pago.UsuarioRegistro,
                                    Activo = pagos.Pago.Activo,
                                    FechaRegistro = pagos.Pago.FechaRegistro,
                                    Serie = pagos.Pago.Serie
                                };
                                
                                response = await _Iservices.PostPagosAsync(pagoprimermes);
                                if (response)
                                {
                                    // Redirigir al recibo del pago de matrícula (tipoMovimiento Matrícula), no al del mes de enero
                                    int idParaRecibo = idPagoMatricula > 0 ? idPagoMatricula : (await _Iservices.GetPagosAsync()).Max(a => a.IdPago);
                                    TempData["Mensaje"] = "Pago registrado correctamente.";
                                    TempData["Tipo"] = "success";
                                    return RedirectToAction("Details", "Pagos", new { id = idParaRecibo, imprimir = true });
                                }
                                else
                                {
                                    TempData["Mensaje"] = "No se proceso el Pago del primer mes.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                            }
                            else
                            {
                                // Si el monto es diferente (pago parcial), procesar como abono de matrícula
                                // Restar siempre la mensualidad del monto, similar al if anterior
                                decimal valormatricula = totalMatricula - restarMensualidad;
                                pagos.Pago.Monto = valormatricula;
                                pagos.Pago.IdPeriodo = periodoMatricula;
                                response = await _Iservices.PostPagosAsync(pagos.Pago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el abono de matrícula.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                // Obtener el ID del pago de matrícula recién guardado para mostrar su recibo (no el del primer mes)
                                var pagosDespuesAbonoMat = await _Iservices.GetPagosAsync();
                                int idPagoMatriculaAbono = pagosDespuesAbonoMat
                                    .Where(p => p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento && p.NumeroRecibo == pagos.Pago.NumeroRecibo && p.IdAlumno == pagos.Pago.IdAlumno)
                                    .OrderByDescending(p => p.IdPago)
                                    .Select(p => p.IdPago)
                                    .FirstOrDefault();
                                
                                // Mensualidad de enero con el valor de la mensualidad del grado
                                var pagoprimermes = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    NumeroRecibo = pagos.Pago.NumeroRecibo,
                                    Anyo = pagos.Pago.Anyo,
                                    IdMes = idMesMensualidadConMatricula,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = 1,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    IdGrado = pagos.Pago.IdGrado,
                                    IdPeriodo = periodoMatricula,
                                    IdRecinto = pagos.Pago.IdRecinto,
                                    FechaEmision = pagos.Pago.FechaEmision,
                                    Mora = 0,
                                    Monto = restarMensualidad,
                                    Descripcion = pagos.Pago.Descripcion,
                                    UsuarioRegistro = pagos.Pago.UsuarioRegistro,
                                    Activo = pagos.Pago.Activo,
                                    FechaRegistro = pagos.Pago.FechaRegistro,
                                    Serie = pagos.Pago.Serie
                                };
                                
                                response = await _Iservices.PostPagosAsync(pagoprimermes);
                                if (response)
                                {
                                    // Redirigir al recibo del pago de matrícula (tipoMovimiento Matrícula), no al del primer mes
                                    int idParaRecibo = idPagoMatriculaAbono > 0 ? idPagoMatriculaAbono : (await _Iservices.GetPagosAsync()).Max(a => a.IdPago);
                                    TempData["Mensaje"] = "El abono de matrícula y el pago de enero se registraron correctamente.";
                                    TempData["Tipo"] = "success";
                                    return RedirectToAction("Details", "Pagos", new { id = idParaRecibo, imprimir = true });
                                }
                                else
                                {
                                    TempData["Mensaje"] = "No se proceso el Pago del primer mes.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                            }
                           
                        }
                        else
                        {
                            //pagos.Pago.IdMes = 0;
                            response = await _Iservices.PostPagosAsync(pagos.Pago);
                            if (response)
                            {
                                var idPag = buscarIdGuardado.Max(a => a.IdPago);


                                TempData["Mensaje"] = "Pago registrado correctamente.";
                                TempData["Tipo"] = "success";
                                return RedirectToAction("Details", "Pagos", new { id = idPag + 1, imprimir = true });
                            }
                            else
                            {
                                TempData["Mensaje"] = "No se proceso el Pago.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }
                        }
                    }
                }
                else
                {
                    TempData["Mensaje"] = "Error: Los datos del pago no son válidos.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                // En caso de error, redirigir a Create para que se cargue el modelo correctamente
                TempData["Mensaje"] = $"Ocurrió un error al procesar el pago: {ex.Message}";
                TempData["Tipo"] = "warning";
                // Log del error completo para debugging
                System.Diagnostics.Debug.WriteLine($"Error en Create de Pagos: {ex}");
                return RedirectToAction("Create");
            }
        }

        //Calcular meses atrasados
        public int CalcularMesesAtrasados(DateTime fechaDeuda, DateTime fechaActual)
        {
            // Normalizar días (solo comparar por año y mes)
            fechaDeuda = new DateTime(fechaDeuda.Year, fechaDeuda.Month, 1);
            fechaActual = new DateTime(fechaActual.Year, fechaActual.Month, 1);

            int meses = (fechaActual.Year - fechaDeuda.Year) * 12 + (fechaActual.Month - fechaDeuda.Month);
            if (fechaActual.Day < fechaDeuda.Day)
            {
                meses--;
            }

            return meses < 0 ? 0 : meses;
        }

        //calcular mora
        //public int CalcularMoraTotal(DateTime fechaDeuda, DateTime fechaActual)
        //{

        //    return mesesAtrasados * MoraPorMes;
        //}
        [Authorize]
        [HttpGet]
        public IActionResult ObtenerMora(int idAlumno, int idTipoMovimiento, int periodo)
        {
            const int idTipoMensualidad = 1;
            if (idTipoMovimiento != idTipoMensualidad)
            {
                return Json(new { mora = 0, mes = DateTime.Now.Month, aplicaMora = false });
            }

            int mes = DateTime.Now.Month;
            var listpagos = _Iservices.GetPagosAsync().Result;
            var pagosMensualidad = listpagos
                .Where(p => p.IdAlumno == idAlumno &&
                            p.IdTipoMovimiento == idTipoMensualidad &&
                            p.IdPeriodo == periodo &&
                            p.IdMes.HasValue)
                .ToList();
            var mesesPagadosBD = pagosMensualidad.Select(p => p.IdMes!.Value).ToHashSet();

            int mesMatricula = ObtenerMesCalendarioPrimeraMatricula(listpagos, idAlumno, periodo);

            // Mora únicamente desde el mes calendario de la matrícula: no se cobra mora por meses anteriores (ene–mar si matrícula es abril).
            if (mesMatricula > mes)
            {
                return Json(new { mora = 0, mes, aplicaMora = false });
            }

            int cantidadMesesEnVentana = mes - mesMatricula;
            if (cantidadMesesEnVentana <= 0)
            {
                return Json(new { mora = 0, mes, aplicaMora = false });
            }

            var mesesPendientes = Enumerable.Range(mesMatricula, cantidadMesesEnVentana)
                .Except(mesesPagadosBD)
                .Distinct()
                .ToList();
            int moraTotal = MoraPorMes * mesesPendientes.Count;
            bool aplica = moraTotal > 0;

            return Json(new { mora = moraTotal, mes, aplicaMora = aplica });
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

        /// <summary>
        /// Busca el costo de mensualidad como en estado de cuenta, con tolerancias:
        /// recinto + grado + ciclo + modalidad → modalidad 0 (todas) → cualquier modalidad del mismo grado;
        /// si no hay fila por grado, una única fila activa para recinto+ciclo+modalidad (sin desglose por grado).
        /// </summary>
        private static TblCostoMensualidad? ResolverFilaCostoMensualidad(
            IList<TblCostoMensualidad>? list,
            int? idRecinto,
            int idGrado,
            int idPeriodo,
            int? idModalidad)
        {
            if (list == null || list.Count == 0)
                return null;

            IEnumerable<TblCostoMensualidad> Base() =>
                list.Where(x => x.IdRecinto == idRecinto &&
                                x.IdPeriodo == idPeriodo &&
                                x.Activo);

            var porGrado = Base().Where(x => x.IdGrado == idGrado).ToList();

            TblCostoMensualidad? EligePorModalidad(IReadOnlyList<TblCostoMensualidad> src)
            {
                if (src.Count == 0) return null;
                if (idModalidad.HasValue && idModalidad.Value > 0)
                {
                    var exacta = src.FirstOrDefault(x => x.IdModalidad == idModalidad.Value);
                    if (exacta != null) return exacta;
                    var wildcard = src.FirstOrDefault(x => x.IdModalidad == 0);
                    if (wildcard != null) return wildcard;
                }
                return src.FirstOrDefault();
            }

            var fila = EligePorModalidad(porGrado);
            if (fila != null)
                return fila;

            if (!idModalidad.HasValue || idModalidad.Value <= 0)
                return null;

            var soloModalidad = Base().Where(x => x.IdModalidad == idModalidad.Value).ToList();
            if (soloModalidad.Count == 1)
                return soloModalidad[0];
            var soloWildcard = Base().Where(x => x.IdModalidad == 0).ToList();
            if (soloWildcard.Count == 1)
                return soloWildcard[0];

            return null;
        }

        /// <summary>
        /// Misma lógica flexible que <see cref="ObtenerMensualidad"/> (desglose matrícula completa / servidor).
        /// </summary>
        public async Task<decimal> ObtenerMensualidadDecimal(
            int? idRecinto, int idGrado, int idPeriodo, int? idModalidad = null)
        {
            var list = await _Iservices.GetCostosMensualidadAsync();
            var fila = ResolverFilaCostoMensualidad(list, idRecinto, idGrado, idPeriodo, idModalidad);
            return fila != null ? (decimal)fila.CostoMensualidad : 0m;
        }
        public async Task<decimal> ObtenerMatriculaDecimal(
    int? idRecinto, int? idModalidad, int idPeriodo)
        {
            return await _Iservices.GetCostosMatriculaAsync()
                .ContinueWith(t => t.Result
                    .Where(x => x.IdRecinto == idRecinto &&
                                 x.IdModalidad == idModalidad &&
                                x.IdPeriodo == idPeriodo &&
                                x.Activo)
                    .Select(x => x.CostoMatricula)
                    .FirstOrDefault());
        }

        /// <summary>
        /// Precarga recinto, modalidad, grado y ciclo lectivo actual para matrícula/mensualidad según ficha del alumno.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<JsonResult> ObtenerDatosAlumnoMatricula(int idAlumno)
        {
            if (idAlumno <= 0)
                return Json(new { ok = false });

            var alumno = await _Iservices.GetAlumnoIdAsync(idAlumno);
            if (alumno == null || alumno.IdAlumno <= 0)
                return Json(new { ok = false, mensaje = "Alumno no encontrado" });

            var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoActual = periodos.FirstOrDefault(a => a.Activo && a.Actual);

            return Json(new
            {
                ok = true,
                idRecinto = alumno.IdRecinto,
                idModalidad = alumno.IdModalidad,
                idGrado = alumno.IdGrado,
                idPeriodo = periodoActual?.IdPeriodo ?? alumno.IdPeriodo
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult ObtenerMensualidad(int? idRecinto, int idGrado, int idPeriodo, int? idModalidad = null)
        {
            var list = _Iservices.GetCostosMensualidadAsync().Result;
            var fila = ResolverFilaCostoMensualidad(list, idRecinto, idGrado, idPeriodo, idModalidad);

            if (fila == null)
                return Json(new { costo = 0, sinConfiguracion = true });

            return Json(new { costo = fila.CostoMensualidad, sinConfiguracion = false });
        }
        [HttpGet]
        [Authorize]
        public IActionResult ObtenerMatricula(int? idRecinto, int? idModalidad, int idPeriodo)
        {
            var matricula= _Iservices.GetCostosMatriculaAsync().Result
                .Where(x => x.IdRecinto == idRecinto &&
                            x.IdModalidad == idModalidad &&
                            //x.IdModalidad == idModalidad &&
                            x.IdPeriodo == idPeriodo &&
                            x.Activo == true)
                .Select(x => new {
                    costo = x.CostoMatricula
                })
                .FirstOrDefault();

            return Json(matricula);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ObtenerAbonosPrevios(int idAlumno, int idTipoMovimiento, int idPeriodo)
        {
            try
            {
                var listpagos = _Iservices.GetPagosAsync().Result;
                var tiposMovimiento = _Iservices.GetTipoMovimientoAsync().Result;
                
                // Buscar abonos previos según el tipo de movimiento
                decimal totalAbonos = 0;
                
                if (idTipoMovimiento == 2 || idTipoMovimiento == 4) // Matrícula Completa o Confirmación de Matrícula
                {
                    // Si es Confirmación de Matrícula (tipo 4), buscar abonos previos del mismo tipo
                    // Si es Matrícula Completa (tipo 2), buscar abonos de confirmación de matrícula (tipo 4)
                    int tipoAbonoBuscado = idTipoMovimiento == 4 ? 4 : 4; // Ambos casos buscan tipo 4
                    
                    // Buscar abonos de confirmación de matrícula
                    var abonosMatricula = listpagos
                        .Where(p => p.IdAlumno == idAlumno &&
                                    p.IdPeriodo == idPeriodo &&
                                    p.IdTipoMovimiento == tipoAbonoBuscado &&
                                    p.Activo == true)
                        .ToList();
                    
                    totalAbonos = abonosMatricula.Sum(p => p.Monto);
                    
                    // Si no encuentra por ID, intentar buscar por concepto
                    if (totalAbonos == 0)
                    {
                        var tipoConfirmacion = tiposMovimiento
                            .FirstOrDefault(tm => tm.Concepto.ToLower().Contains("confirmación") || 
                                                 tm.Concepto.ToLower().Contains("reserva") ||
                                                 (tm.Concepto.ToLower().Contains("abono") && tm.Concepto.ToLower().Contains("matrícula")));
                        
                        if (tipoConfirmacion != null && tipoConfirmacion.IdTipoMovimiento != idTipoMovimiento)
                        {
                            var abonosPorConcepto = listpagos
                                .Where(p => p.IdAlumno == idAlumno &&
                                            p.IdPeriodo == idPeriodo &&
                                            p.IdTipoMovimiento == tipoConfirmacion.IdTipoMovimiento &&
                                            p.Activo == true)
                                .ToList();
                            
                            totalAbonos = abonosPorConcepto.Sum(p => p.Monto);
                        }
                    }
                }
                else if (idTipoMovimiento == 1) // Mensualidad
                {
                    // Buscar abonos de mensualidad previos
                    // Buscar el tipo de movimiento que corresponde a "Abono de Mensualidad"
                    var tipoAbonoMensualidad = tiposMovimiento
                        .FirstOrDefault(tm => (tm.Concepto.ToLower().Contains("Abono Mensualidad") && 
                                               tm.Concepto.ToLower().Contains("Mensualidad")) ||
                                              tm.Concepto.ToLower().Contains("abono mensualidad"));
                    
                    if (tipoAbonoMensualidad != null)
                    {
                        // Buscar abonos de mensualidad
                        var abonosMensualidad = listpagos
                            .Where(p => p.IdAlumno == idAlumno &&
                                        p.IdPeriodo == idPeriodo &&
                                        p.IdTipoMovimiento == tipoAbonoMensualidad.IdTipoMovimiento &&
                                        p.Activo == true)
                            .ToList();
                        
                        totalAbonos = abonosMensualidad.Sum(p => p.Monto);
                    }
                    // Si no hay tipo específico de abono de mensualidad, no se restan abonos
                    // porque las mensualidades pagadas no son abonos, son pagos completos
                }

                return Json(new { 
                    totalAbonos = totalAbonos,
                    tieneAbonos = totalAbonos > 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    totalAbonos = 0,
                    tieneAbonos = false,
                    error = ex.Message
                });
            }
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
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            var pago = await _Iservices.GetPagoById(id);
            
            if (pago == null)
            {
                TempData["Mensaje"] = "Pago no encontrado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var viewmodel = new PagosViewModel
            {
                Pago = pago,
                SiguienteNumero = pago.NumeroRecibo ?? 0,
                tipoMovimientoSelectListItem = (await _Iservices.GetTipoMovimientoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoMovimiento.ToString(),
                                       Text = r.Concepto,
                                       Selected = r.IdTipoMovimiento == pago.IdTipoMovimiento
                                   }).ToList(),
                tipoRecibosSelectListItem = (await _Iservices.GetTipoReciboAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoRecibo.ToString(),
                                       Text = r.TipoRecibo,
                                       Selected = r.IdTipoRecibo == pago.IdTipoRecibo
                                   }).ToList(),
                metodoPagoSelectListItem = (await _Iservices.GetMetodoPagoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdMetodoPago.ToString(),
                                       Text = r.MetodoPago,
                                       Selected = r.IdMetodoPago == pago.IdMetodoPago
                                   }).ToList(),
                meses = await _Iservices.GetMesesAsync(),
                periodo = (await _Iservices.GetPeriodoAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdPeriodo.ToString(),
                    Text = r.Periodo.ToString(),
                    Selected = r.IdPeriodo == pago.IdPeriodo
                }).ToList(),
                gradosSelectListItem = (await _Iservices.GetGradosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdGrado.ToString(),
                    Text = r.NombreGrado,
                    Selected = r.IdGrado == pago.IdGrado
                }).ToList(),
                recintos = (await _Iservices.GetRecintosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto.ToString(),
                    Selected = r.IdRecinto == pago.IdRecinto
                }).ToList(),
                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdModalidad.ToString(),
                    Text = r.Modalidad.ToString(),
                    Selected = r.IdModalidad == pago.IdModalidad
                }).ToList(),
                alumnos = await _Iservices.GetAlumnosAsync()
            };

            return View(viewmodel);
        }

        // POST: PagosController/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, PagosViewModel pagosViewModel)
        {
            try
            {
                if (id != pagosViewModel.Pago.IdPago)
                {
                    TempData["Mensaje"] = "El ID del pago no coincide.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener el pago original para preservar algunos campos
                var pagoOriginal = await _Iservices.GetPagoById(id);
                if (pagoOriginal == null)
                {
                    TempData["Mensaje"] = "Pago no encontrado.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Index));
                }

                // Actualizar campos
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                pagosViewModel.Pago.UsuarioActualizo = idUsuario;
                pagosViewModel.Pago.FechaActualizo = DateTime.Now;
                
                // Preservar campos que no deben cambiar
                pagosViewModel.Pago.UsuarioRegistro = pagoOriginal.UsuarioRegistro;
                pagosViewModel.Pago.FechaRegistro = pagoOriginal.FechaRegistro;
                pagosViewModel.Pago.Serie = pagoOriginal.Serie; // No cambiar la serie

                // Llamar al servicio de actualización
                bool response = await _Iservices.UpdatePago(pagosViewModel.Pago);

                if (response)
                {
                    TempData["Mensaje"] = "Pago actualizado correctamente.";
                    TempData["Tipo"] = "success";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    TempData["Mensaje"] = "No se pudo actualizar el pago.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Ocurrió un error al actualizar el pago: {ex.Message}";
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
        }

        // GET: PagosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PagosController/Delete/5
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
