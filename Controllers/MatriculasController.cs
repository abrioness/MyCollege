using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Helpers;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private static readonly string[] Estados =
        {
            TblMatricula.EstadoReserva,
            TblMatricula.EstadoInscrito,
            TblMatricula.EstadoActivo,
            TblMatricula.EstadoRetirado
        };

        private readonly IServicesApi _services;

        public MatriculasController(IServicesApi services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index(int? idPeriodo, string? estado, string? q)
        {
            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoSugerido = CicloLectivoHelper.ResolverPeriodoMatricula(periodos, DateTime.Now)
                                  ?? CicloLectivoHelper.ResolverPeriodoActual(periodos);

            // idPeriodo = 0 → todos los ciclos. Sin parámetro → ciclo sugerido (no es el listado de Tbl_Alumnos).
            bool verTodos = idPeriodo == 0;
            int idFiltro = verTodos ? 0 : (idPeriodo ?? periodoSugerido?.IdPeriodo ?? 0);

            var matriculas = await _services.GetMatriculasAsync(idFiltro > 0 ? idFiltro : null)
                ?? new List<TblMatricula>();
            var errorApi = _services.LastApiError;
            if (!string.IsNullOrWhiteSpace(estado))
                matriculas = matriculas.Where(m => string.Equals(m.Estado, estado, StringComparison.OrdinalIgnoreCase)).ToList();

            var alumnosLista = await _services.GetAlumnosAsync() ?? new List<TblAlumno>();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                var ids = alumnosLista
                    .Where(a =>
                        (!string.IsNullOrWhiteSpace(a.Nombre) && a.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase))
                        || (!string.IsNullOrWhiteSpace(a.Apellido) && a.Apellido.Contains(term, StringComparison.OrdinalIgnoreCase))
                        || ($"{a.Nombre} {a.Apellido}").Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.IdAlumno)
                    .ToHashSet();
                matriculas = matriculas.Where(m => ids.Contains(m.IdAlumno)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(errorApi))
            {
                TempData["Mensaje"] = errorApi.Contains("401", StringComparison.OrdinalIgnoreCase)
                    ? "La API rechazó la sesión (401). Cierre sesión e ingrese de nuevo."
                    : "No se pudieron leer las matrículas desde la API. " + errorApi;
                TempData["Tipo"] = "warning";
            }

            var vm = new MatriculaListaViewModel
            {
                Matriculas = matriculas,
                Alumnos = alumnosLista,
                Periodos = periodos.OrderByDescending(p => p.Periodo).ToList(),
                Grados = await _services.GetGradosAsync() ?? new List<Grados>(),
                Modalidades = await _services.GetModalidadesAsync() ?? new List<Modalidades>(),
                Recintos = await _services.GetRecintosAsync() ?? new List<Recintos>(),
                Turnos = await _services.GetTurnosAsync() ?? new List<Turnos>(),
                Grupos = await _services.GetGruposAsync() ?? new List<Grupos>(),
                IdPeriodoFiltro = idFiltro > 0 ? idFiltro : null,
                EstadoFiltro = estado,
                TextoBusqueda = q,
                PeriodoMatriculaSugerido = periodoSugerido,
                ErrorApi = errorApi
            };
            return View(vm);
        }

        public async Task<IActionResult> Create(int? idAlumno, int? idPeriodo)
        {
            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var periodoDestino = idPeriodo.HasValue
                ? periodos.FirstOrDefault(p => p.IdPeriodo == idPeriodo.Value)
                : CicloLectivoHelper.ResolverPeriodoMatricula(periodos, DateTime.Now)
                  ?? CicloLectivoHelper.ResolverPeriodoActual(periodos);

            var matricula = new TblMatricula
            {
                IdPeriodo = periodoDestino?.IdPeriodo ?? 0,
                Estado = CicloLectivoHelper.CorrespondeMatriculaSiguienteCiclo(DateTime.Now, periodos)
                    ? TblMatricula.EstadoReserva
                    : TblMatricula.EstadoInscrito,
                FechaMatricula = DateTime.Today,
                Activo = true
            };

            string nombre = string.Empty;
            if (idAlumno.HasValue && idAlumno.Value > 0)
            {
                var existente = await _services.GetMatriculaAlumnoPeriodoAsync(idAlumno.Value, matricula.IdPeriodo);
                if (existente != null)
                    return RedirectToAction(nameof(Edit), new { id = existente.IdMatricula });

                var alumno = await _services.GetAlumnoIdAsync(idAlumno.Value);
                if (alumno != null && alumno.IdAlumno > 0)
                {
                    matricula.IdAlumno = alumno.IdAlumno;
                    matricula.IdGrado = alumno.IdGrado ?? 0;
                    matricula.IdModalidad = alumno.IdModalidad ?? 0;
                    matricula.IdRecinto = alumno.IdRecinto ?? 0;
                    matricula.IdTurno = alumno.IdTurno;
                    matricula.IdGrupo = alumno.IdGrupo;
                    matricula.BecaCompleta = alumno.BecaCompleta == true;
                    matricula.MediaBeca = alumno.MediaBeca == true;
                    matricula.Repitente = alumno.Repitente;
                    matricula.TipoEstudiante = alumno.TipoEstudiante;
                    nombre = $"{alumno.Nombre} {alumno.Apellido}".Trim();
                }
            }

            return View("Form", await ConstruirFormAsync(matricula, nombre, false));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MatriculaFormViewModel form)
        {
            var m = form?.Matricula ?? new TblMatricula();
            if (!Validar(m, out var error))
            {
                SetMensaje(error, "warning");
                return View("Form", await ConstruirFormAsync(m, form?.NombreAlumno, false));
            }

            var ya = await _services.GetMatriculaAlumnoPeriodoAsync(m.IdAlumno, m.IdPeriodo);
            if (ya != null)
            {
                SetMensaje("Ese alumno ya tiene matrícula en este ciclo. Se abrió el registro existente.", "info");
                return RedirectToAction(nameof(Edit), new { id = ya.IdMatricula });
            }

            m.UsuarioRegistro = IdUsuarioActual();
            m.FechaRegistro = DateTime.Now;
            m.Activo = true;
            if (m.FechaMatricula == null)
                m.FechaMatricula = DateTime.Today;

            var (ok, err) = await _services.PostMatriculaAsync(m);
            if (!ok)
            {
                SetMensaje("No se pudo guardar la matrícula. " + (err ?? _services.LastApiError), "warning");
                return View("Form", await ConstruirFormAsync(m, form?.NombreAlumno, false));
            }

            await SincronizarFichaAlumnoSiCorresponde(m);
            SetMensaje("Matrícula académica registrada. No se creó otro alumno.", "success");
            return RedirectToAction(nameof(Index), new { idPeriodo = m.IdPeriodo });
        }

        public async Task<IActionResult> Imprimir(int id)
        {
            var matricula = await _services.GetMatriculaByIdAsync(id);
            if (matricula == null)
            {
                SetMensaje("No se encontró la matrícula del ciclo.", "warning");
                return RedirectToAction(nameof(Index));
            }

            var alumno = await _services.GetAlumnoIdAsync(matricula.IdAlumno);
            if (alumno == null || alumno.IdAlumno <= 0)
            {
                SetMensaje("No se encontró el alumno de esta matrícula.", "warning");
                return RedirectToAction(nameof(Index));
            }

            alumno.IdPeriodo = matricula.IdPeriodo;
            alumno.IdGrado = matricula.IdGrado;
            alumno.IdModalidad = matricula.IdModalidad;
            alumno.IdRecinto = matricula.IdRecinto;
            alumno.IdTurno = matricula.IdTurno;
            alumno.IdGrupo = matricula.IdGrupo;
            alumno.BecaCompleta = matricula.BecaCompleta;
            alumno.MediaBeca = matricula.MediaBeca;
            if (!string.IsNullOrWhiteSpace(matricula.Repitente))
                alumno.Repitente = matricula.Repitente;
            if (!string.IsNullOrWhiteSpace(matricula.TipoEstudiante))
                alumno.TipoEstudiante = matricula.TipoEstudiante;

            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            int? anioCiclo = periodos.FirstOrDefault(p => p.IdPeriodo == matricula.IdPeriodo)?.Periodo;

            var viewModel = new AlumnosViewModel
            {
                alumnos = alumno,
                FechaHojaMatricula = matricula.FechaMatricula ?? matricula.FechaRegistro,
                AnioHojaMatricula = anioCiclo,
                periodoSelectListItem = periodos
                    .Select(r => new SelectListItem { Value = r.IdPeriodo.ToString(), Text = r.Periodo.ToString() })
                    .ToList(),
                gradosSelectListItem = (await _services.GetGradosAsync() ?? new List<Grados>())
                    .Select(r => new SelectListItem { Value = r.IdGrado.ToString(), Text = r.NombreGrado })
                    .ToList(),
                recintosSelectListItem = (await _services.GetRecintosAsync() ?? new List<Recintos>())
                    .Select(r => new SelectListItem { Value = r.IdRecinto.ToString(), Text = r.Recinto })
                    .ToList(),
                modalidadesSelectListItem = (await _services.GetModalidadesAsync() ?? new List<Modalidades>())
                    .Select(r => new SelectListItem { Value = r.IdModalidad.ToString(), Text = r.Modalidad })
                    .ToList(),
                turnosSelectListItem = (await _services.GetTurnosAsync() ?? new List<Turnos>())
                    .Select(r => new SelectListItem { Value = r.IdTurno.ToString(), Text = r.NombreTurno })
                    .ToList(),
                sexosSelectListItem = (await _services.GetSexosAsync() ?? new List<Sexos>())
                    .Select(r => new SelectListItem { Value = r.IdSexo.ToString(), Text = r.Sexo })
                    .ToList(),
                discapacidadSelectListItem = (await _services.GetDiscapacidadAsync() ?? new List<CatDiscapacidad>())
                    .Select(r => new SelectListItem { Value = r.Id_Discapacidad.ToString(), Text = r.Discapacidad })
                    .ToList()
            };

            return View("~/Views/Alumnos/Details.cshtml", viewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var m = await _services.GetMatriculaByIdAsync(id);
            if (m == null)
            {
                SetMensaje("No se encontró la matrícula.", "warning");
                return RedirectToAction(nameof(Index));
            }

            var alumno = await _services.GetAlumnoIdAsync(m.IdAlumno);
            var nombre = alumno != null ? $"{alumno.Nombre} {alumno.Apellido}".Trim() : "";
            return View("Form", await ConstruirFormAsync(m, nombre, true));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MatriculaFormViewModel form)
        {
            var m = form?.Matricula ?? new TblMatricula();
            if (m.IdMatricula <= 0)
            {
                SetMensaje("Registro no válido.", "warning");
                return View("Form", await ConstruirFormAsync(m, form?.NombreAlumno, true));
            }
            if (!Validar(m, out var errorEdit))
            {
                SetMensaje(errorEdit, "warning");
                return View("Form", await ConstruirFormAsync(m, form?.NombreAlumno, true));
            }

            var original = await _services.GetMatriculaByIdAsync(m.IdMatricula);
            if (original != null)
            {
                m.UsuarioRegistro = original.UsuarioRegistro;
                m.FechaRegistro = original.FechaRegistro;
            }
            m.UsuarioActualizo = IdUsuarioActual();
            m.FechaActualizo = DateTime.Now;

            var (ok, err) = await _services.UpdateMatriculaAsync(m);
            if (!ok)
            {
                SetMensaje("No se pudo actualizar. " + (err ?? _services.LastApiError), "warning");
                return View("Form", await ConstruirFormAsync(m, form?.NombreAlumno, true));
            }

            await SincronizarFichaAlumnoSiCorresponde(m);
            SetMensaje("Matrícula actualizada.", "success");
            return RedirectToAction(nameof(Index), new { idPeriodo = m.IdPeriodo });
        }

        private async Task SincronizarFichaAlumnoSiCorresponde(TblMatricula m)
        {
            var esColocacionVigente = string.Equals(m.Estado, TblMatricula.EstadoInscrito, StringComparison.OrdinalIgnoreCase)
                || string.Equals(m.Estado, TblMatricula.EstadoActivo, StringComparison.OrdinalIgnoreCase);
            if (!esColocacionVigente)
                return;

            var alumno = await _services.GetAlumnoIdAsync(m.IdAlumno);
            if (alumno == null || alumno.IdAlumno <= 0)
                return;

            alumno.IdPeriodo = m.IdPeriodo;
            alumno.IdGrado = m.IdGrado;
            alumno.IdModalidad = m.IdModalidad;
            alumno.IdRecinto = m.IdRecinto;
            alumno.IdTurno = m.IdTurno;
            alumno.IdGrupo = m.IdGrupo;
            alumno.BecaCompleta = m.BecaCompleta;
            alumno.MediaBeca = m.MediaBeca;
            alumno.Repitente = m.Repitente;
            alumno.TipoEstudiante = m.TipoEstudiante;
            alumno.UsuarioActualiza = IdUsuarioActual();
            alumno.FechaActualiza = DateTime.Now;
            await _services.UpdateAlumnos(alumno);
        }

        private async Task<MatriculaFormViewModel> ConstruirFormAsync(TblMatricula m, string? nombre, bool esEdicion)
        {
            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            return new MatriculaFormViewModel
            {
                Matricula = m,
                NombreAlumno = nombre ?? string.Empty,
                EsEdicion = esEdicion,
                Periodos = periodos.Where(p => p.Activo || p.IdPeriodo == m.IdPeriodo)
                    .OrderByDescending(p => p.Periodo)
                    .Select(p => new SelectListItem
                    {
                        Value = p.IdPeriodo.ToString(),
                        Text = p.Actual ? $"{p.Periodo} (actual)" : p.Periodo.ToString(),
                        Selected = p.IdPeriodo == m.IdPeriodo
                    }).ToList(),
                Grados = ToSelect((await _services.GetGradosAsync())?.Select(g => (g.IdGrado, g.NombreGrado, g.Activo)), m.IdGrado),
                Modalidades = ToSelect((await _services.GetModalidadesAsync())?.Select(x => (x.IdModalidad, x.Modalidad, x.Activo)), m.IdModalidad),
                Recintos = ToSelect((await _services.GetRecintosAsync())?.Select(x => (x.IdRecinto, x.Recinto, x.Activo)), m.IdRecinto),
                Turnos = ToSelectOpcional((await _services.GetTurnosAsync())?.Select(x => (x.IdTurno, x.NombreTurno, x.Activo)), m.IdTurno),
                Grupos = ToSelectOpcional((await _services.GetGruposAsync())?.Select(x => (x.IdGrupo, x.NombreGrupo, x.Activo)), m.IdGrupo),
                Estados = Estados.Select(e => new SelectListItem { Value = e, Text = e, Selected = e == m.Estado }).ToList()
            };
        }

        private static List<SelectListItem> ToSelect(IEnumerable<(int Id, string Texto, bool Activo)>? src, int seleccionado)
            => (src ?? Enumerable.Empty<(int, string, bool)>())
                .Where(x => x.Activo || x.Id == seleccionado)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Texto, Selected = x.Id == seleccionado })
                .ToList();

        private static List<SelectListItem> ToSelectOpcional(IEnumerable<(int Id, string Texto, bool Activo)>? src, int? seleccionado)
        {
            var items = new List<SelectListItem> { new() { Value = "", Text = "(Sin asignar)" } };
            items.AddRange(ToSelect(src, seleccionado ?? 0));
            return items;
        }

        private static bool Validar(TblMatricula m, out string error)
        {
            if (m.IdAlumno <= 0) { error = "Busque y seleccione un alumno ya registrado. Si es nuevo, créelo primero en Alumnos."; return false; }
            if (m.IdPeriodo <= 0) { error = "Debe seleccionar el ciclo."; return false; }
            if (m.IdGrado <= 0) { error = "Debe seleccionar el nivel."; return false; }
            if (m.IdModalidad <= 0) { error = "Debe seleccionar la modalidad."; return false; }
            if (m.IdRecinto <= 0) { error = "Debe seleccionar el colegio."; return false; }
            error = "";
            return true;
        }

        private int IdUsuarioActual()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : 0;
        }

        private void SetMensaje(string texto, string tipo)
        {
            TempData["Mensaje"] = texto;
            TempData["Tipo"] = tipo;
        }
    }
}
