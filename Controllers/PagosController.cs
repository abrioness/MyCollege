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

            var _pagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();

            if (!_pagos.Any() && !string.IsNullOrWhiteSpace(_Iservices.LastApiError))
            {
                TempData["Mensaje"] = "No se pudieron cargar los pagos en este momento. Verifique que la API esté en ejecución, cierre sesión e ingrese de nuevo.";
                TempData["Tipo"] = "warning";
            }
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

            ViewBag.FiltroFechaActivo = ini.HasValue || fin.HasValue;
            ViewBag.TotalPagosApi = _pagos.Count;
            ViewBag.TotalPagosMostrados = pagosFiltrados.Count;
   
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
            var p = ObtenerMatriculaReferencia(pagos, idAlumno, idPeriodo);
            if (p == null)
                return 1;
            var m = p.FechaEmision?.Month ?? p.FechaRegistro.Month;
            return Math.Clamp(m, 1, 12);
        }

        /// <summary>
        /// Matrícula habitual de inicio de ciclo (oct–dic para el año lectivo siguiente).
        /// En ese caso las mensualidades siguen ene→feb→… aunque la matrícula se haya registrado en octubre.
        /// La regla de “matrícula prematura” solo aplica si la matrícula es entre febrero y septiembre.
        /// </summary>
        private static bool EsMatriculaInicioCicloLectivo(int mesCalendarioMatricula)
            => mesCalendarioMatricula >= 10;

        /// <summary>True si la primera matrícula del período se marcó como continuidad (reingreso a mitad de año).</summary>
        private static bool EsMatriculaContinuidad(IEnumerable<TblPago> pagos, int idAlumno, int idPeriodo)
        {
            var p = ObtenerMatriculaReferencia(pagos, idAlumno, idPeriodo);
            return p?.Continuidad == true;
        }

        private static int CalcularInicioSecuenciaMensual(
            bool matriculaInicioCiclo,
            bool esContinuidad,
            int mesCalendarioMatricula,
            int primerMesPagadoMensual)
        {
            if (esContinuidad)
                return 1;
            if (matriculaInicioCiclo)
                return primerMesPagadoMensual > 0 ? primerMesPagadoMensual : 1;

            return Math.Max(primerMesPagadoMensual, mesCalendarioMatricula);
        }

        private static int ResolverIdPagoTrasRegistro(int idDesdeApi, IEnumerable<TblPago>? pagos, TblPago referencia)
        {
            if (idDesdeApi > 0)
                return idDesdeApi;
            if (pagos == null)
                return 0;

            return pagos
                .Where(p => p.IdAlumno == referencia.IdAlumno
                    && p.NumeroRecibo == referencia.NumeroRecibo
                    && p.IdTipoMovimiento == referencia.IdTipoMovimiento
                    && p.Activo)
                .OrderByDescending(p => p.IdPago)
                .Select(p => p.IdPago)
                .FirstOrDefault();
        }

        private static int ResolverIdPagoMaximoSeguro(IEnumerable<TblPago>? pagos)
            => pagos != null && pagos.Any() ? pagos.Max(p => p.IdPago) : 0;

        private static TblPago? ObtenerMatriculaReferencia(
            IEnumerable<TblPago> pagos, int idAlumno, int idPeriodo)
        {
            var matriculas = pagos
                .Where(p => p.IdAlumno == idAlumno && p.Activo
                    && (p.IdTipoMovimiento == TipoMovimientoMatricula || p.IdTipoMovimiento == TipoMovimientoMatriculaAbono))
                .ToList();
            if (matriculas.Count == 0)
                return null;

            if (idPeriodo > 0)
            {
                var enPeriodo = matriculas
                    .Where(p => p.IdPeriodo == idPeriodo)
                    .OrderBy(p => p.FechaRegistro)
                    .FirstOrDefault();
                if (enPeriodo != null)
                    return enPeriodo;
            }

            return matriculas.OrderByDescending(p => p.FechaRegistro).FirstOrDefault();
        }

        private static HashSet<int> ResolverPeriodosConsultaPago(
            IEnumerable<TblPago> pagos, int idAlumno, int idPeriodoFormulario)
        {
            var periodos = new HashSet<int>();
            if (idPeriodoFormulario > 0)
                periodos.Add(idPeriodoFormulario);

            var matricula = ObtenerMatriculaReferencia(pagos, idAlumno, idPeriodoFormulario);
            if (matricula != null && matricula.IdPeriodo > 0)
                periodos.Add(matricula.IdPeriodo);

            return periodos;
        }

        /// <summary>
        /// Toda matrícula (con o sin continuidad) incluye enero en el paquete. Si falta el registro tipo 1 mes 1
        /// en BD, la validación de mensualidad debe tratar enero como pagado.
        /// </summary>
        private static void IncluirEneroPagadoConMatricula(
            HashSet<int> mesesPagados,
            IEnumerable<TblPago> pagos,
            int idAlumno,
            int idPeriodo)
        {
            if (mesesPagados.Contains(1))
                return;

            if (ObtenerMatriculaReferencia(pagos, idAlumno, idPeriodo) != null)
                mesesPagados.Add(1);
        }

        private static bool ExisteMensualidadEneroEnBd(
            IEnumerable<TblPago> pagos, int idAlumno, int idPeriodoFormulario)
        {
            var periodos = ResolverPeriodosConsultaPago(pagos, idAlumno, idPeriodoFormulario);
            return pagos.Any(p => p.Activo
                && p.IdAlumno == idAlumno
                && periodos.Contains(p.IdPeriodo)
                && p.IdTipoMovimiento == TipoMovimientoMensualidad
                && p.IdMes == 1);
        }

        private async Task<(bool Exito, string? Error)> AsegurarEneroRegistradoConMatricula(
            TblPago plantilla,
            int periodoMatricula,
            decimal montoMensualidadEnero)
        {
            if (montoMensualidadEnero <= 0)
                return (false, "No hay mensualidad de enero configurada.");

            var pagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            if (ExisteMensualidadEneroEnBd(pagos, plantilla.IdAlumno, periodoMatricula))
                return (true, null);

            const int idMesEnero = 1;
            var pagoEnero = new TblPago
            {
                IdAlumno = plantilla.IdAlumno,
                NumeroRecibo = plantilla.NumeroRecibo,
                Anyo = plantilla.Anyo,
                IdMes = idMesEnero,
                IdTipoRecibo = plantilla.IdTipoRecibo,
                IdTipoMovimiento = TipoMovimientoMensualidad,
                IdMetodoPago = plantilla.IdMetodoPago,
                IdGrado = plantilla.IdGrado,
                IdPeriodo = periodoMatricula,
                IdRecinto = plantilla.IdRecinto,
                IdModalidad = plantilla.IdModalidad,
                FechaEmision = plantilla.FechaEmision,
                Mora = 0,
                Monto = montoMensualidadEnero,
                Descripcion = plantilla.Descripcion,
                UsuarioRegistro = plantilla.UsuarioRegistro,
                Activo = plantilla.Activo,
                FechaRegistro = plantilla.FechaRegistro,
                Serie = plantilla.Serie,
                Continuidad = false
            };

            var (ok, _, err) = await _Iservices.PostPagosAsync(pagoEnero);
            return (ok, err);
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

                var costoMat = ResolverFilaCostoMatricula(costosMat, idR, idPeriodoRef, idMod);
                decimal montoMat = costoMat != null ? (decimal)costoMat.CostoMatricula : 0m;

                var pagosAlum = pagos.Where(p => p.IdAlumno == alumno.IdAlumno && p.IdPeriodo == idPeriodoRef && p.Activo).ToList();

                decimal pagadoMat = pagosAlum
                    .Where(p => p.IdTipoMovimiento == TipoMovimientoMatricula || p.IdTipoMovimiento == TipoMovimientoMatriculaAbono)
                    .Sum(p => p.Monto);
                decimal pagadoEneroConMatricula = pagosAlum
                    .Where(p => p.IdTipoMovimiento == TipoMovimientoMensualidad && p.IdMes == 1)
                    .Sum(p => p.Monto);
                var estadoMat = CalcularEstadoMatriculaEstadoCuenta(montoMat, montoMensual, pagadoMat, pagadoEneroConMatricula);
                bool matCancelada = estadoMat.Cancelada;
                decimal saldoMat = estadoMat.SaldoPendiente;

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
                }).ToList(),
                usuariosSelectListItem = (await _Iservices.GetUsuariosAsync())
                .Select(u => new SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = u.NombreCompleto ?? u.NombreUsuario,
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
                    decimal montoMensualidadRecibo = pagosMismoRecibo.Sum(p => p.Monto);
                    decimal moraRecibo = pagosMismoRecibo
                        .Select(p => p.Mora ?? 0)
                        .Where(m => m > 0)
                        .DefaultIfEmpty(0)
                        .Max();
                    decimal totalPagarRecibo = pagosMismoRecibo
                        .Where(p => p.TotalPagar.HasValue && p.TotalPagar.Value > 0)
                        .Select(p => p.TotalPagar!.Value)
                        .DefaultIfEmpty(0m)
                        .Max();
                    if (totalPagarRecibo <= 0)
                        totalPagarRecibo = montoMensualidadRecibo + moraRecibo;

                    if (montoMensualidadRecibo > 0 || moraRecibo > 0 || totalPagarRecibo > 0)
                    {
                        viewModel.MontoMensualidadRecibo = montoMensualidadRecibo;
                        viewModel.MoraRecibo = moraRecibo;
                        viewModel.TotalPagarRecibo = totalPagarRecibo;
                        viewModel.MontoTotalRecibo = totalPagarRecibo;
                        viewModel.CantidadEnLetrasTotalRecibo = NumeroALetras(totalPagarRecibo);
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
                    || pagos.Pago.IdTipoMovimiento == TipoMovimientoMatriculaAbono
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
                        (response, var idPagoRifa, _) = await _Iservices.PostPagosAsync(pagos.Pago);
                        if (response)
                        {
                            var idPag = idPagoRifa > 0 ? idPagoRifa : ResolverIdPagoMaximoSeguro(buscarIdGuardado) + 1;


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
                        var listpagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
                        var periodosConsulta = ResolverPeriodosConsultaPago(
                            listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);

                        bool aplicoMora = false;
                        bool duplicado = false;
                        
                        if (BecaCompleta(pagos.Pago.IdAlumno,pagos.Pago.IdPeriodo).Result && pagos.Pago.IdTipoMovimiento==1)
                        {
                            TempData["Mensaje"] = "El Estudiante Posee Beca Completa.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        decimal tarifaMensual = await ObtenerMensualidadDecimal(
                            pagos.Pago.IdRecinto,
                            pagos.Pago.IdGrado,
                            pagos.Pago.IdPeriodo,
                            pagos.Pago.IdModalidad);

                        if (tarifaMensual <= 0)
                        {
                            TempData["Mensaje"] = "No hay mensualidad configurada para este recinto, nivel, ciclo y modalidad.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }

                        // Reparar enero faltante si ya hay matrícula sin mensualidad mes 1 en BD
                        var matriculaExistente = ObtenerMatriculaReferencia(
                            listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                        if (matriculaExistente != null
                            && !ExisteMensualidadEneroEnBd(listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo))
                        {
                            int periodoMatriculaRef = matriculaExistente.IdPeriodo;
                            var plantillaEnero = new TblPago
                            {
                                IdAlumno = matriculaExistente.IdAlumno,
                                NumeroRecibo = matriculaExistente.NumeroRecibo,
                                Anyo = matriculaExistente.Anyo,
                                IdTipoRecibo = matriculaExistente.IdTipoRecibo,
                                IdMetodoPago = matriculaExistente.IdMetodoPago,
                                IdGrado = matriculaExistente.IdGrado > 0 ? matriculaExistente.IdGrado : pagos.Pago.IdGrado,
                                IdRecinto = matriculaExistente.IdRecinto,
                                IdModalidad = pagos.Pago.IdModalidad,
                                FechaEmision = matriculaExistente.FechaEmision,
                                Descripcion = matriculaExistente.Descripcion,
                                UsuarioRegistro = pagos.Pago.UsuarioRegistro,
                                Activo = true,
                                FechaRegistro = DateTime.Now,
                                Serie = matriculaExistente.Serie
                            };
                            var (reparado, errorReparo) = await AsegurarEneroRegistradoConMatricula(
                                plantillaEnero, periodoMatriculaRef, tarifaMensual);
                            listpagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
                            periodosConsulta = ResolverPeriodosConsultaPago(
                                listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                            if (!reparado)
                            {
                                TempData["Mensaje"] = "No se pudo registrar la mensualidad de enero pendiente de la matrícula. "
                                    + (errorReparo ?? "Verifique la API y el catálogo de mensualidad.");
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }
                        }

                        bool tieneMediaBeca = await MediaBeca(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                        if (tieneMediaBeca)
                            tarifaMensual *= 0.5m;

                        decimal montoPorMes = tarifaMensual;
                        int cantidadMesesPago = ids.Count;
                        decimal totalMensualidadesEsperado = montoPorMes * cantidadMesesPago;

                        decimal totalAbonosMensualidad = 0;
                        var tiposMovimientoAbono = await _Iservices.GetTipoMovimientoAsync();
                        var tipoAbonoMensualidad = tiposMovimientoAbono
                            .FirstOrDefault(tm => (tm.Concepto.ToLower().Contains("abono mensualidad") &&
                                                 tm.Concepto.ToLower().Contains("mensualidad")) ||
                                                tm.Concepto.ToLower().Contains("abono mensualidad"));
                        if (tipoAbonoMensualidad != null)
                        {
                            totalAbonosMensualidad = listpagos
                                .Where(p => p.IdAlumno == pagos.Pago.IdAlumno &&
                                            p.IdPeriodo == pagos.Pago.IdPeriodo &&
                                            p.IdTipoMovimiento == tipoAbonoMensualidad.IdTipoMovimiento &&
                                            p.Activo == true)
                                .Sum(p => p.Monto);
                        }

                        decimal totalEsperadoConAbonos = Math.Max(0, totalMensualidadesEsperado - totalAbonosMensualidad);

                        if (tieneMediaBeca && pagos.Pago.Monto > totalEsperadoConAbonos + 0.01m)
                        {
                            TempData["Mensaje"] = "El Estudiante Cuenta con Media Beca.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }

                        if (Math.Abs(pagos.Pago.Monto - totalEsperadoConAbonos) > 0.05m)
                        {
                            TempData["Mensaje"] = $"El monto ingresado (C$ {pagos.Pago.Monto:N2}) no coincide con {cantidadMesesPago} mes(es) × C$ {montoPorMes:N2} = C$ {totalMensualidadesEsperado:N2}.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }

                        //crear meses pagados previos (mensualidad tipo 1) — incluye pagos del ciclo y del ciclo de la matrícula
                        var mesesPagadosBD = listpagos
                        .Where(p => p.IdAlumno == pagos.Pago.IdAlumno &&
                                    p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento &&
                                    periodosConsulta.Contains(p.IdPeriodo) &&
                                    p.IdMes.HasValue)
                        .Select(p => p.IdMes!.Value)
                        .ToHashSet();
                        int primerMesPagadoMensual = mesesPagadosBD.Any() ? mesesPagadosBD.Min() : 1;
                        // Matrícula prematura: mes calendario de la primera matrícula (p. ej. abril) — la secuencia obligatoria
                        // empieza en ese mes, no desde enero aunque enero esté pagado con la matrícula.
                        int mesCalendarioMatricula = ObtenerMesCalendarioPrimeraMatricula(listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                        bool matriculaInicioCiclo = EsMatriculaInicioCicloLectivo(mesCalendarioMatricula);
                        bool esContinuidad = EsMatriculaContinuidad(listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                        int inicioSecuenciaMensual = CalcularInicioSecuenciaMensual(
                            matriculaInicioCiclo, esContinuidad, mesCalendarioMatricula, primerMesPagadoMensual);
                        //crear meses pagados virtualmente
                        var mesesPagadosAcumulados = new HashSet<int?>(mesesPagadosBD.Select(x => (int?)x));
                        var mesesVirtuales = mesesPagadosBD.ToHashSet();
                        IncluirEneroPagadoConMatricula(
                            mesesVirtuales, listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                        foreach (var m in mesesVirtuales)
                            mesesPagadosAcumulados.Add(m);

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

                            if (!matriculaInicioCiclo
                                && !esContinuidad
                                && idMes < mesCalendarioMatricula
                                && !mesesPagadosBD.Contains(idMes))
                            {
                                TempData["Mensaje"] =
                                    $"Con matrícula en {Mes(mesCalendarioMatricula).Result} (sin continuidad), no corresponde pagar por separado el mes de {Mes(idMes).Result}. Marque continuidad al registrar la matrícula si el alumno reingresa y debe meses anteriores.";
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
                                    Monto = montoPorMes,
                                    Descripcion=pagos.Pago.Descripcion,
                                    UsuarioRegistro=pagos.Pago.UsuarioRegistro,
                                    Activo=pagos.Pago.Activo,
                                    FechaRegistro=pagos.Pago.FechaRegistro,
                                    Serie=pagos.Pago.Serie
                                  
                                    // otros campos...
                                };

                                //await _Iservices.InsertarPagoAsync(nuevoPago);
                                (response, _, _) = await _Iservices.PostPagosAsync(nuevoPago);
                                
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
                        var idPag = ResolverIdPagoMaximoSeguro(pagosActualizados);
                        if (idPag <= 0)
                        {
                            TempData["Mensaje"] = "Pago registrado, pero no se pudo obtener el recibo. Consulte el listado de pagos.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Index", "Pagos");
                        }
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
                            bool esContinuidadMatricula = pagos.Pago.Continuidad;

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

                            // Cierra matrícula cuando la parte neta iguala lo pendiente o el monto cubre el paquete completo del catálogo
                            decimal montoMatriculaNetaCatalogo = Math.Max(0, obtenerMat - restarMensualidad);
                            decimal faltaMatriculaNeta = Math.Max(0, montoMatriculaNetaCatalogo - totalPagadoMatricula);
                            bool esMontoCompleto = Math.Abs(parteMatricula - faltaMatriculaNeta) < 0.01m
                                || Math.Abs(totalMatricula - faltaPorPagar) < 0.01m;
                            
                            // Si el monto completa el pago pendiente de matrícula, procesar como matrícula completa
                            if (esMontoCompleto)
                            {
                                // El monto ingresado completa lo que falta, restar la mensualidad
                                decimal valormatricula = totalMatricula - restarMensualidad;
                                pagos.Pago.Monto = valormatricula;
                                pagos.Pago.IdPeriodo = periodoMatricula;
                                pagos.Pago.Continuidad = esContinuidadMatricula;
                                (response, var idMatriculaCompleta, _) = await _Iservices.PostPagosAsync(pagos.Pago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el pago de matrícula.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                var pagosDespuesMatricula = await _Iservices.GetPagosAsync();
                                int idPagoMatricula = ResolverIdPagoTrasRegistro(idMatriculaCompleta, pagosDespuesMatricula, pagos.Pago);

                                var (eneroOk, errorEnero) = await AsegurarEneroRegistradoConMatricula(
                                    pagos.Pago, periodoMatricula, restarMensualidad);
                                if (!eneroOk)
                                {
                                    TempData["Mensaje"] = "Matrícula registrada, pero no se pudo registrar la mensualidad de enero. "
                                        + (errorEnero ?? "Revise el listado e intente registrar enero en mensualidad.")
                                        + " Si el problema persiste, contacte al administrador.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }

                                int idParaRecibo = idPagoMatricula > 0 ? idPagoMatricula : idMatriculaCompleta;
                                TempData["Mensaje"] = esContinuidadMatricula
                                    ? "Matrícula de continuidad registrada (matrícula + enero). Puede pagar los meses anteriores al mes de matrícula."
                                    : "Pago registrado correctamente (matrícula + mensualidad de enero).";
                                TempData["Tipo"] = "success";
                                return RedirectToAction("Details", "Pagos", new { id = idParaRecibo, imprimir = true });
                            }
                            else
                            {
                                // Si el monto es diferente (pago parcial), procesar como abono de matrícula
                                // Restar siempre la mensualidad del monto, similar al if anterior
                                decimal valormatricula = totalMatricula - restarMensualidad;
                                pagos.Pago.Monto = valormatricula;
                                pagos.Pago.IdPeriodo = periodoMatricula;
                                pagos.Pago.Continuidad = esContinuidadMatricula;
                                (response, var idMatriculaAbono, _) = await _Iservices.PostPagosAsync(pagos.Pago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el abono de matrícula.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                // Obtener el ID del pago de matrícula recién guardado para mostrar su recibo (no el del primer mes)
                                var pagosDespuesAbonoMat = await _Iservices.GetPagosAsync();
                                int idPagoMatriculaAbono = ResolverIdPagoTrasRegistro(idMatriculaAbono, pagosDespuesAbonoMat, pagos.Pago);
                                
                                var (eneroAbonoOk, errorEneroAbono) = await AsegurarEneroRegistradoConMatricula(
                                    pagos.Pago, periodoMatricula, restarMensualidad);
                                if (!eneroAbonoOk)
                                {
                                    TempData["Mensaje"] = "Abono de matrícula registrado, pero no se pudo registrar la mensualidad de enero. "
                                        + (errorEneroAbono ?? "Registre enero por separado en mensualidad.");
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }

                                int idParaReciboAbono = idPagoMatriculaAbono > 0 ? idPagoMatriculaAbono : idMatriculaAbono;
                                TempData["Mensaje"] = esContinuidadMatricula
                                    ? "Abono de matrícula de continuidad y pago de enero registrados. Puede pagar meses anteriores al mes de matrícula."
                                    : "El abono de matrícula y el pago de enero se registraron correctamente.";
                                TempData["Tipo"] = "success";
                                return RedirectToAction("Details", "Pagos", new { id = idParaReciboAbono, imprimir = true });
                            }
                           
                        }
                        else
                        {
                            bool esPagoMatricula = pagos.Pago.IdTipoMovimiento == TipoMovimientoMatricula
                                || pagos.Pago.IdTipoMovimiento == TipoMovimientoMatriculaAbono
                                || conceptoTipoMovimiento.Contains("matricula");

                            (response, var idPagoSimple, var errorPostSimple) = await _Iservices.PostPagosAsync(pagos.Pago);
                            if (response)
                            {
                                if (esPagoMatricula && pagos.Pago.IdGrado > 0)
                                {
                                    decimal montoEnero = await ObtenerMensualidadDecimal(
                                        pagos.Pago.IdRecinto, pagos.Pago.IdGrado,
                                        pagos.Pago.IdPeriodo, pagos.Pago.IdModalidad);
                                    if (montoEnero > 0)
                                    {
                                        var (eneroOk, errorEnero) = await AsegurarEneroRegistradoConMatricula(
                                            pagos.Pago, pagos.Pago.IdPeriodo, montoEnero);
                                        if (!eneroOk)
                                        {
                                            TempData["Mensaje"] = "Matrícula registrada, pero no se pudo registrar enero: "
                                                + (errorEnero ?? "revise el listado.");
                                            TempData["Tipo"] = "warning";
                                            return RedirectToAction("Create");
                                        }
                                    }
                                }

                                var idPag = idPagoSimple > 0 ? idPagoSimple : ResolverIdPagoTrasRegistro(0, buscarIdGuardado, pagos.Pago);
                                if (idPag <= 0)
                                    idPag = ResolverIdPagoMaximoSeguro(buscarIdGuardado) + 1;


                                TempData["Mensaje"] = "Pago registrado correctamente.";
                                TempData["Tipo"] = "success";
                                return RedirectToAction("Details", "Pagos", new { id = idPag + 1, imprimir = true });
                            }
                            else
                            {
                                var msg = "No se proceso el Pago.";
                                if (!string.IsNullOrWhiteSpace(errorPostSimple))
                                    msg += " Detalle: " + errorPostSimple;
                                TempData["Mensaje"] = msg;
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
            bool matriculaInicioCiclo = EsMatriculaInicioCicloLectivo(mesMatricula);
            bool esContinuidad = EsMatriculaContinuidad(listpagos, idAlumno, periodo);

            if (!matriculaInicioCiclo && !esContinuidad && mesMatricula > mes)
            {
                return Json(new { mora = 0, mes, aplicaMora = false });
            }

            int mesInicioMora = (matriculaInicioCiclo || esContinuidad) ? 1 : mesMatricula;
            int cantidadMesesEnVentana = mes - mesInicioMora;
            if (cantidadMesesEnVentana <= 0)
            {
                return Json(new { mora = 0, mes, aplicaMora = false });
            }

            var mesesPendientes = Enumerable.Range(mesInicioMora, cantidadMesesEnVentana)
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
        /// Tarifa de matrícula por recinto, período y modalidad; si no hay fila exacta, usa modalidad 0 (todas).
        /// </summary>
        private static TblCostoMatricula? ResolverFilaCostoMatricula(
            IList<TblCostoMatricula>? list,
            int? idRecinto,
            int idPeriodo,
            int? idModalidad)
        {
            if (list == null || list.Count == 0)
                return null;

            var candidatas = list
                .Where(x => x.IdRecinto == idRecinto && x.IdPeriodo == idPeriodo && x.Activo)
                .ToList();
            if (candidatas.Count == 0)
                return null;

            if (idModalidad.HasValue && idModalidad.Value > 0)
            {
                var exacta = candidatas.FirstOrDefault(x => x.IdModalidad == idModalidad.Value);
                if (exacta != null)
                    return exacta;
                var wildcard = candidatas.FirstOrDefault(x => x.IdModalidad == 0);
                if (wildcard != null)
                    return wildcard;
            }

            return candidatas.FirstOrDefault();
        }

        /// <summary>
        /// Matrícula en catálogo = paquete (neta + enero). Los pagos tipo 2/4 guardan la parte neta; enero va en mensualidad mes 1.
        /// </summary>
        private static (bool Cancelada, decimal SaldoPendiente) CalcularEstadoMatriculaEstadoCuenta(
            decimal montoCatalogo,
            decimal montoMensualidadEnero,
            decimal pagadoMatricula,
            decimal pagadoMensualidadEnero)
        {
            if (montoCatalogo <= 0m)
                return (true, 0m);

            decimal matriculaNetaEsperada = montoMensualidadEnero > 0m
                ? Math.Max(0m, montoCatalogo - montoMensualidadEnero)
                : montoCatalogo;

            bool cancelada =
                pagadoMatricula >= montoCatalogo - 0.01m
                || pagadoMatricula >= matriculaNetaEsperada - 0.01m
                || pagadoMatricula + pagadoMensualidadEnero >= montoCatalogo - 0.01m;

            if (cancelada)
                return (true, 0m);

            decimal saldo = Math.Max(0m, matriculaNetaEsperada - pagadoMatricula);
            return (false, saldo);
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
            var list = await _Iservices.GetCostosMatriculaAsync();
            var fila = ResolverFilaCostoMatricula(list, idRecinto, idPeriodo, idModalidad);
            return fila != null ? (decimal)fila.CostoMatricula : 0m;
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
            var list = _Iservices.GetCostosMatriculaAsync().Result;
            var fila = ResolverFilaCostoMatricula(list, idRecinto, idPeriodo, idModalidad);
            if (fila == null)
                return Json(new { costo = 0, sinConfiguracion = true });

            return Json(new { costo = fila.CostoMatricula, sinConfiguracion = false });
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
