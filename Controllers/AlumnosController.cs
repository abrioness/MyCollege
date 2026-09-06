using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Helpers;
using WebColegio.Models.ViewModel;
using WebColegio.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebColegio.Controllers
{
    public class AlumnosController : Controller
    {
        private readonly IServicesApi _Iservices;
        public AlumnosController(IServicesApi services)
        {
            _Iservices = services;
        }

            private async Task<bool> DebeGuardarRecibioPreescolar(TblAlumno alumno)
            {
                if (alumno == null) return false;

                var modalidad = (await _Iservices.GetModalidadesAsync())
                    .FirstOrDefault(m => m.IdModalidad == alumno.IdModalidad)?.Modalidad ?? string.Empty;

                var grado = (await _Iservices.GetGradosAsync())
                    .FirstOrDefault(g => g.IdGrado == alumno.IdGrado)?.NombreGrado ?? string.Empty;

                var modalidadTexto = modalidad.ToLowerInvariant();
                var gradoTexto = grado.ToLowerInvariant();

                var esPrimariaRegular = modalidadTexto.Contains("primaria regular");
                var esPrimerGrado = gradoTexto == "primero"
                    || gradoTexto.Contains("primer grado")
                    || gradoTexto.Contains("1er grado")
                    || gradoTexto.Contains("1° grado");

                return esPrimariaRegular && esPrimerGrado;
            }
        // GET: AlumnosController
        [Authorize]
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {

                var _alumnos = await _Iservices.GetAlumnosAsync() ?? new List<TblAlumno>();
             _alumnos = _alumnos
            .OrderByDescending(r => r.IdAlumno)
            .ToList();
            var _sexos = await _Iservices.GetSexosAsync();
                var _grupos = await _Iservices.GetGruposAsync();
                var _grados = await _Iservices.GetGradosAsync();
                var _turnos = await _Iservices.GetTurnosAsync();
                var _modalidades = await _Iservices.GetModalidadesAsync();
                var _recintos = await _Iservices.GetRecintosAsync();
            var _discapacidad = await _Iservices.GetDiscapacidadAsync();

            IQueryable<TblAlumno> query = _alumnos.AsQueryable();

            var (ini, fin) = ReporteFechaQuery.ResolverRango(Request, fechainicio, fechafin);
            if (ini.HasValue)
                query = query.Where(a => a.FechaRegistro.Date >= ini.Value.Date);
            if (fin.HasValue)
                query = query.Where(a => a.FechaRegistro.Date <= fin.Value.Date);

            var alumnosFiltrados = query
                .OrderByDescending(a => a.FechaRegistro)
                .ThenByDescending(a => a.IdAlumno)
                .ToList();

            var VieModelAlumnos = new ColeccionCatalogos
                {
                    alumno = alumnosFiltrados,
                    sexos = _sexos,
                    grupos = _grupos,
                    grados = _grados,
                    turnos = _turnos,
                    modalidades = _modalidades,
                    recintos = _recintos,
                    discapacidad=_discapacidad,

                    
                };

                return View(VieModelAlumnos);
        }

        /// <summary>Buscar alumnos por nombre/apellido para autocompletado. Devuelve JSON con id y nombreCompleto.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new List<object>());
            var alumnos = await _Iservices.GetAlumnosAsync() ?? new List<TblAlumno>();
            var term = q.Trim().ToUpperInvariant();
            var lista = alumnos
                .Where(a => a.Activo == true && (
                    (a.Nombre != null && a.Nombre.ToUpperInvariant().Contains(term)) ||
                    (a.Apellido != null && a.Apellido.ToUpperInvariant().Contains(term)) ||
                    ((a.Nombre + " " + a.Apellido).ToUpperInvariant().Contains(term))))
                .Take(20)
                .Select(a => new { id = a.IdAlumno, nombreCompleto = (a.Nombre ?? "") + " " + (a.Apellido ?? "") })
                .ToList();
            return Json(lista);
        }

        // GET: AlumnosController/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int id)
        {

            var _alumnos = await _Iservices.GetAlumnoIdAsync(id);

            var viewModel = new AlumnosViewModel
            {

                alumnos = _alumnos,
                periodoSelectListItem = (await _Iservices.GetPeriodoAsync())
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
                recintosSelectListItem = (await _Iservices.GetRecintosAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto.ToString(),
                }
                ).ToList(),
                modalidadesSelectListItem = (await _Iservices.GetModalidadesAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdModalidad.ToString(),
                    Text = r.Modalidad.ToString(),
                }
                ).ToList(),
                turnosSelectListItem = (await _Iservices.GetTurnosAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdTurno.ToString(),
                    Text = r.NombreTurno.ToString(),
                }
                ).ToList(),
                sexosSelectListItem = (await _Iservices.GetSexosAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdSexo.ToString(),
                    Text = r.Sexo.ToString(),
                }
                ).ToList(),
                discapacidadSelectListItem = (await _Iservices.GetDiscapacidadAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.Id_Discapacidad.ToString(),
                    Text = r.Discapacidad.ToString(),
                }
                ).ToList(),



            };

            if (_alumnos == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

       

        // GET: AlumnosController/Create
        public async Task<ActionResult> Create()
        {

            var viewmodel = new AlumnosViewModel
            {
                codigoestudiante = await _Iservices.GenerarCodigoAlumno(),
                sexosSelectListItem = (await _Iservices.GetSexosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdSexo.ToString(),
                                  Text = r.Sexo,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
                gruposSelectListItem = (await _Iservices.GetGruposAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdGrupo.ToString(),
                                  Text = r.NombreGrupo,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
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
                modalidadesSelectListItem = (await _Iservices.GetModalidadesAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdModalidad.ToString(),
                                  Text = r.Modalidad,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),

                recintosSelectListItem = (await _Iservices.GetRecintosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdRecinto.ToString(),
                                  Text = r.Recinto,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
                discapacidadSelectListItem = (await _Iservices.GetDiscapacidadAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.Id_Discapacidad.ToString(),
                                  Text = r.Discapacidad,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList()
            };

            return View(viewmodel);
        }

        // POST: AlumnosController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TblAlumno alumnos)
        {
            try
            {
                if (alumnos == null)
                {
                    TempData["Mensaje"] = "No se recibieron datos del formulario.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }

                var periodos = await _Iservices.GetPeriodoAsync() ?? new List<CatPeriodo>();
                alumnos.IdPeriodo = periodos.FirstOrDefault(a => a.Activo && a.Actual)?.IdPeriodo ?? 0;

                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var mined = alumnos.CodigoMINED?.Trim();
                if (string.IsNullOrWhiteSpace(mined))
                {
                    TempData["Mensaje"] = "El código MINED es obligatorio.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }

                var existe = await _Iservices.ValidarAlumnoDuplicado(
                    mined,
                    alumnos.CodigoAlumno?.Trim());
                if (existe)
                {
                    TempData["Mensaje"] = "Ya existe un alumno con el mismo código MINED o código de estudiante.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }

                if (!await DebeGuardarRecibioPreescolar(alumnos))
                    alumnos.RecibioEducacionPreescolar = null;

                alumnos.UsuarioRegistro = idUsuario;
                alumnos.FechaRegistro = DateTime.Now;

                var (exito, detalleError, idDesdeApi) = await _Iservices.PostAlumnosAsync(alumnos);

                if (!exito)
                {
                    TempData["Mensaje"] = string.IsNullOrWhiteSpace(detalleError)
                        ? "No se pudo guardar el registro. Verifique la conexión con la API o los datos enviados."
                        : detalleError;
                    TempData["Tipo"] = "error";
                    return RedirectToAction("Create");
                }

                var idNuevo = idDesdeApi;
                if (!idNuevo.HasValue || idNuevo.Value <= 0)
                {
                    var lista = await _Iservices.GetAlumnosAsync() ?? new List<TblAlumno>();
                    var codigo = alumnos.CodigoAlumno?.Trim();
                    var encontrado = lista
                        .OrderByDescending(a => a.IdAlumno)
                        .FirstOrDefault(a =>
                            (!string.IsNullOrEmpty(codigo) &&
                             string.Equals(a.CodigoAlumno?.Trim(), codigo, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(mined) &&
                             string.Equals(a.CodigoMINED?.Trim(), mined, StringComparison.OrdinalIgnoreCase)));
                    idNuevo = encontrado?.IdAlumno;
                }

                TempData["Mensaje"] = "Se registró correctamente al estudiante.";
                TempData["Tipo"] = "success";

                if (idNuevo.HasValue && idNuevo.Value > 0)
                    return RedirectToAction("Details", "Alumnos", new { id = idNuevo.Value });

                TempData["Mensaje"] = "El registro se guardó, pero no se pudo localizar el id del alumno para mostrar el detalle. Revise el listado de alumnos.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al registrar: {ex.Message}";
                TempData["Tipo"] = "error";
                return RedirectToAction("Create");
            }
        }

        // GET: AlumnosController/Edit/5

        public async Task<ActionResult> Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            var alumnos = await _Iservices.GetAlumnoIdAsync(id);
            if (alumnos == null || alumnos.IdAlumno <= 0)
                return NotFound();

            var viewmodel = new AlumnosViewModel
            {
                alumnos = alumnos,
                ListGrados = await _Iservices.GetGradosAsync(),
                sexosSelectListItem = (await _Iservices.GetSexosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdSexo.ToString(),
                                  Text = r.Sexo,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
                gruposSelectListItem = (await _Iservices.GetGruposAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdGrupo.ToString(),
                                  Text = r.NombreGrupo,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
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
                modalidadesSelectListItem = (await _Iservices.GetModalidadesAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdModalidad.ToString(),
                                  Text = r.Modalidad,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),

                recintosSelectListItem = (await _Iservices.GetRecintosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdRecinto.ToString(),
                                  Text = r.Recinto,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
                discapacidadSelectListItem = (await _Iservices.GetDiscapacidadAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.Id_Discapacidad.ToString(),
                                  Text = r.Discapacidad,
                              }).ToList()
            }; 

            return View(viewmodel);
        }

        // POST: AlumnosController/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AlumnosViewModel viewModel)
        {

            try {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (viewModel == null || viewModel.alumnos == null)
                {
                    ModelState.AddModelError("", "Los datos del alumno son inválidos.");
                    return View(viewModel);
                }

                // No exigir ModelState.IsValid: el formulario tiene muchos opcionales y selects;
                // un fallo de enlace silencioso impedía cualquier actualización.
                if (viewModel.alumnos.IdAlumno <= 0)
                {
                    ModelState.AddModelError("", "Identificador de alumno no válido.");
                }
                else
                {
                    var duplicado = false;
                    var mined = viewModel.alumnos.CodigoMINED?.Trim();
                    if (!string.IsNullOrWhiteSpace(mined))
                    {
                        duplicado = await _Iservices.ValidarAlumnoDuplicado(
                            mined,
                            viewModel.alumnos.CodigoAlumno?.Trim(),
                            excluirIdAlumno: viewModel.alumnos.IdAlumno);
                        if (duplicado)
                            ModelState.AddModelError("", "Ya existe otro alumno con el mismo código MINED o código de estudiante.");
                    }

                    if (!duplicado)
                    {
                    if (!await DebeGuardarRecibioPreescolar(viewModel.alumnos))
                        viewModel.alumnos.RecibioEducacionPreescolar = null;

                    viewModel.alumnos.UsuarioActualiza = idUsuario;
                    viewModel.alumnos.FechaActualiza = DateTime.Now;
                    var actualizado = await _Iservices.UpdateAlumnos(viewModel.alumnos);

                    if (actualizado)
                    {
                        TempData["Mensaje"] = "Los datos del estudiante se actualizaron correctamente.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "Error al actualizar los datos del alumno en el servidor. Compruebe la API o los datos enviados.");
                    }
                }

            viewModel.ListGrados = await _Iservices.GetGradosAsync();
            viewModel.sexosSelectListItem = (await _Iservices.GetSexosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdSexo.ToString(),
                                  Text = r.Sexo,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList();
            viewModel.gruposSelectListItem = (await _Iservices.GetGruposAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdGrupo.ToString(),
                                  Text = r.NombreGrupo,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList();
                viewModel.gradosSelectListItem = (await _Iservices.GetGradosAsync())
                             .Select(r => new SelectListItem
                             {
                                 Value = r.IdGrado.ToString(),
                                 Text = r.NombreGrado,
                                 //Selected = r.IdPregunta == respuestas.IdPregunta
                             }).ToList();
                viewModel.turnosSelectListItem = (await _Iservices.GetTurnosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdTurno.ToString(),
                                  Text = r.NombreTurno,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList();
                 viewModel.modalidadesSelectListItem = (await _Iservices.GetModalidadesAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdModalidad.ToString(),
                                  Text = r.Modalidad,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList();

                 viewModel.recintosSelectListItem = (await _Iservices.GetRecintosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdRecinto.ToString(),
                                  Text = r.Recinto,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList();
                viewModel.discapacidadSelectListItem = (await _Iservices.GetDiscapacidadAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.Id_Discapacidad.ToString(),
                                  Text = r.Discapacidad,
                              }).ToList();
                return View(viewModel);
            }
            catch
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin,UserSystem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Anular(int id, DateTime? fechainicio, DateTime? fechafin)
        {
            TblAlumno? alumno;
            try
            {
                alumno = await _Iservices.GetAlumnoIdAsync(id);
            }
            catch
            {
                TempData["Mensaje"] = "Estudiante no encontrado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            if (alumno == null || alumno.IdAlumno <= 0)
            {
                TempData["Mensaje"] = "Estudiante no encontrado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            if (alumno.Activo == false)
            {
                TempData["Mensaje"] = $"{alumno.Nombre} {alumno.Apellido}".Trim() + " ya está anulado.";
                TempData["Tipo"] = "info";
                return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
            }

            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            alumno.Activo = false;
            alumno.UsuarioActualiza = idUsuario;
            alumno.FechaActualiza = DateTime.Now;

            if (await _Iservices.UpdateAlumnos(alumno))
            {
                TempData["Mensaje"] = $"Se anuló el registro de {alumno.Nombre} {alumno.Apellido}.".Trim();
                TempData["Tipo"] = "success";
            }
            else
            {
                TempData["Mensaje"] = "No se pudo anular el estudiante. Revise la conexión con la API.";
                TempData["Tipo"] = "warning";
            }

            return RedirectToAction(nameof(Index), new { fechainicio, fechafin });
        }

        // GET: AlumnosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AlumnosController/Delete/5
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
