using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Helpers;
using WebColegio.Services;
using WebColegio.Views.Shared;

namespace WebColegio.Controllers
{
    public class NotasController : Controller
    {
        private readonly IServicesApi _Iservices;
        public NotasController(IServicesApi services)
        {
            _Iservices = services;
        }

        private static int? ObtenerNumeroGrado(string? nombreGrado)
        {
            if (string.IsNullOrWhiteSpace(nombreGrado)) return null;
            var normalizado = nombreGrado.Trim().ToLowerInvariant();
            var m = Regex.Match(normalizado, @"\d+");
            if (m.Success && int.TryParse(m.Value, out var numero))
                return numero;

            if (normalizado.Contains("primero") || normalizado.Contains("primer")) return 1;
            if (normalizado.Contains("segundo")) return 2;
            if (normalizado.Contains("tercero") || normalizado.Contains("tercer")) return 3;
            if (normalizado.Contains("cuarto")) return 4;
            if (normalizado.Contains("quinto")) return 5;
            if (normalizado.Contains("sexto")) return 6;
            if (normalizado.Contains("setimo") || normalizado.Contains("séptimo") || normalizado.Contains("septimo")) return 7;
            if (normalizado.Contains("octavo")) return 8;
            if (normalizado.Contains("noveno")) return 9;
            if (normalizado.Contains("decimo") || normalizado.Contains("décimo")) return 10;
            if (normalizado.Contains("undecimo") || normalizado.Contains("undécimo")) return 11;
            return null;
        }

        private static bool EsCualitativoSolo(string? modalidad, string? nombreGrado)
        {
            var mod = modalidad?.Trim().ToLowerInvariant() ?? string.Empty;
            if (mod.Contains("preescolar") || mod.Contains("pre escolar")) return true;

            var grado = ObtenerNumeroGrado(nombreGrado);
            return grado == 1 || grado == 2;
        }

        private static (string? Cualitativo, decimal? Cuantitativo) CompletarParCualiCuanti(string? cualitativo, decimal? cuantitativo)
        {
            if (string.IsNullOrWhiteSpace(cualitativo) && cuantitativo.HasValue)
                cualitativo = EscalaCualitativa.NumeroACualitativo(cuantitativo);

            if (!cuantitativo.HasValue && !string.IsNullOrWhiteSpace(cualitativo))
                cuantitativo = EscalaCualitativa.CualitativoANumero(cualitativo);

            return (cualitativo, cuantitativo);
        }

        /// <summary>Período evaluativo vigente por fechas; si no hay coincidencia, el primero activo.</summary>
        private static int? ResolverIdPeriodoEvaluacionPreferido(IEnumerable<PeriodoEvaluacion>? periodos)
        {
            var lista = periodos?
                .Where(p => p.Activo)
                .OrderBy(p => p.FechaInicio)
                .ToList() ?? new List<PeriodoEvaluacion>();
            if (lista.Count == 0) return null;
            var hoy = DateTime.Now.Date;
            var enCurso = lista.FirstOrDefault(p => hoy >= p.FechaInicio.Date && hoy <= p.FechaFin.Date);
            return (enCurso ?? lista[0]).IdPeriodo;
        }

        private static CatPeriodo? ObtenerPeriodoLectivoActual(IEnumerable<CatPeriodo>? periodos)
        {
            var lista = periodos?.Where(p => p.Activo).ToList() ?? new List<CatPeriodo>();
            return lista.FirstOrDefault(p => p.Actual)
                   ?? lista.OrderByDescending(p => p.Periodo).ThenByDescending(p => p.IdPeriodo).FirstOrDefault();
        }

        /// <summary>JSON para precargar modalidad y grado si la matrícula del alumno corresponde al período lectivo actual.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MatriculaAlumnoPeriodoActual(int idAlumno)
        {
            if (idAlumno <= 0)
                return Json(new { ok = false, mensaje = "Debe seleccionar un alumno válido." });

            var periodosLectivos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoActual = ObtenerPeriodoLectivoActual(periodosLectivos);
            var alumno = await _Iservices.GetAlumnoIdAsync(idAlumno);
            if (alumno == null || alumno.IdAlumno <= 0)
                return Json(new { ok = false, mensaje = "No se encontró el alumno." });

            int? idRecintoAlumno = alumno.IdRecinto;

            var matriculaEnPeriodoActual = periodoActual == null
                || !alumno.IdPeriodo.HasValue
                || alumno.IdPeriodo.Value == periodoActual.IdPeriodo;

            if (!alumno.IdModalidad.HasValue || !alumno.IdGrado.HasValue)
                return Json(new { ok = false, mensaje = "El alumno no tiene modalidad o grado registrados en su ficha.", idRecinto = idRecintoAlumno });

            return Json(new
            {
                ok = true,
                matriculaEnPeriodoActual,
                idModalidad = alumno.IdModalidad.Value,
                idGrado = alumno.IdGrado.Value,
                idRecinto = idRecintoAlumno
            });
        }

