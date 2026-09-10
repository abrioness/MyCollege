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
                var err = _Iservices.LastApiError;
                TempData["Mensaje"] = err.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase)
                    ? "La API rechazó la sesión (JWT). Cierre sesión e ingrese de nuevo. En el servidor, Jwt:SecretKey debe ser idéntica en Web y API."
                    : err.Contains("ServerError", StringComparison.OrdinalIgnoreCase) || err.Contains(" 500 ", StringComparison.OrdinalIgnoreCase) || err.Contains("HTTP 500", StringComparison.OrdinalIgnoreCase)
                    ? $"La API falló al leer los pagos. {err} Revise en Swagger GET /api/Pagos y los logs de la API (logs\\stdout)."
                    : err.Contains("DeserializeError", StringComparison.OrdinalIgnoreCase)
                    ? "La API respondió pero los datos de pagos no se pudieron leer. Contacte al administrador."
                    : $"No se pudieron cargar los pagos. {err}";
                TempData["Tipo"] = "warning";
            }
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _tipoRecibo = await _Iservices.GetTipoReciboAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var _meses = await _Iservices.GetMesesAsync();
            var _recinto = await _Iservices.GetRecintosAsync();
            var _usuarios = await _Iservices.GetUsuariosAsync() ?? new List<TblUsuarios>();
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
                recintos = _recinto,
                usuarios = _usuarios
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
        /// <summary>Abono de matrícula del ciclo en curso (Tbl_CatTipoMovimiento 18). El 4 es confirmación del siguiente ciclo.</summary>
        private const int TipoMovimientoMatriculaAbono = 18;
        private const decimal MontoMinimoReservaCupo = 250m;

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
        /// Reserva de cupo del siguiente ciclo (mínimo C$ 250). En catálogo es el tipo 4, no el abono 18.
        /// </summary>
        private static bool EsConfirmacionDeMatricula(int idTipoMovimiento, string? conceptoNormalizado)
        {
            if (idTipoMovimiento == TipoMovimientoMatriculaAbono)
                return false;

            var c = conceptoNormalizado ?? string.Empty;
            return c.Contains("confirmacion") || c.Contains("reserva");
        }

        /// <summary>
        /// Abono a la matrícula del ciclo vigente. Para alumnos activos que aún no la pagan.
        /// </summary>
        private static bool EsAbonoMatriculaCicloActual(int idTipoMovimiento, string? conceptoNormalizado)
        {
            if (EsConfirmacionDeMatricula(idTipoMovimiento, conceptoNormalizado))
                return false;
            if (idTipoMovimiento == TipoMovimientoMatriculaAbono)
                return true;

            var c = conceptoNormalizado ?? string.Empty;
            return c.Contains("abono") && c.Contains("matricula") && !c.Contains("completa");
        }

        private static bool EsMatriculaCompletaTipo(int idTipoMovimiento, string? conceptoNormalizado)
        {
            if (EsConfirmacionDeMatricula(idTipoMovimiento, conceptoNormalizado))
                return false;

            var c = conceptoNormalizado ?? string.Empty;
            return idTipoMovimiento == TipoMovimientoMatricula
                || (c.Contains("matricula") && c.Contains("completa"));
        }

        /// <summary>Mensualidad o abono de mensualidad (no matrícula ni rifa).</summary>
        private static bool EsPagoMensualidad(int idTipoMovimiento, string? conceptoNormalizado)
        {
            if (idTipoMovimiento == TipoMovimientoMensualidad)
                return true;

            var c = conceptoNormalizado ?? string.Empty;
            if (c.Contains("matricula") || c.Contains("rifa") || c.Contains("promoc"))
                return false;
            return c.Contains("mensualidad");
        }

        /// <summary>
        /// Mes calendario (1–12) de la primera matrícula del alumno en el período (fecha de emisión o registro).
        /// Con matrícula prematura (p. ej. en abril), no se exige pagar febrero/marzo antes de abril.
        /// </summary>
        private static int ObtenerMesCalendarioPrimeraMatricula(IEnumerable<TblPago> pagos, int idAlumno, int idPeriodo, int? idRecinto = null)
        {
            var p = ObtenerMatriculaReferencia(pagos, idAlumno, idPeriodo, idRecinto);
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

        /// <summary>
        /// Continuidad de este ciclo: alumno que sigue activo y paga matrícula durante el año
        /// (no es solo reingreso). No se toma la matrícula de otro período.
        /// </summary>
        private static bool EsMatriculaContinuidad(IEnumerable<TblPago> pagos, int idAlumno, int idPeriodo, int? idRecinto = null)
        {
            return pagos.Any(p => p.IdAlumno == idAlumno && p.Activo
                && p.IdTipoMovimiento == TipoMovimientoMatricula
                && p.Continuidad
                && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto)
                && (idPeriodo <= 0 || p.IdPeriodo == idPeriodo));
        }

        private static bool EsTipoEstudianteContinuidad(string? tipoEstudiante)
        {
            var t = NormalizarTexto(tipoEstudiante);
            return t.Contains("continuidad") || t.Contains("reintegro") || t.Contains("reingreso");
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
            IEnumerable<TblPago> pagos, int idAlumno, int idPeriodo, int? idRecinto = null)
        {
            // Solo matrícula completa (tipo 2) de ESTE ciclo. No usar la del año anterior:
            // un alumno de continuidad sigue activo y puede no haber pagado este período.
            var query = pagos.Where(p => p.IdAlumno == idAlumno && p.Activo
                && p.IdTipoMovimiento == TipoMovimientoMatricula
                && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto));

            if (idPeriodo > 0)
                query = query.Where(p => p.IdPeriodo == idPeriodo);

            return query.OrderBy(p => p.FechaRegistro).FirstOrDefault();
        }

        private static HashSet<int> ResolverPeriodosConsultaPago(
            IEnumerable<TblPago> pagos, int idAlumno, int idPeriodoFormulario)
        {
            var periodos = new HashSet<int>();
            if (idPeriodoFormulario > 0)
                periodos.Add(idPeriodoFormulario);

            // Solo el ciclo del formulario: la matrícula de otro año no suma meses pagados de este.
            var matricula = ObtenerMatriculaReferencia(pagos, idAlumno, idPeriodoFormulario);
            if (matricula != null && matricula.IdPeriodo == idPeriodoFormulario)
                periodos.Add(matricula.IdPeriodo);

            return periodos;
        }

        private static void AplicarDistribucionMatriculaEnero(
            Dictionary<int, decimal> pagadoPorMes,
            IEnumerable<TblPago> pagos,
            int idAlumno,
            int idPeriodo,
            decimal catalogoMatricula,
            decimal mensualidadEnero = 0m,
            int? idRecinto = null)
        {
            if (catalogoMatricula <= 0m || pagadoPorMes == null)
                return;

            decimal pagadoMat = pagos
                .Where(p => p.Activo
                    && p.IdAlumno == idAlumno
                    && p.IdPeriodo == idPeriodo
                    && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto)
                    && (p.IdTipoMovimiento == TipoMovimientoMatricula
                        || p.IdTipoMovimiento == TipoMovimientoMatriculaAbono))
                .Sum(p => p.Monto);
            decimal pagadoEne = pagadoPorMes.GetValueOrDefault(1);
            var (_, ene) = EstadoCuentaCalculoHelper.DistribuirPagoMatriculaYEnero(
                catalogoMatricula, pagadoMat, pagadoEne, mensualidadEnero);
            pagadoPorMes[1] = ene;
        }

        private static bool ExisteMensualidadEneroEnBd(
            IEnumerable<TblPago> pagos, int idAlumno, int idPeriodoFormulario, int? idRecinto = null)
        {
            if (idPeriodoFormulario <= 0)
                return false;

            return pagos.Any(p => p.Activo
                && p.IdAlumno == idAlumno
                && p.IdPeriodo == idPeriodoFormulario
                && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto)
                && p.IdTipoMovimiento == TipoMovimientoMensualidad
                && p.IdMes == 1);
        }

        private async Task<(bool Exito, string? Error)> AsegurarEneroRegistradoConMatricula(
            TblPago plantilla,
            int periodoMatricula,
            decimal montoMensualidadEnero,
            bool esAbonoParcial = false)
        {
            if (montoMensualidadEnero <= MensualidadSobranteHelper.Centavo)
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
                Descripcion = esAbonoParcial
                    ? "Abono a enero"
                    : (string.IsNullOrWhiteSpace(plantilla.Descripcion) ? "Mensualidad enero" : plantilla.Descripcion),
                UsuarioRegistro = plantilla.UsuarioRegistro,
                Activo = plantilla.Activo,
                FechaRegistro = plantilla.FechaRegistro,
                Serie = plantilla.Serie,
                Continuidad = false
            };

            var (ok, _, err) = await _Iservices.PostPagosAsync(pagoEnero);
            return (ok, err);
        }

        private static TblPago CrearLineaMensualidad(
            TblPago plantilla,
            int idMes,
            decimal monto,
            int idPeriodo,
            int? mora,
            string? descripcion)
        {
            return new TblPago
            {
                IdAlumno = plantilla.IdAlumno,
                NumeroRecibo = plantilla.NumeroRecibo,
                Anyo = plantilla.Anyo,
                IdMes = idMes,
                IdTipoRecibo = plantilla.IdTipoRecibo,
                IdTipoMovimiento = TipoMovimientoMensualidad,
                IdMetodoPago = plantilla.IdMetodoPago,
                IdGrado = plantilla.IdGrado,
                IdPeriodo = idPeriodo,
                IdRecinto = plantilla.IdRecinto,
                IdModalidad = plantilla.IdModalidad,
                FechaEmision = plantilla.FechaEmision,
                Mora = mora,
                Monto = monto,
                Descripcion = descripcion,
                UsuarioRegistro = plantilla.UsuarioRegistro,
                Activo = plantilla.Activo,
                FechaRegistro = plantilla.FechaRegistro,
                Serie = plantilla.Serie
            };
        }

        [Authorize]
        public async Task<ActionResult> EstadoCuenta()
        {
            var pagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            var alumnos = (await _Iservices.GetAlumnosAsync())?.Where(a => a.Activo != false).ToList()
                ?? new List<TblAlumno>();
            var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoRef = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos);
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
            var matriculasCiclo = (await _Iservices.GetMatriculasAsync() ?? new List<TblMatricula>())
                .Where(m => m.Activo)
                .ToList();
            var grados = await _Iservices.GetGradosAsync() ?? new List<Grados>();
            var recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            var mesesCatalog = await _Iservices.GetMesesAsync() ?? new List<TblCatMeses>();

            var culturaEs = new CultureInfo("es-NI");
            var nombresMes = Enumerable.Range(1, 12).Select(m =>
                mesesCatalog.FirstOrDefault(x => x.IdMes == m)?.Mes?.Trim()
                ?? culturaEs.DateTimeFormat.GetMonthName(m)).ToArray();

            int mesMensualidadRequerido = EstadoCuentaSolvenciaHelper.ObtenerMesMensualidadRequerido();
            string nombreMesRequerido = nombresMes[mesMensualidadRequerido - 1];

            var tiposMov = await _Iservices.GetTipoMovimientoAsync() ?? new List<CatTipoMovimiento>();
            var pagosCaja = await _Iservices.GetPagoCajaAsync() ?? new List<TblPagoCaja>();
            var filas = EstadoCuentaCalculoHelper.ConstruirFilas(
                alumnos,
                pagos,
                costosMen,
                costosMat,
                matriculasCiclo,
                grados,
                recintos,
                mesesCatalog,
                idPeriodoRef,
                anioPeriodo,
                periodos,
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "mensualidad"),
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "rifa", "rifas"),
                EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMov, "promoc"),
                pagosCaja,
                tiposMov);

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
            if (listpagos == null)
            {
                TempData["Mensaje"] = "No se encontró el recibo. Consulte el listado de pagos.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Index");
            }

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
                        var mesesCatalogMat = await _Iservices.GetMesesAsync();
                        decimal tarifaEneroRecibo = 0m;
                        if (listpagos.IdRecinto.HasValue && listpagos.IdGrado > 0)
                        {
                            tarifaEneroRecibo = await ObtenerMensualidadDecimal(
                                listpagos.IdRecinto,
                                listpagos.IdGrado,
                                listpagos.IdPeriodo,
                                listpagos.IdModalidad);
                            if (tarifaEneroRecibo > 0 && await MediaBeca(listpagos.IdAlumno, listpagos.IdPeriodo))
                                tarifaEneroRecibo *= 0.5m;
                        }

                        var lineasMat = new List<ReciboLineaDetalle>();
                        foreach (var lineaPago in pagosMismoRecibo
                            .OrderBy(p => p.IdTipoMovimiento == idTipoMatricula ? 0 : 1)
                            .ThenBy(p => p.IdMes))
                        {
                            if (lineaPago.IdTipoMovimiento == idTipoMatricula)
                            {
                                lineasMat.Add(new ReciboLineaDetalle
                                {
                                    Etiqueta = "Matrícula",
                                    Monto = lineaPago.Monto,
                                    EsAbono = false
                                });
                                continue;
                            }

                            string nombreMes = lineaPago.IdMes.HasValue
                                ? (mesesCatalogMat.FirstOrDefault(m => m.IdMes == lineaPago.IdMes)?.Mes ?? "Enero")
                                : "Enero";
                            bool esAbono = tarifaEneroRecibo > 0
                                && lineaPago.Monto < tarifaEneroRecibo - MensualidadSobranteHelper.Centavo;
                            lineasMat.Add(new ReciboLineaDetalle
                            {
                                Etiqueta = esAbono ? $"Abono {nombreMes}" : nombreMes,
                                Monto = lineaPago.Monto,
                                EsAbono = esAbono
                            });
                        }
                        viewModel.LineasRecibo = lineasMat;
                    }
                }
                else if (listpagos.IdTipoMovimiento == idTipoMensualidad)
                {
                    // Mensualidad: sumar todas las mensualidades del mismo recibo (varios meses en un solo recibo)
                    var pagosMismoRecibo = todosPagos
                        .Where(p => p.NumeroRecibo == listpagos.NumeroRecibo
                            && p.Serie == listpagos.Serie
                            && p.IdAlumno == listpagos.IdAlumno
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
                        decimal tarifaRecibo = 0m;
                        if (listpagos.IdRecinto.HasValue && listpagos.IdGrado > 0)
                        {
                            tarifaRecibo = await ObtenerMensualidadDecimal(
                                listpagos.IdRecinto,
                                listpagos.IdGrado,
                                listpagos.IdPeriodo,
                                listpagos.IdModalidad);
                            if (tarifaRecibo > 0 && await MediaBeca(listpagos.IdAlumno, listpagos.IdPeriodo))
                                tarifaRecibo *= 0.5m;
                        }

                        var lineas = new List<ReciboLineaDetalle>();
                        var etiquetasMeses = new List<string>();
                        foreach (var lineaPago in pagosMismoRecibo.OrderBy(p => p.IdPeriodo).ThenBy(p => p.IdMes))
                        {
                            string nombreMes = lineaPago.IdMes.HasValue
                                ? (mesesCatalog.FirstOrDefault(m => m.IdMes == lineaPago.IdMes)?.Mes ?? lineaPago.IdMes.Value.ToString())
                                : "Mensualidad";
                            bool esAbono = (lineaPago.Descripcion?.IndexOf("Abono a", StringComparison.OrdinalIgnoreCase) >= 0)
                                || (tarifaRecibo > 0 && lineaPago.Monto < tarifaRecibo - MensualidadSobranteHelper.Centavo);
                            string etiqueta = esAbono ? $"Abono {nombreMes}" : nombreMes;
                            lineas.Add(new ReciboLineaDetalle
                            {
                                Etiqueta = etiqueta,
                                Monto = lineaPago.Monto,
                                EsAbono = esAbono
                            });
                            etiquetasMeses.Add(etiqueta);
                        }
                        viewModel.LineasRecibo = lineas;
                        viewModel.DetalleMesesRecibo = etiquetasMeses.Count > 0
                            ? string.Join(", ", etiquetasMeses)
                            : (pagosMismoRecibo.Count > 1 ? $"{pagosMismoRecibo.Count} mensualidades" : null);
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
        public async Task<ActionResult> Create(int? idAlumno, int? idRecinto, int? idGrado, int? idModalidad, int? idPeriodo, string? meses, bool traslado = false, bool cobrarMatricula = false, int? mesIngreso = null)
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

                periodo = (await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>())
                .Where(a => a.Activo)
                .OrderByDescending(a => a.Periodo)
                .Select(r => new SelectListItem
                {
                    Value = r.IdPeriodo.ToString(),
                    Text = r.Actual ? $"{r.Periodo} (actual)" : r.Periodo.ToString(),
                    Selected = r.Actual
                }).ToList(),
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

            if (idAlumno.HasValue && idAlumno.Value > 0)
            {
                var alumnoPre = await _Iservices.GetAlumnoIdAsync(idAlumno.Value);
                if (alumnoPre != null && alumnoPre.IdAlumno > 0)
                {
                    viewmodel.PreloadIdAlumno = alumnoPre.IdAlumno;
                    viewmodel.PreloadNombreAlumno = $"{alumnoPre.Nombre} {alumnoPre.Apellido}".Trim();
                    viewmodel.PreloadIdRecinto = idRecinto ?? alumnoPre.IdRecinto;
                    viewmodel.PreloadIdGrado = idGrado ?? alumnoPre.IdGrado;
                    viewmodel.PreloadIdModalidad = idModalidad ?? alumnoPre.IdModalidad;
                    viewmodel.PreloadIdPeriodo = idPeriodo ?? alumnoPre.IdPeriodo;
                    viewmodel.PreloadMeses = meses;
                    viewmodel.EsCobroTraslado = traslado;
                    viewmodel.CobraMatriculaTraslado = cobrarMatricula;
                    viewmodel.PreloadMesIngreso = mesIngreso;
                    viewmodel.Pago.IdAlumno = alumnoPre.IdAlumno;
                    if (!cobrarMatricula)
                        viewmodel.Pago.IdTipoMovimiento = TipoMovimientoMensualidad;
                    if (viewmodel.PreloadIdRecinto.HasValue)
                        viewmodel.Pago.IdRecinto = viewmodel.PreloadIdRecinto.Value;
                    if (viewmodel.PreloadIdGrado.HasValue)
                        viewmodel.Pago.IdGrado = viewmodel.PreloadIdGrado.Value;
                    if (viewmodel.PreloadIdModalidad.HasValue)
                        viewmodel.Pago.IdModalidad = viewmodel.PreloadIdModalidad;
                    if (viewmodel.PreloadIdPeriodo.HasValue)
                        viewmodel.Pago.IdPeriodo = viewmodel.PreloadIdPeriodo.Value;
                }
            }

            return View(viewmodel);
        }

        [Authorize]
        public async Task<ActionResult> Traslado(int? idAlumno)
        {
            var vm = new TrasladoViewModel();
            await CargarCatalogosTraslado(vm);
            if (idAlumno.HasValue && idAlumno.Value > 0)
            {
                var armado = await ArmarTrasladoAsync(idAlumno.Value, null, vm);
                if (armado == null)
                {
                    TempData["Mensaje"] = "No se encontró el alumno o no tiene colegio asignado.";
                    TempData["Tipo"] = "warning";
                    return View(vm);
                }
                vm = armado;
            }
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Traslado(TrasladoViewModel form)
        {
            if (form == null || form.IdAlumno <= 0)
            {
                TempData["Mensaje"] = "Busque y seleccione el alumno que se traslada.";
                TempData["Tipo"] = "warning";
                var vacio = new TrasladoViewModel();
                await CargarCatalogosTraslado(vacio);
                return View(vacio);
            }

            var vm = await ArmarTrasladoAsync(form.IdAlumno, form.IdRecintoDestino, form);
            if (vm == null)
            {
                TempData["Mensaje"] = "No se pudo armar el traslado. Verifique alumno y colegio destino.";
                TempData["Tipo"] = "warning";
                await CargarCatalogosTraslado(form);
                return View(form);
            }

            if (vm.IdRecintoDestino <= 0 || vm.IdRecintoDestino == vm.IdRecintoOrigen)
            {
                TempData["Mensaje"] = "Seleccione un colegio destino distinto al actual.";
                TempData["Tipo"] = "warning";
                return View(vm);
            }

            var recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            string nombreOrigen = recintos.FirstOrDefault(r => r.IdRecinto == vm.IdRecintoOrigen)?.Recinto
                ?? vm.NombreRecintoOrigen;
            var alumno = await _Iservices.GetAlumnoIdAsync(vm.IdAlumno);
            if (alumno == null || alumno.IdAlumno <= 0)
            {
                TempData["Mensaje"] = "Alumno no encontrado.";
                TempData["Tipo"] = "warning";
                return View(vm);
            }

            int idPeriodo = vm.IdPeriodo > 0 ? vm.IdPeriodo : (alumno.IdPeriodo ?? 0);
            var matricula = await BuscarMatriculaCicloTrasladoAsync(vm.IdAlumno, idPeriodo);
            if (matricula != null && matricula.IdPeriodo > 0)
                idPeriodo = matricula.IdPeriodo;
            if (idPeriodo <= 0)
            {
                var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
                idPeriodo = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos)?.IdPeriodo ?? 0;
            }

            if (vm.FechaTraslado is not { Year: > 2000 } ft
                || vm.MesIngreso is < 1 or > 12)
            {
                TempData["Mensaje"] = "Indique la fecha de traslado. Ese mes es el primero a cobrar en el destino; no se usa la fecha de hoy por omisión.";
                TempData["Tipo"] = "warning";
                return View(vm);
            }

            DateTime fechaTraslado = ft.Date;
            if (fechaTraslado.Month != vm.MesIngreso)
            {
                int anio = vm.AnioPeriodo > 0 ? vm.AnioPeriodo : fechaTraslado.Year;
                int dia = Math.Min(fechaTraslado.Day, DateTime.DaysInMonth(anio, vm.MesIngreso));
                fechaTraslado = new DateTime(anio, vm.MesIngreso, dia);
            }

            string nota = TrasladoHelper.Anotar(
                matricula?.Observaciones ?? alumno.Observaciones,
                vm.IdRecintoOrigen,
                nombreOrigen,
                fechaTraslado,
                vm.MesIngreso);

            if (matricula != null && matricula.IdMatricula > 0)
            {
                matricula.IdRecinto = vm.IdRecintoDestino;
                if (vm.IdGrado > 0)
                    matricula.IdGrado = vm.IdGrado;
                if (form.IdModalidad.HasValue && form.IdModalidad.Value > 0)
                    matricula.IdModalidad = form.IdModalidad.Value;
                matricula.Estado = TblMatricula.EstadoActivo;
                matricula.Activo = true;
                matricula.Continuidad = true;
                matricula.TipoEstudiante = TrasladoHelper.TipoEstudianteTraslado;
                matricula.Observaciones = nota;
                matricula.UsuarioActualizo = IdUsuarioPago();
                matricula.FechaActualizo = DateTime.Now;
                var (okMat, errMat) = await _Iservices.UpdateMatriculaAsync(matricula);
                if (!okMat)
                {
                    TempData["Mensaje"] = "No se pudo actualizar la matrícula del ciclo. " + (errMat ?? _Iservices.LastApiError);
                    TempData["Tipo"] = "warning";
                    return View(vm);
                }
            }
            else
            {
                var nueva = new TblMatricula
                {
                    IdAlumno = vm.IdAlumno,
                    IdPeriodo = idPeriodo,
                    IdGrado = vm.IdGrado,
                    IdModalidad = form.IdModalidad ?? vm.IdModalidad ?? 0,
                    IdRecinto = vm.IdRecintoDestino,
                    IdTurno = alumno.IdTurno,
                    IdGrupo = alumno.IdGrupo,
                    Estado = TblMatricula.EstadoActivo,
                    Continuidad = true,
                    TipoEstudiante = TrasladoHelper.TipoEstudianteTraslado,
                    Observaciones = nota,
                    FechaMatricula = DateTime.Today,
                    Activo = true,
                    UsuarioRegistro = IdUsuarioPago(),
                    FechaRegistro = DateTime.Now,
                    BecaCompleta = alumno.BecaCompleta == true,
                    MediaBeca = alumno.MediaBeca == true,
                    Repitente = alumno.Repitente
                };
                var (okNueva, errNueva) = await _Iservices.PostMatriculaAsync(nueva);
                if (!okNueva && EsConflictoMatriculaExistente(errNueva ?? _Iservices.LastApiError))
                {
                    matricula = await BuscarMatriculaCicloTrasladoAsync(vm.IdAlumno, idPeriodo);
                    if (matricula != null && matricula.IdMatricula > 0)
                    {
                        matricula.IdRecinto = vm.IdRecintoDestino;
                        if (vm.IdGrado > 0)
                            matricula.IdGrado = vm.IdGrado;
                        if (form.IdModalidad.HasValue && form.IdModalidad.Value > 0)
                            matricula.IdModalidad = form.IdModalidad.Value;
                        matricula.Estado = TblMatricula.EstadoActivo;
                        matricula.Activo = true;
                        matricula.Continuidad = true;
                        matricula.TipoEstudiante = TrasladoHelper.TipoEstudianteTraslado;
                        matricula.Observaciones = nota;
                        matricula.UsuarioActualizo = IdUsuarioPago();
                        matricula.FechaActualizo = DateTime.Now;
                        var (okMat, errMat) = await _Iservices.UpdateMatriculaAsync(matricula);
                        if (!okMat)
                        {
                            TempData["Mensaje"] = "No se pudo actualizar la matrícula del ciclo. " + (errMat ?? _Iservices.LastApiError);
                            TempData["Tipo"] = "warning";
                            return View(vm);
                        }
                        okNueva = true;
                    }
                }
                if (!okNueva)
                {
                    TempData["Mensaje"] = "No se pudo registrar la colocación en el colegio destino. " + (errNueva ?? _Iservices.LastApiError);
                    TempData["Tipo"] = "warning";
                    return View(vm);
                }
            }

            alumno.IdRecinto = vm.IdRecintoDestino;
            if (vm.IdGrado > 0)
                alumno.IdGrado = vm.IdGrado;
            if (form.IdModalidad.HasValue && form.IdModalidad.Value > 0)
                alumno.IdModalidad = form.IdModalidad;
            alumno.IdPeriodo = idPeriodo;
            alumno.TipoEstudiante = TrasladoHelper.TipoEstudianteTraslado;
            alumno.Observaciones = TrasladoHelper.Anotar(alumno.Observaciones, vm.IdRecintoOrigen, nombreOrigen, fechaTraslado, vm.MesIngreso);
            alumno.UsuarioActualiza = IdUsuarioPago();
            alumno.FechaActualiza = DateTime.Now;
            await _Iservices.UpdateAlumnos(alumno);

            var mesesCobro = vm.Meses.Where(m => m.SaldoACobrar > MensualidadSobranteHelper.Centavo).Select(m => m.IdMes).ToList();
            string mesNombre = vm.Meses.FirstOrDefault(m => m.IdMes == vm.MesIngreso)?.Nombre ?? vm.MesIngreso.ToString();
            if (vm.CobraMatricula || mesesCobro.Count > 0)
            {
                TempData["Mensaje"] = vm.CobraMatricula
                    ? $"Traslado aplicado. Cobre la matrícula del destino y las mensualidades desde {mesNombre}."
                    : $"Traslado aplicado. Cobre las mensualidades desde {mesNombre}.";
                TempData["Tipo"] = "success";
                return RedirectToAction(nameof(Create), new
                {
                    idAlumno = vm.IdAlumno,
                    idRecinto = vm.IdRecintoDestino,
                    idGrado = vm.IdGrado,
                    idModalidad = form.IdModalidad ?? vm.IdModalidad,
                    idPeriodo,
                    meses = string.Join(",", mesesCobro),
                    traslado = true,
                    cobrarMatricula = vm.CobraMatricula,
                    mesIngreso = vm.MesIngreso
                });
            }

            TempData["Mensaje"] = "Traslado aplicado. No hay matrícula ni mensualidades pendientes por cobrar.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(EstadoCuenta));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerResumenTraslado(
            int idAlumno,
            int idRecintoDestino,
            int mesIngreso = 0,
            DateTime? fechaTraslado = null)
        {
            var baseVm = new TrasladoViewModel
            {
                MesIngreso = mesIngreso,
                FechaTraslado = fechaTraslado
            };
            var vm = await ArmarTrasladoAsync(idAlumno, idRecintoDestino, baseVm);
            if (vm == null)
                return Json(new { ok = false });

            return Json(new
            {
                ok = true,
                nombreAlumno = vm.NombreAlumno,
                idPeriodo = vm.IdPeriodo,
                anioPeriodo = vm.AnioPeriodo,
                idRecintoOrigen = vm.IdRecintoOrigen,
                nombreRecintoOrigen = vm.NombreRecintoOrigen,
                tarifaOrigen = vm.TarifaOrigen,
                tarifaDestino = vm.TarifaDestino,
                totalACobrar = vm.TotalACobrar,
                mesesACobrarCsv = vm.MesesACobrarCsv,
                idGrado = vm.IdGrado,
                idModalidad = vm.IdModalidad,
                cobraMatricula = vm.CobraMatricula,
                montoMatricula = vm.MontoMatricula,
                matriculaNeta = vm.MatriculaNeta,
                saldoMatricula = vm.SaldoMatricula,
                mesIngreso = vm.MesIngreso,
                fechaTraslado = vm.FechaTraslado?.ToString("yyyy-MM-dd"),
                recintosDestino = vm.RecintosDestino.Select(r => new { value = r.Value, text = r.Text }),
                meses = vm.Meses.Select(m => new
                {
                    m.IdMes,
                    m.Nombre,
                    tarifa = vm.TarifaDestino,
                    m.Respetado,
                    m.NoCorresponde,
                    m.SaldoACobrar
                })
            });
        }

        private static bool EsConflictoMatriculaExistente(string? error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return false;
            var t = error.ToLowerInvariant();
            return t.Contains("ya tiene matr") || t.Contains("ya tiene matricula") || t.Contains("conflicto") || t.Contains("409");
        }

        private async Task<TblMatricula?> BuscarMatriculaCicloTrasladoAsync(int idAlumno, int idPeriodoPreferido)
        {
            if (idAlumno <= 0)
                return null;

            if (idPeriodoPreferido > 0)
            {
                var directa = await _Iservices.GetMatriculaAlumnoPeriodoAsync(idAlumno, idPeriodoPreferido);
                if (directa != null && directa.IdMatricula > 0)
                    return directa;
            }

            var lista = (await _Iservices.GetMatriculasAsync(idAlumno: idAlumno) ?? new List<TblMatricula>())
                .Where(m => m.IdAlumno == idAlumno)
                .OrderByDescending(m => m.IdMatricula)
                .ToList();
            if (lista.Count == 0)
                return null;

            static bool EsCurso(TblMatricula m)
                => m.Activo
                   && !string.Equals(m.Estado, TblMatricula.EstadoRetirado, StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(m.Estado, TblMatricula.EstadoReserva, StringComparison.OrdinalIgnoreCase);

            if (idPeriodoPreferido > 0)
            {
                return lista.FirstOrDefault(m => m.IdPeriodo == idPeriodoPreferido && EsCurso(m))
                    ?? lista.FirstOrDefault(m => m.IdPeriodo == idPeriodoPreferido)
                    ?? lista.FirstOrDefault(EsCurso)
                    ?? lista.FirstOrDefault();
            }

            return lista.FirstOrDefault(EsCurso) ?? lista.FirstOrDefault();
        }

        private async Task CargarCatalogosTraslado(TrasladoViewModel vm)
        {
            var recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            vm.RecintosDestino = recintos
                .Where(r => (r.Activo || r.IdRecinto == vm.IdRecintoDestino)
                    && (vm.IdRecintoOrigen <= 0 || r.IdRecinto != vm.IdRecintoOrigen))
                .Select(r => new SelectListItem
                {
                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto,
                    Selected = r.IdRecinto == vm.IdRecintoDestino
                }).ToList();
            vm.Grados = (await _Iservices.GetGradosAsync() ?? new List<Grados>())
                .Select(g => new SelectListItem
                {
                    Value = g.IdGrado.ToString(),
                    Text = g.NombreGrado,
                    Selected = g.IdGrado == vm.IdGrado
                }).ToList();
            vm.Modalidades = (await _Iservices.GetModalidadesAsync() ?? new List<Modalidades>())
                .Select(m => new SelectListItem
                {
                    Value = m.IdModalidad.ToString(),
                    Text = m.Modalidad,
                    Selected = vm.IdModalidad.HasValue && m.IdModalidad == vm.IdModalidad.Value
                }).ToList();
        }

        private async Task<TrasladoViewModel?> ArmarTrasladoAsync(int idAlumno, int? idRecintoDestino, TrasladoViewModel? baseVm)
        {
            var alumno = await _Iservices.GetAlumnoIdAsync(idAlumno);
            if (alumno == null || alumno.IdAlumno <= 0 || !alumno.IdRecinto.HasValue)
                return null;

            var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoMensual = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos);
            var matriculaCiclo = await BuscarMatriculaCicloTrasladoAsync(
                idAlumno, alumno.IdPeriodo ?? periodoMensual?.IdPeriodo ?? 0);
            int idPeriodo = matriculaCiclo is { IdPeriodo: > 0 }
                ? matriculaCiclo.IdPeriodo
                : (alumno.IdPeriodo ?? periodoMensual?.IdPeriodo ?? 0);
            var recintos = await _Iservices.GetRecintosAsync() ?? new List<Recintos>();
            var mesesCat = await _Iservices.GetMesesAsync() ?? new List<TblCatMeses>();

            var vm = baseVm ?? new TrasladoViewModel();
            vm.IdAlumno = alumno.IdAlumno;
            vm.NombreAlumno = $"{alumno.Nombre} {alumno.Apellido}".Trim();
            vm.IdPeriodo = idPeriodo;
            vm.AnioPeriodo = periodos.FirstOrDefault(p => p.IdPeriodo == idPeriodo)?.Periodo
                ?? periodoMensual?.Periodo
                ?? DateTime.Today.Year;
            int idRecintoOrigen = matriculaCiclo is { IdRecinto: > 0 }
                ? matriculaCiclo.IdRecinto
                : alumno.IdRecinto.Value;
            vm.IdRecintoOrigen = idRecintoOrigen;
            vm.NombreRecintoOrigen = recintos.FirstOrDefault(r => r.IdRecinto == vm.IdRecintoOrigen)?.Recinto ?? "";
            vm.IdGrado = vm.IdGrado > 0
                ? vm.IdGrado
                : (matriculaCiclo is { IdGrado: > 0 } ? matriculaCiclo.IdGrado : alumno.IdGrado ?? 0);
            vm.IdModalidad = vm.IdModalidad is > 0
                ? vm.IdModalidad
                : (matriculaCiclo is { IdModalidad: > 0 } ? matriculaCiclo.IdModalidad : alumno.IdModalidad);
            vm.IdRecintoDestino = idRecintoDestino ?? vm.IdRecintoDestino;
            if (vm.IdRecintoDestino == vm.IdRecintoOrigen)
                vm.IdRecintoDestino = 0;
            await CargarCatalogosTraslado(vm);

            int idDestino = vm.IdRecintoDestino > 0 && vm.IdRecintoDestino != vm.IdRecintoOrigen
                ? vm.IdRecintoDestino
                : 0;
            vm.TarifaOrigen = vm.IdGrado > 0
                ? await ObtenerMensualidadDecimal(vm.IdRecintoOrigen, vm.IdGrado, idPeriodo, vm.IdModalidad)
                : 0m;
            vm.TarifaDestino = idDestino > 0 && vm.IdGrado > 0
                ? await ObtenerMensualidadDecimal(idDestino, vm.IdGrado, idPeriodo, vm.IdModalidad)
                : 0m;
            if (alumno.MediaBeca == true)
            {
                vm.TarifaOrigen *= 0.5m;
                vm.TarifaDestino *= 0.5m;
            }
            if (alumno.BecaCompleta == true)
            {
                vm.TarifaOrigen = 0m;
                vm.TarifaDestino = 0m;
            }

            if (vm.FechaTraslado is { Year: > 2000 } fechaSel)
            {
                vm.FechaTraslado = fechaSel.Date;
                vm.MesIngreso = fechaSel.Month;
            }
            else if (vm.MesIngreso is >= 1 and <= 12)
            {
                int anioFecha = vm.AnioPeriodo > 0 ? vm.AnioPeriodo : DateTime.Today.Year;
                vm.FechaTraslado = new DateTime(anioFecha, vm.MesIngreso, 1);
            }
            else
            {
                vm.FechaTraslado = null;
                vm.MesIngreso = 0;
            }

            var todosPagosCiclo = (await _Iservices.GetPagosAsync() ?? new List<TblPago>())
                .Where(p => p.IdAlumno == idAlumno
                    && p.IdPeriodo == idPeriodo
                    && p.Activo)
                .ToList();
            // Mientras el alumno sigue en el origen, el destino no tiene pagos de este traslado.
            bool yaEstaEnDestino = idDestino > 0 && matriculaCiclo is { IdRecinto: > 0 }
                && matriculaCiclo.IdRecinto == idDestino;
            var pagosDestino = yaEstaEnDestino
                ? todosPagosCiclo.Where(p =>
                    p.IdRecinto == idDestino
                    && (!vm.FechaTraslado.HasValue
                        || (p.FechaEmision ?? p.FechaRegistro).Date >= vm.FechaTraslado.Value.Date))
                    .ToList()
                : new List<TblPago>();
            var pagosMesDestino = pagosDestino
                .Where(p => p.IdTipoMovimiento == TipoMovimientoMensualidad && p.IdMes.HasValue)
                .ToList();

            vm.MontoMatricula = idDestino > 0
                ? await ObtenerMatriculaDecimal(idDestino, vm.IdModalidad, idPeriodo)
                : 0m;
            vm.MatriculaNeta = EstadoCuentaCalculoHelper.MatriculaNetaDesdePaquete(
                vm.MontoMatricula, vm.TarifaDestino);

            if (idDestino <= 0 || vm.MesIngreso is < 1 or > 12)
            {
                vm.Meses = new List<TrasladoMesFila>();
                vm.CobraMatricula = false;
                vm.SaldoMatricula = 0m;
                vm.TotalACobrar = 0m;
                vm.MesesACobrarCsv = "";
                return vm;
            }

            var tipos = await _Iservices.GetTipoMovimientoAsync() ?? new List<CatTipoMovimiento>();
            var tiposMat = EstadoCuentaCalculoHelper.TiposPagoMatricula(tipos);
            decimal pagadoMatDestino = pagosDestino
                .Where(p => tiposMat.Contains(p.IdTipoMovimiento))
                .Sum(p => p.Monto);
            decimal pagadoEneroDestino = pagosMesDestino.Where(p => p.IdMes == 1).Sum(p => p.Monto);
            var estadoMat = CalcularEstadoMatriculaEstadoCuenta(
                vm.MontoMatricula, vm.TarifaDestino, pagadoMatDestino, pagadoEneroDestino);
            vm.SaldoMatricula = estadoMat.Cancelada ? 0m : estadoMat.SaldoPendiente;
            vm.CobraMatricula = idDestino > 0
                && vm.SaldoMatricula > MensualidadSobranteHelper.Centavo;

            vm.Meses = new List<TrasladoMesFila>();
            for (int m = 1; m <= 12; m++)
            {
                decimal pagadoDestino = m == 1
                    ? pagadoEneroDestino
                    : pagosMesDestino.Where(p => p.IdMes == m).Sum(p => p.Monto);
                bool noCorresponde = m < vm.MesIngreso;
                bool pagadoEnDestino = !noCorresponde
                    && vm.TarifaDestino > 0m
                    && MensualidadSobranteHelper.MesCancelado(vm.TarifaDestino, pagadoDestino);
                decimal saldo = noCorresponde || pagadoEnDestino || vm.TarifaDestino <= 0m
                    ? 0m
                    : Math.Max(0m, vm.TarifaDestino - pagadoDestino);
                string nombre = mesesCat.FirstOrDefault(x => x.IdMes == m)?.Mes
                    ?? CultureInfo.GetCultureInfo("es-NI").DateTimeFormat.GetMonthName(m);
                vm.Meses.Add(new TrasladoMesFila
                {
                    IdMes = m,
                    Nombre = nombre,
                    Pagado = pagadoDestino,
                    Respetado = pagadoEnDestino,
                    NoCorresponde = noCorresponde,
                    SaldoACobrar = saldo
                });
            }

            vm.TotalACobrar = vm.SaldoMatricula + vm.Meses.Sum(x => x.SaldoACobrar);
            vm.MesesACobrarCsv = string.Join(",", vm.Meses.Where(x => x.SaldoACobrar > 0.01m).Select(x => x.IdMes));
            return vm;
        }

        private int IdUsuarioPago()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : 0;
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
                bool esConfirmacionMatricula = EsConfirmacionDeMatricula(
                    pagos.Pago.IdTipoMovimiento, conceptoTipoMovimiento);
                bool esAbonoMatricula = EsAbonoMatriculaCicloActual(
                    pagos.Pago.IdTipoMovimiento, conceptoTipoMovimiento);
                bool esMatriculaCompleta = EsMatriculaCompletaTipo(
                    pagos.Pago.IdTipoMovimiento, conceptoTipoMovimiento);
                
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
                    if (pagos.Pago.IdTipoMovimiento == EstadoCuentaCalculoHelper.TipoRifa
                        || conceptoTipoMovimiento.Contains("rifa"))
                    {
                        var alumnoRifa = await _Iservices.GetAlumnoIdAsync(pagos.Pago.IdAlumno);
                        var matriculaRifa = pagos.Pago.IdPeriodo > 0
                            ? await _Iservices.GetMatriculaAlumnoPeriodoAsync(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo)
                            : null;
                        var cajaRifa = await _Iservices.GetPagoCajaAsync() ?? new List<TblPagoCaja>();
                        var idsRifa = EstadoCuentaCalculoHelper.IdsPorConcepto(tiposMovimientoCatalogo, "rifa", "rifas");
                        idsRifa.Add(EstadoCuentaCalculoHelper.TipoRifa);
                        var tiposMatRifa = EstadoCuentaCalculoHelper.TiposPagoMatricula(tiposMovimientoCatalogo);
                        var periodosRifa = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
                        int anioRifa = periodosRifa.FirstOrDefault(p => p.IdPeriodo == pagos.Pago.IdPeriodo)?.Periodo
                            ?? DateTime.Now.Year;
                        DateTime? fechaIngresoRifa = TrasladoHelper.LeerFechaTraslado(matriculaRifa?.Observaciones)
                            ?? TrasladoHelper.LeerFechaTraslado(alumnoRifa?.Observaciones);
                        var evalRifa = EstadoCuentaCalculoHelper.EvaluarRifaAlumno(
                            alumnoRifa,
                            matriculaRifa,
                            buscarIdGuardado,
                            cajaRifa,
                            idsRifa,
                            tiposMatRifa,
                            tiposMovimientoCatalogo,
                            pagos.Pago.IdPeriodo,
                            anioRifa,
                            pagos.Pago.IdRecinto,
                            fechaIngresoOverride: fechaIngresoRifa);
                        int siguienteRifa = evalRifa.SiguienteSemestreACobrar();
                        if (siguienteRifa == 0)
                        {
                            TempData["Mensaje"] = evalRifa.MensajeSinCobro();
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        pagos.Pago.IdMes = siguienteRifa == 1 ? 1 : 7;
                        pagos.Pago.Descripcion = EstadoCuentaCalculoHelper.AdjuntarMarcaRifa(
                            pagos.Pago.Descripcion, siguienteRifa);
                        (response, var idPagoRifa, _) = await _Iservices.PostPagosAsync(pagos.Pago);
                        if (response)
                        {
                            var pagosTrasRifa = await _Iservices.GetPagosAsync();
                            var idPag = idPagoRifa > 0
                                ? idPagoRifa
                                : ResolverIdPagoTrasRegistro(0, pagosTrasRifa, pagos.Pago);
                            if (idPag <= 0)
                                idPag = ResolverIdPagoMaximoSeguro(pagosTrasRifa);

                            var etiqueta = siguienteRifa == 1 ? "1.er semestre" : "2.º semestre";
                            var extra = siguienteRifa == 2 && !evalRifa.AplicaRifa1
                                ? " La rifa 1 no aplica: se matriculó después de que se generó."
                                : "";
                            TempData["Mensaje"] = $"Pago de rifa del {etiqueta} registrado.{extra}";
                            TempData["Tipo"] = "success";
                            if (idPag <= 0)
                                return RedirectToAction("Index", "Pagos");
                            return RedirectToAction("Details", "Pagos", new { id = idPag, imprimir = true });
                        }
                        else
                        {
                            TempData["Mensaje"] = "No se proceso el Pago.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                    }

                    
                    if (EsPagoMensualidad(pagos.Pago.IdTipoMovimiento, conceptoTipoMovimiento))
                    {
                        if (string.IsNullOrWhiteSpace(MesesSeleccionados))
                        {
                            TempData["Mensaje"] = "Debe seleccionar el mes o los meses pendientes para el pago o abono de mensualidad.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }

                        var ids = MesesSeleccionados.Split(',')
                            .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                            .Where(n => n is >= 1 and <= 12)
                            .Distinct()
                            .OrderBy(n => n)
                            .ToList();
                        if (ids.Count == 0)
                        {
                            TempData["Mensaje"] = "Debe seleccionar el mes o los meses pendientes para el pago o abono de mensualidad.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        var listpagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
                        var periodosCatMens = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
                        pagos.Pago.IdPeriodo = ResolverIdPeriodoMensualidad(periodosCatMens, pagos.Pago.IdPeriodo);
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

                        // Si la matrícula se guardó por encima del catálogo y no hay línea de enero, el sobrante es abono de enero.
                        var matriculaExistente = ObtenerMatriculaReferencia(
                            listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo, pagos.Pago.IdRecinto);
                        if (matriculaExistente != null
                            && !ExisteMensualidadEneroEnBd(listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo, pagos.Pago.IdRecinto))
                        {
                            decimal catalogoMatReparar = await ObtenerMatriculaDecimal(
                                pagos.Pago.IdRecinto, pagos.Pago.IdModalidad, pagos.Pago.IdPeriodo);
                            decimal pagadoMatReparar = listpagos
                                .Where(p => p.Activo
                                    && p.IdAlumno == pagos.Pago.IdAlumno
                                    && p.IdPeriodo == pagos.Pago.IdPeriodo
                                    && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, pagos.Pago.IdRecinto)
                                    && (p.IdTipoMovimiento == TipoMovimientoMatricula
                                        || p.IdTipoMovimiento == TipoMovimientoMatriculaAbono))
                                .Sum(p => p.Monto);
                            var (_, eneroDistribuido) = EstadoCuentaCalculoHelper.DistribuirPagoMatriculaYEnero(
                                catalogoMatReparar, pagadoMatReparar, 0m, tarifaMensual);
                            if (eneroDistribuido > MensualidadSobranteHelper.Centavo)
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
                                    Descripcion = "Abono a enero",
                                    UsuarioRegistro = pagos.Pago.UsuarioRegistro,
                                    Activo = true,
                                    FechaRegistro = DateTime.Now,
                                    Serie = matriculaExistente.Serie
                                };
                                bool esAbonoEneroReparar = tarifaMensual > 0
                                    && eneroDistribuido < tarifaMensual - MensualidadSobranteHelper.Centavo;
                                var (reparado, errorReparo) = await AsegurarEneroRegistradoConMatricula(
                                    plantillaEnero, periodoMatriculaRef, eneroDistribuido, esAbonoEneroReparar);
                                listpagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
                                periodosConsulta = ResolverPeriodosConsultaPago(
                                    listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                                if (!reparado)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el abono de enero proveniente de la matrícula. "
                                        + (errorReparo ?? "Verifique la API y el catálogo de mensualidad.");
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                            }
                        }

                        bool tieneMediaBeca = await MediaBeca(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo);
                        if (tieneMediaBeca)
                            tarifaMensual *= 0.5m;

                        var estadoMatCheck = await EvaluarMatriculaComoEstadoCuentaAsync(
                            pagos.Pago.IdAlumno,
                            pagos.Pago.IdRecinto,
                            pagos.Pago.IdGrado,
                            pagos.Pago.IdModalidad,
                            pagos.Pago.IdPeriodo);
                        if (!estadoMatCheck.Cancelada && estadoMatCheck.SaldoPendiente > 0.01m)
                        {
                            TempData["Mensaje"] =
                                $"La matrícula aún tiene saldo pendiente de C$ {estadoMatCheck.SaldoPendiente:N2}. "
                                + "Debe pagar o abonar la matrícula antes de registrar un pago o abono de mensualidad.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }

                        decimal montoPorMes = tarifaMensual;

                        var pagadoPorMes = listpagos
                            .Where(p => p.IdAlumno == pagos.Pago.IdAlumno &&
                                        p.IdTipoMovimiento == TipoMovimientoMensualidad &&
                                        periodosConsulta.Contains(p.IdPeriodo) &&
                                        p.IdMes.HasValue &&
                                        p.Activo &&
                                        EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, pagos.Pago.IdRecinto))
                            .GroupBy(p => p.IdMes!.Value)
                            .ToDictionary(g => g.Key, g => g.Sum(p => p.Monto));

                        decimal catalogoMatMensual = await ObtenerMatriculaDecimal(
                            pagos.Pago.IdRecinto, pagos.Pago.IdModalidad, pagos.Pago.IdPeriodo);
                        AplicarDistribucionMatriculaEnero(
                            pagadoPorMes, listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo, catalogoMatMensual, montoPorMes, pagos.Pago.IdRecinto);
                        var mesesConAlgunPago = pagadoPorMes.Where(kv => kv.Value > MensualidadSobranteHelper.Centavo)
                            .Select(kv => kv.Key)
                            .ToHashSet();

                        var idsOrdenados = ids;
                        var mesesEnEstePago = idsOrdenados.ToHashSet();

                        if (pagos.Pago.Monto <= MensualidadSobranteHelper.Centavo)
                        {
                            TempData["Mensaje"] = "El monto del abono o pago de mensualidad debe ser mayor a cero.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }

                        int primerMesPagadoMensual = mesesConAlgunPago.Any() ? mesesConAlgunPago.Min() : 1;
                        int mesCalendarioMatricula = ObtenerMesCalendarioPrimeraMatricula(listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo, pagos.Pago.IdRecinto);
                        bool matriculaInicioCiclo = EsMatriculaInicioCicloLectivo(mesCalendarioMatricula);
                        bool esContinuidad = EsMatriculaContinuidad(listpagos, pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo, pagos.Pago.IdRecinto);
                        if (!esContinuidad)
                        {
                            var alumnoMens = await _Iservices.GetAlumnoIdAsync(pagos.Pago.IdAlumno);
                            if (EsTipoEstudianteContinuidad(alumnoMens?.TipoEstudiante))
                                esContinuidad = true;
                        }
                        int inicioSecuenciaMensual = CalcularInicioSecuenciaMensual(
                            matriculaInicioCiclo, esContinuidad, mesCalendarioMatricula, primerMesPagadoMensual);

                        bool MesCanceladoLocal(int mes) =>
                            MensualidadSobranteHelper.MesCancelado(montoPorMes, pagadoPorMes.GetValueOrDefault(mes));

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
                            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroRecibo == pagos.Pago.NumeroRecibo && r.IdMes == idMes && r.IdPeriodo == pagos.Pago.IdPeriodo && r.Serie == "A" && r.Activo == true);
                            if (validarDuplicado)
                            {
                                TempData["Mensaje"] = "El número de Recibo ya Existe.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }

                            if (MesCanceladoLocal(idMes))
                            {
                                TempData["Mensaje"] = $"El mes {Mes(idMes).Result} ya fue pagado por este alumno.";
                                TempData["Tipo"] = "warning";
                                continue;
                            }

                            if (!matriculaInicioCiclo
                                && !esContinuidad
                                && idMes < mesCalendarioMatricula
                                && !mesesConAlgunPago.Contains(idMes))
                            {
                                TempData["Mensaje"] =
                                    $"Con matrícula en {Mes(mesCalendarioMatricula).Result} (sin continuidad), no corresponde pagar por separado el mes de {Mes(idMes).Result}. Marque continuidad al registrar la matrícula si el alumno sigue activo este ciclo y debe meses anteriores.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }

                            List<int> mesesPendientes;
                            if (idMes <= inicioSecuenciaMensual)
                                mesesPendientes = new List<int>();
                            else
                            {
                                var entre = Enumerable.Range(inicioSecuenciaMensual, idMes - inicioSecuenciaMensual).ToList();
                                mesesPendientes = entre
                                    .Where(m => !MesCanceladoLocal(m) && !mesesEnEstePago.Contains(m))
                                    .ToList();
                            }

                            if (mesesPendientes.Any())
                            {
                                var primerMesFaltante = mesesPendientes.Min();
                                TempData["Mensaje"] =
                                    $"No puede pagar el mes de {Mes(idMes).Result} sin {Mes(primerMesFaltante).Result}. " +
                                    "Incluya ese mes (y los anteriores pendientes) en la misma selección del modal.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }
                        }

                        var aplicaciones = MensualidadSobranteHelper.Distribuir(
                            montoPorMes, pagadoPorMes, idsOrdenados, pagos.Pago.Monto);
                        if (aplicaciones.Count == 0)
                        {
                            TempData["Mensaje"] = "No hay saldo de mensualidad para aplicar este pago.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }

                        int? idPeriodoSiguiente = null;
                        if (aplicaciones.Any(a => a.PeriodoOffset > 0))
                        {
                            var periodosCatSiguiente = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
                            var periodoPagoActual = periodosCatSiguiente.FirstOrDefault(p => p.IdPeriodo == pagos.Pago.IdPeriodo);
                            if (periodoPagoActual != null)
                            {
                                int anioSiguiente = CicloLectivoHelper.AnioSiguienteCiclo(periodoPagoActual.Periodo);
                                idPeriodoSiguiente = periodosCatSiguiente
                                    .FirstOrDefault(p => p.Activo && p.Periodo == anioSiguiente)?.IdPeriodo;
                            }
                        }

                        if (aplicaciones.Any(a => a.PeriodoOffset > 0) && !idPeriodoSiguiente.HasValue)
                        {
                            decimal extraSinCiclo = aplicaciones.Where(a => a.PeriodoOffset > 0).Sum(a => a.Monto);
                            aplicaciones = aplicaciones.Where(a => a.PeriodoOffset == 0).ToList();
                            var ultima = aplicaciones.LastOrDefault();
                            if (ultima != null && extraSinCiclo > 0)
                                ultima.Monto += extraSinCiclo;
                            else if (extraSinCiclo > 0)
                            {
                                aplicaciones.Add(new AplicacionMensualidad
                                {
                                    IdMes = 12,
                                    Monto = extraSinCiclo,
                                    EsAbono = true,
                                    EsSobranteSiguienteMes = true,
                                    PeriodoOffset = 0
                                });
                            }
                        }

                        bool primeraLinea = true;
                        var nombresAplicados = new List<string>();
                        int idUltimaLinea = 0;
                        foreach (var app in aplicaciones)
                        {
                            int idPeriodoLinea = pagos.Pago.IdPeriodo;
                            if (app.PeriodoOffset > 0)
                            {
                                if (!idPeriodoSiguiente.HasValue)
                                    continue;
                                idPeriodoLinea = idPeriodoSiguiente.Value;
                            }

                            string nombreMes = await Mes(app.IdMes);
                            string descripcionLinea = pagos.Pago.Descripcion ?? "";
                            if (app.EsAbono || app.EsSobranteSiguienteMes)
                            {
                                string etiquetaAbono = $"Abono a {nombreMes}";
                                descripcionLinea = string.IsNullOrWhiteSpace(descripcionLinea)
                                    ? etiquetaAbono
                                    : descripcionLinea + " | " + etiquetaAbono;
                            }

                            var nuevoPago = CrearLineaMensualidad(
                                pagos.Pago,
                                app.IdMes,
                                app.Monto,
                                idPeriodoLinea,
                                primeraLinea ? pagos.Pago.Mora : 0,
                                descripcionLinea);

                            (response, var idLineaMens, _) = await _Iservices.PostPagosAsync(nuevoPago);
                            if (!response)
                            {
                                TempData["Mensaje"] = $"No se logró procesar el pago del mes {nombreMes}.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }
                            if (idLineaMens > 0)
                                idUltimaLinea = idLineaMens;

                            primeraLinea = false;
                            nombresAplicados.Add(app.EsAbono || app.EsSobranteSiguienteMes
                                ? $"abono {nombreMes} C$ {app.Monto:N2}"
                                : $"{nombreMes} C$ {app.Monto:N2}");
                        }

                        decimal noAplicado = MensualidadSobranteHelper.SobranteNoAplicado(pagos.Pago.Monto, aplicaciones);

                        var pagosActualizados = await _Iservices.GetPagosAsync();
                        var idPag = idUltimaLinea > 0
                            ? idUltimaLinea
                            : ResolverIdPagoTrasRegistro(0, pagosActualizados, pagos.Pago);
                        if (idPag <= 0)
                            idPag = ResolverIdPagoMaximoSeguro(pagosActualizados);
                        if (idPag <= 0)
                        {
                            TempData["Mensaje"] = "Pago registrado, pero no se pudo obtener el recibo. Consulte el listado de pagos.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Index", "Pagos");
                        }

                        string mensajeOk = nombresAplicados.Count == 0
                            ? "Pago registrado correctamente."
                            : "Pago registrado: " + string.Join("; ", nombresAplicados) + ".";
                        if (noAplicado > MensualidadSobranteHelper.Centavo)
                            mensajeOk += $" Quedó un sobrante de C$ {noAplicado:N2} sin mes destino (fin de ciclo o siguiente ciclo no habilitado).";
                        if (tieneMediaBeca)
                            mensajeOk += " El estudiante tiene media beca (tarifa al 50%).";

                        TempData["Mensaje"] = mensajeOk;
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Details", "Pagos", new { id = idPag, imprimir = true });
                        // 4️⃣ Mensaje final
                       
                        
                        

                    }
                    else
                    {
                        if (esConfirmacionMatricula)
                        {
                            int periodoConfirmacion = await ResolverIdPeriodoConfirmacionAsync(pagos.Pago.IdPeriodo);
                            pagos.Pago.IdPeriodo = periodoConfirmacion;

                            var pagosReservaPrevios = (await _Iservices.GetPagosAsync() ?? new List<TblPago>())
                                .Where(a => a.IdAlumno == pagos.Pago.IdAlumno
                                    && a.IdPeriodo == periodoConfirmacion
                                    && a.Activo
                                    && a.IdTipoMovimiento != TipoMovimientoMensualidad)
                                .ToList();
                            decimal yaReservado = pagosReservaPrevios
                                .Where(p =>
                                {
                                    var conc = NormalizarTexto(
                                        tiposMovimientoCatalogo.FirstOrDefault(t => t.IdTipoMovimiento == p.IdTipoMovimiento)?.Concepto);
                                    return EsConfirmacionDeMatricula(p.IdTipoMovimiento, conc);
                                })
                                .Sum(p => p.Monto);

                            if (yaReservado < MontoMinimoReservaCupo
                                && pagos.Pago.Monto + MensualidadSobranteHelper.Centavo < MontoMinimoReservaCupo)
                            {
                                TempData["Mensaje"] = $"Para reservar el cupo del siguiente ciclo el monto mínimo es C$ {MontoMinimoReservaCupo:N2}.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            if (pagos.Pago.Monto <= MensualidadSobranteHelper.Centavo)
                            {
                                TempData["Mensaje"] = "El monto de la confirmación de matrícula debe ser mayor que cero.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            (response, var idConfirmacion, var errorConfirmacion) = await _Iservices.PostPagosAsync(pagos.Pago);
                            if (!response)
                            {
                                var msg = "No se pudo registrar la confirmación de matrícula.";
                                if (!string.IsNullOrWhiteSpace(errorConfirmacion))
                                    msg += " Detalle: " + errorConfirmacion;
                                TempData["Mensaje"] = msg;
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }

                            var pagosDespuesConfirmacion = await _Iservices.GetPagosAsync();
                            int idPagoConfirmacion = ResolverIdPagoTrasRegistro(idConfirmacion, pagosDespuesConfirmacion, pagos.Pago);
                            int idParaReciboConfirmacion = idPagoConfirmacion > 0 ? idPagoConfirmacion : idConfirmacion;

                            TempData["Mensaje"] = "Confirmación de matrícula registrada: reserva de cupo del siguiente ciclo. No incluye mensualidad de enero ni abona la matrícula del año en curso.";
                            TempData["Tipo"] = "success";
                            return RedirectToAction("Details", "Pagos", new { id = idParaReciboConfirmacion, imprimir = true });
                        }
                        else if (esMatriculaCompleta || esAbonoMatricula)
                        {
                            var periodosCat = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
                            var periodoActualCiclo = CicloLectivoHelper.ResolverPeriodoActual(periodosCat);
                            int periodoMatricula = pagos.Pago.IdPeriodo;
                            bool esContinuidadMatricula = pagos.Pago.Continuidad;
                            int mesPresentacion = pagos.Pago.FechaEmision?.Month ?? DateTime.Now.Month;
                            var alumnoPago = await _Iservices.GetAlumnoIdAsync(pagos.Pago.IdAlumno);

                            if (!esContinuidadMatricula)
                            {
                                if (EsTipoEstudianteContinuidad(alumnoPago?.TipoEstudiante)
                                    && !EsMatriculaInicioCicloLectivo(mesPresentacion))
                                    esContinuidadMatricula = true;
                            }

                            bool pendienteCicloActual = periodoActualCiclo != null
                                && await TienePendienteMatriculaCicloAsync(
                                    pagos.Pago.IdAlumno,
                                    periodoActualCiclo,
                                    pagos.Pago.IdRecinto,
                                    pagos.Pago.IdGrado,
                                    pagos.Pago.IdModalidad);

                            var pagosCicloActual = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
                            bool tienePagosCicloVigente = periodoActualCiclo != null && pagosCicloActual.Any(p =>
                                p.IdAlumno == pagos.Pago.IdAlumno
                                && p.IdPeriodo == periodoActualCiclo.IdPeriodo
                                && p.Activo);
                            bool esDelCicloVigente = periodoActualCiclo != null && (
                                alumnoPago?.IdPeriodo == periodoActualCiclo.IdPeriodo
                                || tienePagosCicloVigente);

                            var cicloEnCurso = CicloLectivoHelper.ResolverPeriodoMensualidad(periodosCat)
                                ?? periodoActualCiclo;
                            bool forzarCicloActual = esAbonoMatricula
                                || esContinuidadMatricula
                                || (esDelCicloVigente && pendienteCicloActual);
                            if (forzarCicloActual && cicloEnCurso != null)
                                periodoMatricula = cicloEnCurso.IdPeriodo;
                            
                            // Validar que los campos requeridos estén presentes
                            if (pagos.Pago.IdGrado == 0)
                            {
                                TempData["Mensaje"] = "Debe seleccionar el Nivel (Grado) para procesar la matrícula o el abono.";
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

                            if (await BecaCompleta(pagos.Pago.IdAlumno, periodoMatricula))
                                restarMensualidad = 0m;
                            else if (restarMensualidad > 0 && await MediaBeca(pagos.Pago.IdAlumno, periodoMatricula))
                                restarMensualidad *= 0.5m;

                            var totalMatricula = Convert.ToDecimal(pagos.Pago.Monto);
                            var pagosPreviosCiclo = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
                            decimal totalPagadoMatricula = pagosPreviosCiclo
                                .Where(a => a.IdAlumno == pagos.Pago.IdAlumno &&
                                           (a.IdTipoMovimiento == TipoMovimientoMatricula
                                            || a.IdTipoMovimiento == TipoMovimientoMatriculaAbono) &&
                                           a.IdPeriodo == periodoMatricula &&
                                           a.Activo &&
                                           EstadoCuentaCalculoHelper.EsPagoDelRecinto(a, pagos.Pago.IdRecinto))
                                .Sum(p => p.Monto);
                            decimal totalPagadoEnero = pagosPreviosCiclo
                                .Where(a => a.IdAlumno == pagos.Pago.IdAlumno
                                    && a.IdPeriodo == periodoMatricula
                                    && a.IdTipoMovimiento == TipoMovimientoMensualidad
                                    && a.IdMes == 1
                                    && a.Activo
                                    && EstadoCuentaCalculoHelper.EsPagoDelRecinto(a, pagos.Pago.IdRecinto))
                                .Sum(p => p.Monto);

                            decimal matriculaNeta = EstadoCuentaCalculoHelper.MatriculaNetaDesdePaquete(
                                obtenerMat, restarMensualidad);
                            var (pagadoMatDist, pagadoEneDist) = EstadoCuentaCalculoHelper.DistribuirPagoMatriculaYEnero(
                                obtenerMat, totalPagadoMatricula, totalPagadoEnero, restarMensualidad);
                            decimal faltaMatricula = Math.Max(0m, matriculaNeta - pagadoMatDist);
                            decimal faltaEnero = Math.Max(0m, restarMensualidad - pagadoEneDist);

                            if (faltaMatricula <= MensualidadSobranteHelper.Centavo
                                && faltaEnero <= MensualidadSobranteHelper.Centavo)
                            {
                                TempData["Mensaje"] = "La matrícula y la mensualidad de enero ya están pagadas.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            decimal maxPermitido = faltaMatricula + faltaEnero;
                            if (totalMatricula > maxPermitido + MensualidadSobranteHelper.Tolerancia)
                            {
                                TempData["Mensaje"] = $"El monto (C$ {totalMatricula:N2}) supera lo pendiente (matrícula C$ {faltaMatricula:N2} + enero C$ {faltaEnero:N2}; máximo C$ {maxPermitido:N2}).";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            if (totalMatricula <= MensualidadSobranteHelper.Centavo)
                            {
                                TempData["Mensaje"] = "Ingrese un monto mayor a cero.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            var (aMatricula, aEnero) = EstadoCuentaCalculoHelper.AsignarPagoAMatriculaLuegoEnero(
                                faltaMatricula, faltaEnero, totalMatricula);
                            if (aMatricula + aEnero <= MensualidadSobranteHelper.Centavo)
                            {
                                TempData["Mensaje"] = "El monto no se pudo aplicar a matrícula ni a enero.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            int idPagoMatricula = 0;
                            pagos.Pago.IdPeriodo = periodoMatricula;
                            pagos.Pago.Continuidad = esContinuidadMatricula;

                            if (aMatricula > MensualidadSobranteHelper.Centavo)
                            {
                                pagos.Pago.Monto = aMatricula;
                                (response, var idMatriculaPost, _) = await _Iservices.PostPagosAsync(pagos.Pago);
                                if (!response)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el pago de matrícula.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }

                                var pagosDespuesMatricula = await _Iservices.GetPagosAsync();
                                idPagoMatricula = ResolverIdPagoTrasRegistro(idMatriculaPost, pagosDespuesMatricula, pagos.Pago);
                                if (idPagoMatricula <= 0)
                                    idPagoMatricula = idMatriculaPost;
                            }

                            if (aEnero > MensualidadSobranteHelper.Centavo)
                            {
                                bool esAbonoEnero = aEnero < faltaEnero - MensualidadSobranteHelper.Centavo;
                                var (eneroOk, errorEnero) = await AsegurarEneroRegistradoConMatricula(
                                    pagos.Pago, periodoMatricula, aEnero, esAbonoEnero);
                                if (!eneroOk)
                                {
                                    TempData["Mensaje"] = "Matrícula registrada, pero no se pudo registrar el abono de enero. "
                                        + (errorEnero ?? "Revise el listado e intente registrar enero en mensualidad.");
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                            }

                            bool matriculaQuedaPagada = aMatricula >= faltaMatricula - MensualidadSobranteHelper.Centavo
                                || faltaMatricula <= MensualidadSobranteHelper.Centavo;
                            string detalleEnero = aEnero > MensualidadSobranteHelper.Centavo
                                ? (aEnero < faltaEnero - MensualidadSobranteHelper.Centavo
                                    ? $" Se abonaron C$ {aEnero:N2} a enero (pendiente C$ {faltaEnero - aEnero:N2})."
                                    : " Se pagó la mensualidad de enero.")
                                : string.Empty;
                            string detalleMat = matriculaQuedaPagada
                                ? "Matrícula pagada."
                                : $"Abono de matrícula C$ {aMatricula:N2} (pendiente C$ {faltaMatricula - aMatricula:N2}).";

                            int idParaRecibo = idPagoMatricula;
                            if (idParaRecibo <= 0)
                            {
                                var pagosTrasEnero = await _Iservices.GetPagosAsync();
                                idParaRecibo = ResolverIdPagoTrasRegistro(0, pagosTrasEnero, pagos.Pago);
                            }

                            TempData["Mensaje"] = (esAbonoMatricula ? "Abono del ciclo en curso. " : string.Empty)
                                + (esContinuidadMatricula ? "Continuidad. " : string.Empty)
                                + detalleMat + detalleEnero;
                            TempData["Tipo"] = "success";
                            return RedirectToAction("Details", "Pagos", new { id = idParaRecibo, imprimir = true });
                           
                        }
                        else
                        {
                            bool esPagoMatricula = !esConfirmacionMatricula
                                && (pagos.Pago.IdTipoMovimiento == TipoMovimientoMatricula
                                    || (conceptoTipoMovimiento.Contains("matricula")
                                        && conceptoTipoMovimiento.Contains("completa")));

                            (response, var idPagoSimple, var errorPostSimple) = await _Iservices.PostPagosAsync(pagos.Pago);
                            if (response)
                            {
                                if (esPagoMatricula && pagos.Pago.IdGrado > 0)
                                {
                                    decimal tarifaEneroSimple = await ObtenerMensualidadDecimal(
                                        pagos.Pago.IdRecinto, pagos.Pago.IdGrado,
                                        pagos.Pago.IdPeriodo, pagos.Pago.IdModalidad);
                                    decimal tarifaMatSimple = await ObtenerMatriculaDecimal(
                                        pagos.Pago.IdRecinto, pagos.Pago.IdModalidad, pagos.Pago.IdPeriodo);
                                    if (await BecaCompleta(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo))
                                        tarifaEneroSimple = 0m;
                                    else if (tarifaEneroSimple > 0 && await MediaBeca(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo))
                                        tarifaEneroSimple *= 0.5m;

                                    var pagosTrasSimple = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
                                    decimal yaMatSimple = pagosTrasSimple
                                        .Where(p => p.Activo && p.IdAlumno == pagos.Pago.IdAlumno
                                            && p.IdPeriodo == pagos.Pago.IdPeriodo
                                            && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, pagos.Pago.IdRecinto)
                                            && (p.IdTipoMovimiento == TipoMovimientoMatricula
                                                || p.IdTipoMovimiento == TipoMovimientoMatriculaAbono))
                                        .Sum(p => p.Monto);
                                    decimal yaEneSimple = pagosTrasSimple
                                        .Where(p => p.Activo && p.IdAlumno == pagos.Pago.IdAlumno
                                            && p.IdPeriodo == pagos.Pago.IdPeriodo
                                            && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, pagos.Pago.IdRecinto)
                                            && p.IdTipoMovimiento == TipoMovimientoMensualidad
                                            && p.IdMes == 1)
                                        .Sum(p => p.Monto);
                                    var (_, eneDistSimple) = EstadoCuentaCalculoHelper.DistribuirPagoMatriculaYEnero(
                                        tarifaMatSimple, yaMatSimple, yaEneSimple, tarifaEneroSimple);
                                    decimal eneroARegistrar = Math.Max(0m, eneDistSimple - yaEneSimple);
                                    if (tarifaEneroSimple > 0)
                                        eneroARegistrar = Math.Min(eneroARegistrar, Math.Max(0m, tarifaEneroSimple - yaEneSimple));
                                    if (eneroARegistrar > MensualidadSobranteHelper.Centavo)
                                    {
                                        bool esAbonoEneroSimple = tarifaEneroSimple > 0
                                            && (yaEneSimple + eneroARegistrar) < tarifaEneroSimple - MensualidadSobranteHelper.Centavo;
                                        var (eneroOk, errorEnero) = await AsegurarEneroRegistradoConMatricula(
                                            pagos.Pago, pagos.Pago.IdPeriodo, eneroARegistrar, esAbonoEneroSimple);
                                        if (!eneroOk)
                                        {
                                            TempData["Mensaje"] = "Matrícula registrada, pero no se pudo registrar el abono de enero: "
                                                + (errorEnero ?? "revise el listado.");
                                            TempData["Tipo"] = "warning";
                                            return RedirectToAction("Create");
                                        }
                                    }
                                }

                                var pagosTrasSimpleId = await _Iservices.GetPagosAsync();
                                var idPag = idPagoSimple > 0
                                    ? idPagoSimple
                                    : ResolverIdPagoTrasRegistro(0, pagosTrasSimpleId, pagos.Pago);
                                if (idPag <= 0)
                                    idPag = ResolverIdPagoMaximoSeguro(pagosTrasSimpleId);

                                TempData["Mensaje"] = "Pago registrado correctamente.";
                                TempData["Tipo"] = "success";
                                if (idPag <= 0)
                                    return RedirectToAction("Index", "Pagos");
                                return RedirectToAction("Details", "Pagos", new { id = idPag, imprimir = true });
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
            var alumnoMoraDatos = _Iservices.GetAlumnoIdAsync(idAlumno).Result;
            var pagosMensualidad = listpagos
                .Where(p => p.IdAlumno == idAlumno &&
                            p.IdTipoMovimiento == idTipoMensualidad &&
                            p.IdPeriodo == periodo &&
                            p.IdMes.HasValue &&
                            EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, alumnoMoraDatos?.IdRecinto))
                .ToList();
            var mesesPagadosBD = pagosMensualidad
                .GroupBy(p => p.IdMes!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Monto));

            decimal tarifaMora = 0m;
            if (alumnoMoraDatos?.IdRecinto != null && alumnoMoraDatos.IdGrado.HasValue)
            {
                tarifaMora = ObtenerMensualidadDecimal(
                    alumnoMoraDatos.IdRecinto,
                    alumnoMoraDatos.IdGrado.Value,
                    periodo,
                    alumnoMoraDatos.IdModalidad).GetAwaiter().GetResult();
                if (tarifaMora > 0 && alumnoMoraDatos.MediaBeca == true)
                    tarifaMora *= 0.5m;
            }

            bool MesMoraCancelado(int idMes)
            {
                decimal pagado = mesesPagadosBD.GetValueOrDefault(idMes);
                if (tarifaMora > 0)
                    return MensualidadSobranteHelper.MesCancelado(tarifaMora, pagado);
                return pagado > 0;
            }

            int mesMatricula = ObtenerMesCalendarioPrimeraMatricula(listpagos, idAlumno, periodo, alumnoMoraDatos?.IdRecinto);
            bool matriculaInicioCiclo = EsMatriculaInicioCicloLectivo(mesMatricula);
            bool esContinuidad = EsMatriculaContinuidad(listpagos, idAlumno, periodo, alumnoMoraDatos?.IdRecinto);
            if (!esContinuidad && EsTipoEstudianteContinuidad(alumnoMoraDatos?.TipoEstudiante))
                esContinuidad = true;

            if (!matriculaInicioCiclo && !esContinuidad && mesMatricula > mes)
            {
                return Json(new { mora = 0, mes, aplicaMora = false });
            }

            decimal catalogoMatMora = 0m;
            if (alumnoMoraDatos?.IdRecinto != null)
                catalogoMatMora = ObtenerMatriculaDecimal(
                    alumnoMoraDatos.IdRecinto, alumnoMoraDatos.IdModalidad, periodo).GetAwaiter().GetResult();
            AplicarDistribucionMatriculaEnero(mesesPagadosBD, listpagos, idAlumno, periodo, catalogoMatMora, tarifaMora, alumnoMoraDatos?.IdRecinto);

            int mesInicioMora = (matriculaInicioCiclo || esContinuidad) ? 1 : mesMatricula;
            int cantidadMesesEnVentana = mes - mesInicioMora;
            if (cantidadMesesEnVentana <= 0)
            {
                return Json(new { mora = 0, mes, aplicaMora = false });
            }

            var mesesPendientes = Enumerable.Range(mesInicioMora, cantidadMesesEnVentana)
                .Where(m => !MesMoraCancelado(m))
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

        private static (bool Cancelada, decimal SaldoPendiente) CalcularEstadoMatriculaEstadoCuenta(
            decimal montoCatalogo,
            decimal montoMensualidadEnero,
            decimal pagadoMatricula,
            decimal pagadoMensualidadEnero)
            => EstadoCuentaCalculoHelper.CalcularEstadoMatricula(
                montoCatalogo, montoMensualidadEnero, pagadoMatricula, pagadoMensualidadEnero);

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
        /// Mensualidad, morosos y adelantos: siempre el ciclo marcado como actual.
        /// Habilitar 2027 no debe mover estos cobros.
        /// </summary>
        private static int ResolverIdPeriodoMensualidad(IEnumerable<CatPeriodo>? periodos, int idPeriodoFormulario)
        {
            var actual = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos);
            if (actual == null)
                return idPeriodoFormulario;

            var siguiente = CicloLectivoHelper.ResolverPeriodoSiguiente(periodos);
            if (siguiente != null && idPeriodoFormulario == siguiente.IdPeriodo && !siguiente.Actual)
                return actual.IdPeriodo;

            return idPeriodoFormulario > 0 ? idPeriodoFormulario : actual.IdPeriodo;
        }

        private async Task<int> ResolverIdPeriodoConfirmacionAsync(int idPeriodoFormulario)
        {
            var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var destino = CicloLectivoHelper.ResolverPeriodoMatricula(periodos, DateTime.Now, forzarCicloActual: false);
            return destino?.IdPeriodo ?? idPeriodoFormulario;
        }

        private async Task<bool> TienePendienteMatriculaCicloAsync(
            int idAlumno,
            CatPeriodo ciclo,
            int? idRecinto,
            int idGrado,
            int? idModalidad)
        {
            if (idAlumno <= 0 || ciclo == null)
                return false;

            var estado = await EvaluarMatriculaComoEstadoCuentaAsync(
                idAlumno, idRecinto, idGrado, idModalidad, ciclo.IdPeriodo);
            return !estado.Cancelada && estado.SaldoPendiente > 0.01m;
        }

        /// <summary>
        /// Misma regla que el estado de cuenta: tipos de matrícula del catálogo
        /// y pagos del mismo año lectivo, no solo tipos 2/4 del IdPeriodo del formulario.
        /// </summary>
        private async Task<(bool Cancelada, decimal SaldoPendiente)> EvaluarMatriculaComoEstadoCuentaAsync(
            int idAlumno,
            int? idRecintoForm,
            int idGradoForm,
            int? idModalidadForm,
            int idPeriodoForm)
        {
            var alumno = await _Iservices.GetAlumnoIdAsync(idAlumno);
            var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoRef = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos);
            int idPeriodoRef = periodoRef?.IdPeriodo ?? idPeriodoForm;
            int anioRef = periodoRef is { Periodo: > 0 } ? periodoRef.Periodo : DateTime.Now.Year;

            var mats = (await _Iservices.GetMatriculasAsync(idAlumno: idAlumno) ?? new List<TblMatricula>())
                .Where(m => m.Activo)
                .ToList();
            var ciclo = EstadoCuentaCalculoHelper.ResolverCicloAlumno(
                alumno ?? new TblAlumno { IdAlumno = idAlumno },
                mats,
                periodos,
                idPeriodoRef,
                anioRef);

            var mat = ciclo.Matricula;
            int? idR = mat is { IdRecinto: > 0 } ? mat.IdRecinto : (idRecintoForm ?? alumno?.IdRecinto);
            int idG = mat is { IdGrado: > 0 }
                ? mat.IdGrado
                : (idGradoForm > 0 ? idGradoForm : alumno?.IdGrado ?? 0);
            int? idMod = mat is { IdModalidad: > 0 } ? mat.IdModalidad : (idModalidadForm ?? alumno?.IdModalidad);

            decimal costoMat = await ObtenerMatriculaDecimal(idR, idMod, ciclo.IdPeriodo);
            decimal costoMen = idG > 0
                ? await ObtenerMensualidadDecimal(idR, idG, ciclo.IdPeriodo, idMod)
                : 0m;
            if (await BecaCompleta(idAlumno, ciclo.IdPeriodo))
                costoMen = 0m;
            else if (costoMen > 0 && await MediaBeca(idAlumno, ciclo.IdPeriodo))
                costoMen *= 0.5m;

            var pagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            var tipos = await _Iservices.GetTipoMovimientoAsync() ?? new List<CatTipoMovimiento>();
            var tiposMat = EstadoCuentaCalculoHelper.TiposPagoMatricula(tipos);
            var tiposMen = EstadoCuentaCalculoHelper.TiposPagoMensualidad(tipos);
            decimal pagadoMat = EstadoCuentaCalculoHelper.SumarPagadoMatricula(
                pagos, idAlumno, ciclo.IdPeriodo, ciclo.Anio, periodos, tiposMat, idR);
            decimal pagadoEnero = EstadoCuentaCalculoHelper.SumarPagadoEnero(
                pagos, idAlumno, ciclo.IdPeriodo, tiposMen, idR);

            return EstadoCuentaCalculoHelper.CalcularEstadoMatricula(costoMat, costoMen, pagadoMat, pagadoEnero);
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
            var periodoActual = CicloLectivoHelper.ResolverPeriodoMensualidad(periodos);
            var periodoSiguiente = CicloLectivoHelper.ResolverPeriodoSiguiente(periodos);
            var filaMatriculaCiclo = periodoActual != null
                ? await _Iservices.GetMatriculaAlumnoPeriodoAsync(idAlumno, periodoActual.IdPeriodo)
                : null;
            bool sugerirContinuidad = EsTipoEstudianteContinuidad(alumno.TipoEstudiante)
                || filaMatriculaCiclo?.Continuidad == true;

            bool pendienteMatriculaCicloActual = false;
            if (periodoActual != null)
            {
                pendienteMatriculaCicloActual = await TienePendienteMatriculaCicloAsync(
                    idAlumno,
                    periodoActual,
                    alumno.IdRecinto,
                    alumno.IdGrado ?? 0,
                    alumno.IdModalidad);
            }

            var pagosAlumno = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            bool tienePagosCicloActual = periodoActual != null && pagosAlumno.Any(p =>
                p.IdAlumno == idAlumno && p.IdPeriodo == periodoActual.IdPeriodo && p.Activo);
            bool esDelCicloVigente = periodoActual != null && (
                alumno.IdPeriodo == periodoActual.IdPeriodo
                || filaMatriculaCiclo != null
                || tienePagosCicloActual);

            bool forzarCicloActual = sugerirContinuidad
                || (esDelCicloVigente && pendienteMatriculaCicloActual);

            var periodoMatricula = CicloLectivoHelper.ResolverPeriodoMatricula(
                periodos, DateTime.Now, forzarCicloActual);
            var periodoConfirmacion = CicloLectivoHelper.ResolverPeriodoMatricula(
                periodos, DateTime.Now, forzarCicloActual: false);

            return Json(new
            {
                ok = true,
                idRecinto = alumno.IdRecinto,
                idModalidad = alumno.IdModalidad,
                idGrado = alumno.IdGrado,
                idTurno = alumno.IdTurno,
                idPeriodo = periodoActual?.IdPeriodo ?? alumno.IdPeriodo,
                idPeriodoMatricula = periodoMatricula?.IdPeriodo ?? periodoActual?.IdPeriodo ?? alumno.IdPeriodo,
                idPeriodoConfirmacion = periodoConfirmacion?.IdPeriodo ?? periodoSiguiente?.IdPeriodo ?? periodoActual?.IdPeriodo ?? alumno.IdPeriodo,
                tipoEstudiante = alumno.TipoEstudiante,
                sugerirContinuidad,
                tienePendienteMatriculaCicloActual = pendienteMatriculaCicloActual
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
        public async Task<IActionResult> ObtenerSaldoMeses(
            int idAlumno, int idPeriodo, int? idRecinto, int idGrado, int? idModalidad, string? meses)
        {
            decimal tarifa = await ObtenerMensualidadDecimal(idRecinto, idGrado, idPeriodo, idModalidad);
            if (tarifa > 0)
            {
                if (await BecaCompleta(idAlumno, idPeriodo))
                    tarifa = 0m;
                else if (await MediaBeca(idAlumno, idPeriodo))
                    tarifa *= 0.5m;
            }

            var ids = (meses ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                .Where(n => n >= 1 && n <= 12)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var listpagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            var periodosConsulta = ResolverPeriodosConsultaPago(listpagos, idAlumno, idPeriodo);
            var pagadoPorMes = listpagos
                .Where(p => p.IdAlumno == idAlumno
                            && p.IdTipoMovimiento == TipoMovimientoMensualidad
                            && periodosConsulta.Contains(p.IdPeriodo)
                            && p.IdMes.HasValue
                            && p.Activo
                            && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto))
                .GroupBy(p => p.IdMes!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Monto));

            decimal catalogoMatSaldo = idRecinto.HasValue
                ? await ObtenerMatriculaDecimal(idRecinto, idModalidad, idPeriodo)
                : 0m;
            AplicarDistribucionMatriculaEnero(pagadoPorMes, listpagos, idAlumno, idPeriodo, catalogoMatSaldo, tarifa, idRecinto);

            var detalle = ids.Select(m =>
            {
                decimal pagado = pagadoPorMes.GetValueOrDefault(m);
                decimal restante = MensualidadSobranteHelper.RestanteMes(tarifa, pagado);
                bool cancelado = MensualidadSobranteHelper.MesCancelado(tarifa, pagado);
                return new
                {
                    idMes = m,
                    pagado,
                    restante,
                    cancelado,
                    tieneAbono = pagado > MensualidadSobranteHelper.Centavo && !cancelado
                };
            }).ToList();

            return Json(new
            {
                tarifa,
                totalRestante = detalle.Sum(d => d.restante),
                meses = detalle
            });
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
        public IActionResult ObtenerAbonosPrevios(int idAlumno, int idTipoMovimiento, int idPeriodo, int? idRecinto = null)
        {
            try
            {
                var listpagos = _Iservices.GetPagosAsync().Result ?? new List<TblPago>();
                var tiposMovimiento = _Iservices.GetTipoMovimientoAsync().Result ?? new List<CatTipoMovimiento>();
                
                // Buscar abonos previos según el tipo de movimiento
                decimal totalAbonos = 0;
                
                var tipoSel = tiposMovimiento?.FirstOrDefault(tm => tm.IdTipoMovimiento == idTipoMovimiento);
                var conceptoSel = NormalizarTexto(tipoSel?.Concepto);
                bool esReservaCupo = EsConfirmacionDeMatricula(idTipoMovimiento, conceptoSel);

                if (esReservaCupo)
                {
                    totalAbonos = listpagos
                        .Where(p => p.IdAlumno == idAlumno && p.IdPeriodo == idPeriodo && p.Activo
                            && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto)
                            && EsConfirmacionDeMatricula(
                                p.IdTipoMovimiento,
                                NormalizarTexto(tiposMovimiento.FirstOrDefault(t => t.IdTipoMovimiento == p.IdTipoMovimiento)?.Concepto)))
                        .Sum(p => p.Monto);
                }
                else if (idTipoMovimiento == TipoMovimientoMatricula
                    || idTipoMovimiento == TipoMovimientoMatriculaAbono
                    || EsAbonoMatriculaCicloActual(idTipoMovimiento, conceptoSel)
                    || EsMatriculaCompletaTipo(idTipoMovimiento, conceptoSel))
                {
                    totalAbonos = listpagos
                        .Where(p => p.IdAlumno == idAlumno
                            && p.IdPeriodo == idPeriodo
                            && p.Activo
                            && EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto)
                            && (p.IdTipoMovimiento == TipoMovimientoMatricula
                                || p.IdTipoMovimiento == TipoMovimientoMatriculaAbono))
                        .Sum(p => p.Monto);
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
                                        p.Activo == true &&
                                        EstadoCuentaCalculoHelper.EsPagoDelRecinto(p, idRecinto))
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

        [Authorize(Roles = "Admin,UserSystem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AnularRecibo(int id, DateTime? fechainicio, DateTime? fechafin)
        {
            var pago = await _Iservices.GetPagoById(id);
            if (pago == null)
            {
                TempData["Mensaje"] = "Recibo no encontrado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            if (!pago.Activo)
            {
                TempData["Mensaje"] = $"El recibo {pago.NumeroRecibo} ya está anulado.";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            var todosPagos = await _Iservices.GetPagosAsync() ?? new List<TblPago>();
            var filasRecibo = todosPagos
                .Where(p => p.NumeroRecibo == pago.NumeroRecibo
                    && p.Serie == pago.Serie
                    && p.IdAlumno == pago.IdAlumno
                    && p.IdPeriodo == pago.IdPeriodo
                    && p.Activo)
                .ToList();

            if (!filasRecibo.Any())
            {
                TempData["Mensaje"] = "No hay registros activos para anular.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ahora = DateTime.Now;
            var errores = 0;

            foreach (var fila in filasRecibo)
            {
                fila.Activo = false;
                fila.UsuarioActualizo = idUsuario;
                fila.FechaActualizo = ahora;
                if (!await _Iservices.UpdatePago(fila))
                    errores++;
            }

            if (errores == 0)
            {
                TempData["Mensaje"] = filasRecibo.Count > 1
                    ? $"Recibo {pago.NumeroRecibo} anulado correctamente ({filasRecibo.Count} registros)."
                    : $"Recibo {pago.NumeroRecibo} anulado correctamente.";
                TempData["Tipo"] = "success";
            }
            else
            {
                TempData["Mensaje"] = $"No se pudo anular completamente el recibo {pago.NumeroRecibo}. Revise la conexión con la API.";
                TempData["Tipo"] = "warning";
            }

            return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
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