        // GET: NotasController
        [Authorize]
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {
            var _notas = await _Iservices.GetNotasAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoEvaluacion = await _Iservices.GetTipEvaluacionAsync();            
            var _periodo = await _Iservices.GetPeriodoEvaluacionAsync();
            var _asignatura = await _Iservices.GetAsignaturaAsync();
            var _modalidad = await _Iservices.GetModalidadesAsync();
            var _grados = await _Iservices.GetGradosAsync();

            IQueryable<TblNotas> query = _notas.AsQueryable();

            var (ini, fin) = ReporteFechaQuery.ResolverRango(Request, fechainicio, fechafin);
            if (ini.HasValue)
                query = query.Where(a => a.FechaRegistro.Date >= ini.Value.Date);
            if (fin.HasValue)
                query = query.Where(a => a.FechaRegistro.Date <= fin.Value.Date);

            var notasFiltrados = query
                .OrderByDescending(a => a.FechaRegistro)
                .ThenByDescending(a => a.IdNota)
                .ToList();
            var VieModelNotas = new ColeccionCatalogos
            {
                notas = notasFiltrados,
                alumno = _alumnos,
                tipoEvaluaciones = _tipoEvaluacion,
                periodoEvaluacions = _periodo,
                asignaturas = _asignatura,
                modalidades = _modalidad,
                grados = _grados

            };
            if(VieModelNotas==null)
            {
                TempData["Message"] = "No hay notas registradas";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            { TempData["Message"] = "Notas encontradas";
                return View(VieModelNotas);
            }
          
        }
        // GET: NotasController/Details/ para Los tutores
        [Authorize]
        public async Task<ActionResult> DetailsNotas(string cedulatutor)
        {
            
              //_context.TblNotas.FirstOrDefault(n => n.Id == id);
            //var asignatura = (await _Iservices.GetAsignaturaAsync())
            // .FirstOrDefault(a => a.IdAsignatura == nota.IdAsignatura);
            if(string.IsNullOrEmpty(cedulatutor))
            {
                TempData["Mensaje"] = "El nombre del Usuario no existe";
                TempData["Tipo"] = "warning";
                return View("NotFound");

            }

            var notasAlumnoTutor = await _Iservices.GetNotasPorUsuario(cedulatutor);
            var v_alumNota = await _Iservices.V_alumnoNotas(cedulatutor);
            var _modalidad = await _Iservices.GetModalidadesAsync();
            var _nivel = await _Iservices.GetGradosAsync();
            if (v_alumNota == null)
            {
                TempData["Mensaje"] = "No existen datos del alumno";
                TempData["Tipo"] = "warning";
                return NoContent();

            }
            List<TblNotas> listnota = await _Iservices.GetNotasAlumnoById(v_alumNota.IdAlumno);
            var periodosCatDet = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoLectivoActual = ObtenerPeriodoLectivoActual(periodosCatDet);
            var viewModel = new NotasViewModel
            {
                listNotas = listnota,
                alumnoNotas = v_alumNota,
                PeriodoLectivoActualTexto = periodoLectivoActual?.Periodo.ToString(),

                asignaturaSelectListItem = (await _Iservices.GetAsignaturaAsync())
                                            .Select(r => new SelectListItem
                                            {
                                                Value = r.IdAsignatura.ToString(),
                                                Text = r.NombreAsignatura
                                            }).ToList(),
                periodoSelectListItem = periodosCatDet
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdPeriodo.ToString(),
                                      Text = r.Periodo.ToString(),
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                sexoSelectListItem = (await _Iservices.GetSexosAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdSexo.ToString(),
                                      Text = r.Sexo,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdModalidad.ToString(),
                                      Text = r.Modalidad,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                gradosSelectListItem = (await _Iservices.GetGradosAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdGrado.ToString(),
                                      Text = r.NombreGrado,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                
            };

            if (notasAlumnoTutor == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        // GET: NotasController/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int id)
        {
            var v_alumNota = await _Iservices.GetAlumnoIdAsync(id);
            List<TblNotas> listnota = await _Iservices.GetNotasAlumnoById(id);   //_context.TblNotas.FirstOrDefault(n => n.Id == id);
            //var v_alumNota = await _Iservices.V_alumnoNotas(cedulatutor);
            var _modalidad = await _Iservices.GetModalidadesAsync();
            var _nivel = await _Iservices.GetGradosAsync();

            if (listnota == null)
            {
                return NotFound();
            }
            var periodosCatDet = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoLectivoActual = ObtenerPeriodoLectivoActual(periodosCatDet);
            var viewModel = new NotasViewModel
            {
                listNotas = listnota,
                alumnoNotas=v_alumNota,
                PeriodoLectivoActualTexto = periodoLectivoActual?.Periodo.ToString(),

                asignaturaSelectListItem = (await _Iservices.GetAsignaturaAsync())
                                            .Select(r => new SelectListItem
                                            {
                                                Value = r.IdAsignatura.ToString(),
                                                Text = r.NombreAsignatura
                                            }).ToList(),
                periodoSelectListItem = periodosCatDet
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdPeriodo.ToString(),
                                      Text = r.Periodo.ToString(),
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                sexoSelectListItem = (await _Iservices.GetSexosAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdSexo.ToString(),
                                      Text = r.Sexo,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdModalidad.ToString(),
                                      Text = r.Modalidad,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                gradosSelectListItem = (await _Iservices.GetGradosAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdGrado.ToString(),
                                      Text = r.NombreGrado,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),

            };
            if (viewModel == null)
            {
                TempData["Mensaje"] = "No existe Notas";
                TempData["Tipo"] = "warning";
                return NotFound();
            }

            return View(viewModel);
        }

        // GET: NotasController/Create
        [Authorize]
        public async Task<ActionResult> Create()
        {
            var periodosEval = await _Iservices.GetPeriodoEvaluacionAsync() ?? new List<PeriodoEvaluacion>();
            var idPeriodoEvalPreferido = ResolverIdPeriodoEvaluacionPreferido(periodosEval) ?? 0;

            var viewmodel = new NotasViewModel
            {
                notas = new TblNotas { IdPeriodo = idPeriodoEvalPreferido },
                NotasCualitativasSelectListItem = EscalaCualitativa.Opciones
                    .Select(o => new SelectListItem { Value = o.Codigo, Text = $"{o.Codigo} - {o.Descripcion} ({o.Rango})" })
                    .ToList(),
                tipoEvaluacionesSelectListItem = (await _Iservices.GetTipEvaluacionAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdTipoEvaluacion.ToString(),
                                      Text = r.NombreTipEvaluacion,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                periodoSelectListItem = (await _Iservices.GetPeriodoAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdPeriodo.ToString(),
                                      Text = r.Periodo.ToString(),
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                asignaturaSelectListItem = (await _Iservices.GetAsignaturaAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdAsignatura.ToString(),
                                      Text = r.NombreAsignatura,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),

                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdModalidad.ToString(),
                                      Text = r.Modalidad,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
              gradosSelectListItem = (await _Iservices.GetGradosAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdGrado.ToString(),
                                      Text = r.NombreGrado,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                recintosSelectListItem = (await _Iservices.GetRecintosAsync())
                    .Where(r => r.Activo)
                    .OrderBy(r => r.Recinto)
                    .Select(r => new SelectListItem
                    {
                        Value = r.IdRecinto.ToString(),
                        Text = r.Recinto
                    }).ToList(),
                periodoEvaluacionsSelectListItem = periodosEval
                    .Where(p => p.Activo)
                    .OrderBy(p => p.FechaInicio)
                    .Select(r => new SelectListItem
                    {
                        Value = r.IdPeriodo.ToString(),
                        Text = r.NombrePeriodo,
                        Selected = idPeriodoEvalPreferido > 0 && r.IdPeriodo == idPeriodoEvalPreferido
                    }).ToList(),

            };

            return View(viewmodel);
        }

        // POST: NotasController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TblNotas notas)
        {
            bool validarDuplicado= false;
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (notas.IdAlumno <= 0)
                {
                    TempData["Mensaje"] = "Debe buscar y seleccionar un alumno de la lista.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }

                if (notas.IdPeriodo <= 0)
                {
                    var periodosEval = await _Iservices.GetPeriodoEvaluacionAsync() ?? new List<PeriodoEvaluacion>();
                    var idPe = ResolverIdPeriodoEvaluacionPreferido(periodosEval);
                    if (idPe.HasValue) notas.IdPeriodo = idPe.Value;
                }

                if (notas.IdPeriodo <= 0)
                {
                    TempData["Mensaje"] = "No hay período de evaluación configurado. Configure períodos evaluativos o seleccione uno.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }

                validarDuplicado =await _Iservices.ValidarNotas(notas.IdAsignatura,notas.IdPeriodo,notas.IdAlumno);
                if (validarDuplicado == true)
                {
                    TempData["Mensaje"] = "El Alumno ya Posee un Registro de Nota con la Asignatura Seleccionada.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }

                if (notas != null)
                {
                    // Asegurar datos mínimos requeridos
                    notas.Activo = true;
                    notas.UsuarioRegistro = idUsuario;
                    notas.FechaRegistro = DateTime.Now;
                    if (!notas.IdColegio.HasValue || notas.IdColegio <= 0)
                    {
                        var alumnoFicha = await _Iservices.GetAlumnoIdAsync(notas.IdAlumno);
                        if (alumnoFicha?.IdRecinto is int idRec && idRec > 0)
                            notas.IdColegio = idRec;
                    }
                    var modalidad = (await _Iservices.GetModalidadesAsync())
                        .FirstOrDefault(m => m.IdModalidad == notas.IdModalidad)?.Modalidad;
                    var nombreGrado = (await _Iservices.GetGradosAsync())
                        .FirstOrDefault(g => g.IdGrado == notas.IdGrado)?.NombreGrado;
                    var esSoloCualitativo = EsCualitativoSolo(modalidad, nombreGrado);

                    if (esSoloCualitativo)
                    {
                        notas.Acumulado1 = null;
                        notas.Examen1 = null;
                        notas.PrimerCorteCuantitativo = null;
                        notas.Acumulado2 = null;
                        notas.Examen2 = null;
                        notas.SegundoCorteCuantitativo = null;
                        notas.Acumulado3 = null;
                        notas.Examen3 = null;
                        notas.TercerCorteCuantitativo = null;
                        notas.Acumulado4 = null;
                        notas.Examen4 = null;
                        notas.CuartoCorteCuantitativo = null;
                        notas.NotaFinalCuantitativo = null;
                    }
                    else
                    {
                        (notas.PrimerCorteCualitativo, notas.PrimerCorteCuantitativo) = CompletarParCualiCuanti(notas.PrimerCorteCualitativo, notas.PrimerCorteCuantitativo);
                        (notas.SegundoCorteCualitativo, notas.SegundoCorteCuantitativo) = CompletarParCualiCuanti(notas.SegundoCorteCualitativo, notas.SegundoCorteCuantitativo);
                        (notas.TercerCorteCualitativo, notas.TercerCorteCuantitativo) = CompletarParCualiCuanti(notas.TercerCorteCualitativo, notas.TercerCorteCuantitativo);
                        (notas.CuartoCorteCualitativo, notas.CuartoCorteCuantitativo) = CompletarParCualiCuanti(notas.CuartoCorteCualitativo, notas.CuartoCorteCuantitativo);
                        (notas.NotaFinalCualitativo, notas.NotaFinalCuantitativo) = CompletarParCualiCuanti(notas.NotaFinalCualitativo, notas.NotaFinalCuantitativo);
                    }

                    var (exito, detalleApi) = await _Iservices.PostNotasAsync(notas);
                    if (exito)
                    {
                        TempData["Mensaje"] = "Se agregó la nota del alumno correctamente.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction(nameof(Create));
                    }

                    var msgError = string.IsNullOrWhiteSpace(detalleApi)
                        ? "No se pudo guardar la nota. Revise la consola de la API o la conexión."
                        : (detalleApi.Length > 800 ? detalleApi[..800] + "…" : detalleApi);
                    TempData["Mensaje"] = "Error al guardar en la API: " + msgError;
                    TempData["Tipo"] = "error";
                    return RedirectToAction(nameof(Create));
                }

                TempData["Mensaje"] = "Los datos enviados no son válidos.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error inesperado al guardar: " + ex.Message;
                TempData["Tipo"] = "error";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: NotasController/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            //var v_alumNota = await _Iservices.V_alumnoNotas(id);
            var notas = await _Iservices.GetNotasById(id);
            if (id == 0)
            {
                return NotFound(); // si no existe
            }

            var viewmodel = new NotasViewModel
            {
                notas = notas,
                NotasCualitativasSelectListItem = EscalaCualitativa.Opciones
                    .Select(o => new SelectListItem { Value = o.Codigo, Text = $"{o.Codigo} - {o.Descripcion} ({o.Rango})" })
                    .ToList(),
                alumnosSelectListItem = (await _Iservices.GetAlumnosAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdAlumno.ToString(),
                                      Text = r.Nombre+" "+r.Apellido ,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdModalidad.ToString(),
                                      Text = r.Modalidad,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
               gradosSelectListItem = (await _Iservices.GetGradosAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdGrado.ToString(),
                                      Text = r.NombreGrado,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),
                recintosSelectListItem = (await _Iservices.GetRecintosAsync())
                    .Where(r => r.Activo)
                    .OrderBy(r => r.Recinto)
                    .Select(r => new SelectListItem
                    {
                        Value = r.IdRecinto.ToString(),
                        Text = r.Recinto
                    }).ToList(),
                asignaturaSelectListItem = (await _Iservices.GetAsignaturaAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdAsignatura.ToString(),
                                      Text = r.NombreAsignatura,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList(),

            };
            return View(viewmodel);
        }

        // POST: NotasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(NotasViewModel viewModel)
        {

            try
            {
                if (viewModel == null || viewModel.notas == null)
                {
                    ModelState.AddModelError("", "Los datos de las notas del alumno son inválidos.");
                    return View(viewModel);
                }

                var n = viewModel.notas;
                if (n.NotaFinalCuantitativo.HasValue && string.IsNullOrEmpty(n.NotaFinalCualitativo))
                    n.NotaFinalCualitativo = EscalaCualitativa.NumeroACualitativo(n.NotaFinalCuantitativo);
                if (n.PrimerCorteCuantitativo.HasValue && string.IsNullOrEmpty(n.PrimerCorteCualitativo))
                    n.PrimerCorteCualitativo = EscalaCualitativa.NumeroACualitativo(n.PrimerCorteCuantitativo);
                if (n.SegundoCorteCuantitativo.HasValue && string.IsNullOrEmpty(n.SegundoCorteCualitativo))
                    n.SegundoCorteCualitativo = EscalaCualitativa.NumeroACualitativo(n.SegundoCorteCuantitativo);
                if (n.TercerCorteCuantitativo.HasValue && string.IsNullOrEmpty(n.TercerCorteCualitativo))
                    n.TercerCorteCualitativo = EscalaCualitativa.NumeroACualitativo(n.TercerCorteCuantitativo);
                if (n.CuartoCorteCuantitativo.HasValue && string.IsNullOrEmpty(n.CuartoCorteCualitativo))
                    n.CuartoCorteCualitativo = EscalaCualitativa.NumeroACualitativo(n.CuartoCorteCuantitativo);

                if (!ModelState.IsValid)
                {
                    if ((!n.IdColegio.HasValue || n.IdColegio <= 0) && n.IdAlumno > 0)
                    {
                        var alumnoEd = await _Iservices.GetAlumnoIdAsync(n.IdAlumno);
                        if (alumnoEd?.IdRecinto is int idRecEd && idRecEd > 0)
                            n.IdColegio = idRecEd;
                    }

                    var actualizado = await _Iservices.UpdateNotas(viewModel.notas);

                    if (actualizado)
                    {
                        TempData["Mensaje"] = "Nota actualizada correctamente.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "Error al actualizar la nota del alumno."); // si hay errores, devuelve a la vista con los datos
                }

                viewModel.tipoEvaluacionesSelectListItem = (await _Iservices.GetTipEvaluacionAsync())
                                 .Select(r => new SelectListItem
                                 {
                                     Value = r.IdTipoEvaluacion.ToString(),
                                     Text = r.NombreTipEvaluacion,
                                     //Selected = r.IdPregunta == respuestas.IdPregunta
                                 }).ToList();
                     viewModel.periodoEvaluacionsSelectListItem = (await _Iservices.GetPeriodoEvaluacionAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdPeriodo.ToString(),
                                      Text = r.NombrePeriodo,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList();
                viewModel.asignaturaSelectListItem = (await _Iservices.GetAsignaturaAsync())
                                  .Select(r => new SelectListItem
                                  {
                                      Value = r.IdAsignatura.ToString(),
                                      Text = r.NombreAsignatura,
                                      //Selected = r.IdPregunta == respuestas.IdPregunta
                                  }).ToList();
                viewModel.recintosSelectListItem = (await _Iservices.GetRecintosAsync())
                    .Where(r => r.Activo)
                    .OrderBy(r => r.Recinto)
                    .Select(r => new SelectListItem
                    {
                        Value = r.IdRecinto.ToString(),
                        Text = r.Recinto
                    }).ToList();

               
                return View(viewModel);



            }
            catch
            {
                return View();
            }
        }

        // GET: NotasController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: NotasController/Delete/5
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
