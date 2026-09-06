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
    [Authorize(Roles = "Admin,UserSystem")]
    public class CostosController : Controller
    {
        private readonly IServicesApi _services;

        public CostosController(IServicesApi services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index(int? idPeriodo)
        {
            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var costosMat = await _services.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>();
            var costosMen = await _services.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>();
            var recintos = await _services.GetRecintosAsync() ?? new List<Recintos>();
            var modalidades = await _services.GetModalidadesAsync() ?? new List<Modalidades>();
            var grados = await _services.GetGradosAsync() ?? new List<Grados>();

            var periodoActual = CicloLectivoHelper.ResolverPeriodoActual(periodos);
            int anioActual = periodoActual?.Periodo ?? DateTime.Now.Year;
            int anioSiguiente = CicloLectivoHelper.AnioSiguienteCiclo(anioActual);
            bool ventanaOctubre = CicloLectivoHelper.EstaAbiertaMatriculaSiguienteCiclo(DateTime.Now);
            var periodoSiguiente = periodos.FirstOrDefault(p => p.Periodo == anioSiguiente);
            bool creadoAuto = false;
            string? avisoCiclo = null;

            if (ventanaOctubre && periodoSiguiente == null && periodoActual != null)
            {
                var (ok, err, _) = await CrearPeriodoSiguienteAsync(anioSiguiente, marcarActual: false);
                if (ok)
                {
                    periodos = await _services.GetPeriodoAsync() ?? periodos;
                    periodoSiguiente = periodos.FirstOrDefault(p => p.Periodo == anioSiguiente);
                    creadoAuto = periodoSiguiente != null;
                    avisoCiclo = creadoAuto
                        ? $"Desde octubre se preparó automáticamente el ciclo {anioSiguiente}. Revise y ajuste las tarifas; no se marcó como ciclo actual para no afectar las mensualidades en curso."
                        : "Ya es octubre, pero no se pudo confirmar el ciclo siguiente. Use Habilitar ciclo.";
                }
                else
                {
                    avisoCiclo = "Ya es octubre y el ciclo siguiente aún no existe. Habilítelo aquí para poder matricular. "
                        + (string.IsNullOrWhiteSpace(err) ? "" : err);
                }
            }

            int idFiltro = idPeriodo ?? periodoActual?.IdPeriodo ?? 0;
            if (idFiltro > 0)
            {
                costosMat = costosMat.Where(c => c.IdPeriodo == idFiltro).ToList();
                costosMen = costosMen.Where(c => c.IdPeriodo == idFiltro).ToList();
            }

            int idSiguiente = periodoSiguiente?.IdPeriodo ?? 0;
            var vm = new CostosAdminViewModel
            {
                CostosMatricula = costosMat.OrderBy(c => c.IdRecinto).ThenBy(c => c.IdModalidad).ToList(),
                CostosMensualidad = costosMen.OrderBy(c => c.IdRecinto).ThenBy(c => c.IdGrado).ThenBy(c => c.IdModalidad).ToList(),
                Periodos = periodos.OrderByDescending(p => p.Periodo).ToList(),
                Recintos = recintos,
                Modalidades = modalidades,
                Grados = grados,
                IdPeriodoFiltro = idFiltro > 0 ? idFiltro : null,
                PeriodoActual = periodoActual,
                PeriodoSiguiente = periodoSiguiente,
                AnioCicloSiguiente = anioSiguiente,
                VentanaMatriculaSiguienteCiclo = ventanaOctubre,
                PeriodoSiguienteExiste = periodoSiguiente != null,
                CicloSiguienteCreadoAutomaticamente = creadoAuto,
                CantidadCostosMatriculaSiguiente = idSiguiente > 0
                    ? (await _services.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>()).Count(c => c.IdPeriodo == idSiguiente)
                    : 0,
                CantidadCostosMensualidadSiguiente = idSiguiente > 0
                    ? (await _services.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>()).Count(c => c.IdPeriodo == idSiguiente)
                    : 0,
                AvisoCiclo = avisoCiclo
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HabilitarSiguienteCiclo(bool copiarTarifas = true)
        {
            try
            {
                var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
                var actual = CicloLectivoHelper.ResolverPeriodoActual(periodos);
                if (actual == null)
                {
                    SetMensaje("No hay un ciclo actual configurado. Cree primero un período en catálogo.", "warning");
                    return RedirectToAction(nameof(Index));
                }

                int anioSiguiente = CicloLectivoHelper.AnioSiguienteCiclo(actual.Periodo);
                var existente = periodos.FirstOrDefault(p => p.Periodo == anioSiguiente);
                if (existente == null)
                {
                    var (ok, err, creado) = await CrearPeriodoSiguienteAsync(anioSiguiente, marcarActual: false);
                    if (!ok)
                    {
                        SetMensaje("No se pudo crear el ciclo " + anioSiguiente + ". " + (err ?? "Verifique la API de CatPeriodo."), "warning");
                        return RedirectToAction(nameof(Index));
                    }

                    existente = creado is { IdPeriodo: > 0 }
                        ? creado
                        : null;
                    if (existente == null)
                    {
                        periodos = await _services.GetPeriodoAsync() ?? periodos;
                        existente = periodos.FirstOrDefault(p => p.Periodo == anioSiguiente);
                    }
                }

                if (existente == null || existente.IdPeriodo <= 0)
                {
                    SetMensaje("El ciclo se envió a la API pero no aparece en el listado. Recargue o revise CatPeriodo.", "warning");
                    return RedirectToAction(nameof(Index));
                }

                if (!copiarTarifas)
                {
                    SetMensaje(
                        $"Ciclo {anioSiguiente} listo para matrículas. No se copiaron tarifas. "
                        + "No se marcó como ciclo actual.",
                        "success");
                    return RedirectToAction(nameof(Index), new { idPeriodo = existente.IdPeriodo });
                }

                var resultado = await CopiarTarifasAsync(actual.IdPeriodo, existente.IdPeriodo);
                string mensaje =
                    $"Ciclo {anioSiguiente} listo para matrículas (reserva y matrícula completa). "
                    + $"Se copiaron {resultado.Copiados} tarifas del ciclo {actual.Periodo}. "
                    + (resultado.Omitidos > 0 ? $"{resultado.Omitidos} ya existían y no se duplicaron. " : "")
                    + "No se marcó como ciclo actual: las mensualidades siguen en el ciclo vigente hasta que usted lo active (habitualmente en enero).";

                if (resultado.Errores.Count > 0)
                {
                    mensaje += " Algunas tarifas no se copiaron: " + string.Join(" ", resultado.Errores.Take(3));
                    SetMensaje(mensaje, "warning");
                }
                else if (resultado.Copiados == 0 && resultado.Omitidos == 0)
                {
                    SetMensaje(
                        $"Ciclo {anioSiguiente} quedó habilitado, pero el ciclo {actual.Periodo} no tiene tarifas activas para copiar. Agréguelas en este ciclo o en el actual.",
                        "warning");
                }
                else
                {
                    SetMensaje(mensaje, "success");
                }

                return RedirectToAction(nameof(Index), new { idPeriodo = existente.IdPeriodo });
            }
            catch (Exception ex)
            {
                SetMensaje("No se pudo habilitar el ciclo siguiente. " + ex.Message, "warning");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarCiclo(int idPeriodo)
        {
            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            var destino = periodos.FirstOrDefault(p => p.IdPeriodo == idPeriodo);
            if (destino == null)
            {
                SetMensaje("No se encontró el ciclo a activar.", "warning");
                return RedirectToAction(nameof(Index));
            }

            int idUsuario = IdUsuarioActual();
            foreach (var p in periodos)
            {
                bool debeSerActual = p.IdPeriodo == idPeriodo;
                if (p.Actual == debeSerActual && (debeSerActual ? p.Activo : true))
                    continue;

                p.Actual = debeSerActual;
                if (debeSerActual)
                    p.Activo = true;
                p.UsuarioActualizo = idUsuario;
                p.FechaActualizo = DateTime.Now;
                var (ok, err) = await _services.UpdatePeriodoAsync(p);
                if (!ok)
                {
                    SetMensaje($"No se pudo actualizar el ciclo {p.Periodo}. " + (err ?? ""), "warning");
                    return RedirectToAction(nameof(Index));
                }
            }

            SetMensaje($"El ciclo {destino.Periodo} quedó como actual. Las mensualidades y el estado de cuenta usarán este período.", "success");
            return RedirectToAction(nameof(Index), new { idPeriodo });
        }

        public async Task<IActionResult> CreateMatricula(int? idPeriodo)
        {
            var vm = await ConstruirFormMatriculaAsync(new TblCostoMatricula
            {
                IdPeriodo = idPeriodo ?? 0,
                Activo = true
            }, esEdicion: false);
            return View("FormMatricula", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMatricula(CostoMatriculaFormViewModel form)
        {
            var costo = form?.Costo ?? new TblCostoMatricula();
            if (!ValidarCostoMatricula(costo, out var error))
            {
                SetMensaje(error, "warning");
                return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, false));
            }

            if (await ExisteMatriculaDuplicada(costo, excluirId: null))
            {
                SetMensaje("Ya existe un costo de matrícula para ese colegio, ciclo y modalidad.", "warning");
                return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, false));
            }

            costo.Activo = true;
            costo.UsuarioRegistro = IdUsuarioActual();
            costo.FechaRegistro = AhoraSql();
            var (ok, err) = await _services.PostCostoMatriculaAsync(costo);
            if (!ok)
            {
                SetMensaje("No se pudo guardar el costo de matrícula. " + (err ?? "Revise el endpoint de la API."), "warning");
                return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, false));
            }

            SetMensaje("Costo de matrícula registrado.", "success");
            return RedirectToAction(nameof(Index), new { idPeriodo = costo.IdPeriodo });
        }

        public async Task<IActionResult> EditMatricula(int id)
        {
            var lista = await _services.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>();
            var costo = lista.FirstOrDefault(c => c.IdCostoMatricula == id);
            if (costo == null)
            {
                SetMensaje("No se encontró el costo de matrícula.", "warning");
                return RedirectToAction(nameof(Index));
            }
            return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, true));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMatricula(CostoMatriculaFormViewModel form)
        {
            var costo = form?.Costo ?? new TblCostoMatricula();
            if (costo.IdCostoMatricula <= 0)
            {
                SetMensaje("Registro no válido.", "warning");
                return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, true));
            }
            if (!ValidarCostoMatricula(costo, out var errorMat))
            {
                SetMensaje(errorMat, "warning");
                return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, true));
            }

            if (await ExisteMatriculaDuplicada(costo, costo.IdCostoMatricula))
            {
                SetMensaje("Ya existe otro costo de matrícula para ese colegio, ciclo y modalidad.", "warning");
                return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, true));
            }

            var lista = await _services.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>();
            var original = lista.FirstOrDefault(c => c.IdCostoMatricula == costo.IdCostoMatricula);
            if (original != null)
            {
                costo.UsuarioRegistro = original.UsuarioRegistro;
                costo.FechaRegistro = original.FechaRegistro;
            }
            costo.UsuarioActualizo = IdUsuarioActual();
            costo.FechaActualizo = DateTime.Now;

            var (ok, err) = await _services.UpdateCostoMatriculaAsync(costo);
            if (!ok)
            {
                SetMensaje("No se pudo actualizar el costo de matrícula. " + (err ?? ""), "warning");
                return View("FormMatricula", await ConstruirFormMatriculaAsync(costo, true));
            }

            SetMensaje("Costo de matrícula actualizado.", "success");
            return RedirectToAction(nameof(Index), new { idPeriodo = costo.IdPeriodo });
        }

        public async Task<IActionResult> CreateMensualidad(int? idPeriodo)
        {
            var vm = await ConstruirFormMensualidadAsync(new TblCostoMensualidad
            {
                IdPeriodo = idPeriodo ?? 0,
                Activo = true
            }, false);
            return View("FormMensualidad", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMensualidad(CostoMensualidadFormViewModel form)
        {
            var costo = form?.Costo ?? new TblCostoMensualidad();
            if (!ValidarCostoMensualidad(costo, out var error))
            {
                SetMensaje(error, "warning");
                return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, false));
            }

            if (await ExisteMensualidadDuplicada(costo, null))
            {
                SetMensaje("Ya existe un costo de mensualidad para ese colegio, nivel, ciclo y modalidad.", "warning");
                return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, false));
            }

            costo.Activo = true;
            costo.UsuarioRegistro = IdUsuarioActual();
            costo.FechaRegistro = AhoraSql();
            var (ok, err) = await _services.PostCostoMensualidadAsync(costo);
            if (!ok)
            {
                SetMensaje("No se pudo guardar el costo de mensualidad. " + (err ?? "Revise el endpoint de la API."), "warning");
                return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, false));
            }

            SetMensaje("Costo de mensualidad registrado.", "success");
            return RedirectToAction(nameof(Index), new { idPeriodo = costo.IdPeriodo });
        }

        public async Task<IActionResult> EditMensualidad(int id)
        {
            var lista = await _services.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>();
            var costo = lista.FirstOrDefault(c => c.IdMensualidad == id);
            if (costo == null)
            {
                SetMensaje("No se encontró el costo de mensualidad.", "warning");
                return RedirectToAction(nameof(Index));
            }
            return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, true));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMensualidad(CostoMensualidadFormViewModel form)
        {
            var costo = form?.Costo ?? new TblCostoMensualidad();
            if (costo.IdMensualidad <= 0)
            {
                SetMensaje("Registro no válido.", "warning");
                return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, true));
            }
            if (!ValidarCostoMensualidad(costo, out var errorMen))
            {
                SetMensaje(errorMen, "warning");
                return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, true));
            }

            if (await ExisteMensualidadDuplicada(costo, costo.IdMensualidad))
            {
                SetMensaje("Ya existe otro costo de mensualidad para esa combinación.", "warning");
                return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, true));
            }

            var lista = await _services.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>();
            var original = lista.FirstOrDefault(c => c.IdMensualidad == costo.IdMensualidad);
            if (original != null)
            {
                costo.UsuarioRegistro = original.UsuarioRegistro;
                costo.FechaRegistro = original.FechaRegistro;
            }
            costo.UsuarioActualizo = IdUsuarioActual();
            costo.FechaActualizo = DateTime.Now;

            var (ok, err) = await _services.UpdateCostoMensualidadAsync(costo);
            if (!ok)
            {
                SetMensaje("No se pudo actualizar el costo de mensualidad. " + (err ?? ""), "warning");
                return View("FormMensualidad", await ConstruirFormMensualidadAsync(costo, true));
            }

            SetMensaje("Costo de mensualidad actualizado.", "success");
            return RedirectToAction(nameof(Index), new { idPeriodo = costo.IdPeriodo });
        }

        private async Task<(bool Ok, string? Error, CatPeriodo? Creado)> CrearPeriodoSiguienteAsync(int anioSiguiente, bool marcarActual)
        {
            var nuevo = new CatPeriodo
            {
                Periodo = anioSiguiente,
                Actual = marcarActual,
                Activo = true,
                UsuarioRegistro = IdUsuarioActual(),
                FechaRegistro = AhoraSql()
            };
            return await _services.PostPeriodoAsync(nuevo);
        }

        private async Task<(int Copiados, int Omitidos, List<string> Errores)> CopiarTarifasAsync(int idPeriodoOrigen, int idPeriodoDestino)
        {
            int copiados = 0;
            int omitidos = 0;
            var errores = new List<string>();
            int idUsuario = IdUsuarioActual();
            var fecha = AhoraSql();

            var mats = (await _services.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>())
                .Where(c => c.IdPeriodo == idPeriodoOrigen && c.Activo)
                .ToList();
            var destMats = (await _services.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>())
                .Where(c => c.IdPeriodo == idPeriodoDestino)
                .ToList();

            foreach (var origen in mats)
            {
                bool yaExiste = destMats.Any(d =>
                    d.IdRecinto == origen.IdRecinto && d.IdModalidad == origen.IdModalidad);
                if (yaExiste)
                {
                    omitidos++;
                    continue;
                }

                var copia = new TblCostoMatricula
                {
                    CostoMatricula = origen.CostoMatricula,
                    IdPeriodo = idPeriodoDestino,
                    IdRecinto = origen.IdRecinto,
                    IdModalidad = origen.IdModalidad,
                    Activo = true,
                    UsuarioRegistro = idUsuario,
                    FechaRegistro = fecha
                };
                var (ok, err) = await _services.PostCostoMatriculaAsync(copia);
                if (ok)
                {
                    copiados++;
                    destMats.Add(copia);
                }
                else if (errores.Count < 5)
                {
                    errores.Add("Matrícula (colegio " + origen.IdRecinto + "): " + (err ?? "error de API"));
                }
            }

            var mens = (await _services.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>())
                .Where(c => c.IdPeriodo == idPeriodoOrigen && c.Activo)
                .ToList();
            var destMens = (await _services.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>())
                .Where(c => c.IdPeriodo == idPeriodoDestino)
                .ToList();

            foreach (var origen in mens)
            {
                bool yaExiste = destMens.Any(d =>
                    d.IdRecinto == origen.IdRecinto
                    && d.IdGrado == origen.IdGrado
                    && d.IdModalidad == origen.IdModalidad);
                if (yaExiste)
                {
                    omitidos++;
                    continue;
                }

                var copia = new TblCostoMensualidad
                {
                    CostoMensualidad = origen.CostoMensualidad,
                    IdPeriodo = idPeriodoDestino,
                    IdRecinto = origen.IdRecinto,
                    IdGrado = origen.IdGrado,
                    IdModalidad = origen.IdModalidad,
                    Activo = true,
                    UsuarioRegistro = idUsuario,
                    FechaRegistro = fecha
                };
                var (ok, err) = await _services.PostCostoMensualidadAsync(copia);
                if (ok)
                {
                    copiados++;
                    destMens.Add(copia);
                }
                else if (errores.Count < 5)
                {
                    errores.Add("Mensualidad (colegio " + origen.IdRecinto + ", grado " + origen.IdGrado + "): " + (err ?? "error de API"));
                }
            }

            return (copiados, omitidos, errores);
        }

        private static DateTime AhoraSql()
        {
            var n = DateTime.Now;
            return new DateTime(n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second, DateTimeKind.Unspecified);
        }

        private async Task<CostoMatriculaFormViewModel> ConstruirFormMatriculaAsync(TblCostoMatricula costo, bool esEdicion)
        {
            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            if (costo.IdPeriodo == 0)
            {
                var destino = CicloLectivoHelper.ResolverPeriodoMatricula(periodos, DateTime.Now)
                              ?? CicloLectivoHelper.ResolverPeriodoActual(periodos);
                costo.IdPeriodo = destino?.IdPeriodo ?? 0;
            }

            return new CostoMatriculaFormViewModel
            {
                Costo = costo,
                EsEdicion = esEdicion,
                Periodos = ToSelectPeriodos(periodos, costo.IdPeriodo),
                Recintos = ToSelectRecintos(await _services.GetRecintosAsync(), costo.IdRecinto),
                Modalidades = ToSelectModalidades(await _services.GetModalidadesAsync(), costo.IdModalidad)
            };
        }

        private async Task<CostoMensualidadFormViewModel> ConstruirFormMensualidadAsync(TblCostoMensualidad costo, bool esEdicion)
        {
            var periodos = await _services.GetPeriodoAsync() ?? new List<CatPeriodo>();
            if (costo.IdPeriodo == 0)
            {
                var actual = CicloLectivoHelper.ResolverPeriodoActual(periodos);
                costo.IdPeriodo = actual?.IdPeriodo ?? 0;
            }

            return new CostoMensualidadFormViewModel
            {
                Costo = costo,
                EsEdicion = esEdicion,
                Periodos = ToSelectPeriodos(periodos, costo.IdPeriodo),
                Recintos = ToSelectRecintos(await _services.GetRecintosAsync(), costo.IdRecinto),
                Modalidades = ToSelectModalidades(await _services.GetModalidadesAsync(), costo.IdModalidad),
                Grados = (await _services.GetGradosAsync() ?? new List<Grados>())
                    .Where(g => g.Activo)
                    .OrderBy(g => g.NombreGrado)
                    .Select(g => new SelectListItem
                    {
                        Value = g.IdGrado.ToString(),
                        Text = g.NombreGrado,
                        Selected = g.IdGrado == costo.IdGrado
                    }).ToList()
            };
        }

        private static List<SelectListItem> ToSelectPeriodos(IEnumerable<CatPeriodo>? periodos, int seleccionado)
            => (periodos ?? Enumerable.Empty<CatPeriodo>())
                .Where(p => p.Activo || p.IdPeriodo == seleccionado)
                .OrderByDescending(p => p.Periodo)
                .Select(p => new SelectListItem
                {
                    Value = p.IdPeriodo.ToString(),
                    Text = p.Actual ? $"{p.Periodo} (actual)" : p.Periodo.ToString(),
                    Selected = p.IdPeriodo == seleccionado
                }).ToList();

        private static List<SelectListItem> ToSelectRecintos(IEnumerable<Recintos>? recintos, int seleccionado)
            => (recintos ?? Enumerable.Empty<Recintos>())
                .Where(r => r.Activo || r.IdRecinto == seleccionado)
                .Select(r => new SelectListItem
                {
                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto,
                    Selected = r.IdRecinto == seleccionado
                }).ToList();

        private static List<SelectListItem> ToSelectModalidades(IEnumerable<Modalidades>? modalidades, int seleccionado)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "0", Text = "Todas las modalidades", Selected = seleccionado == 0 }
            };
            items.AddRange((modalidades ?? Enumerable.Empty<Modalidades>())
                .Where(m => m.Activo || m.IdModalidad == seleccionado)
                .Select(m => new SelectListItem
                {
                    Value = m.IdModalidad.ToString(),
                    Text = m.Modalidad,
                    Selected = m.IdModalidad == seleccionado
                }));
            return items;
        }

        private async Task<bool> ExisteMatriculaDuplicada(TblCostoMatricula costo, int? excluirId)
        {
            var lista = await _services.GetCostosMatriculaAsync() ?? new List<TblCostoMatricula>();
            return lista.Any(c =>
                c.IdPeriodo == costo.IdPeriodo
                && c.IdRecinto == costo.IdRecinto
                && c.IdModalidad == costo.IdModalidad
                && (!excluirId.HasValue || c.IdCostoMatricula != excluirId.Value));
        }

        private async Task<bool> ExisteMensualidadDuplicada(TblCostoMensualidad costo, int? excluirId)
        {
            var lista = await _services.GetCostosMensualidadAsync() ?? new List<TblCostoMensualidad>();
            return lista.Any(c =>
                c.IdPeriodo == costo.IdPeriodo
                && c.IdRecinto == costo.IdRecinto
                && c.IdGrado == costo.IdGrado
                && c.IdModalidad == costo.IdModalidad
                && (!excluirId.HasValue || c.IdMensualidad != excluirId.Value));
        }

        private static bool ValidarCostoMatricula(TblCostoMatricula costo, out string error)
        {
            if (costo.CostoMatricula <= 0)
            {
                error = "El valor de matrícula debe ser mayor que cero.";
                return false;
            }
            if (costo.IdPeriodo <= 0 || costo.IdRecinto <= 0)
            {
                error = "Debe seleccionar ciclo y colegio.";
                return false;
            }
            error = "";
            return true;
        }

        private static bool ValidarCostoMensualidad(TblCostoMensualidad costo, out string error)
        {
            if (costo.CostoMensualidad <= 0)
            {
                error = "El valor de mensualidad debe ser mayor que cero.";
                return false;
            }
            if (costo.IdPeriodo <= 0 || costo.IdRecinto <= 0 || costo.IdGrado <= 0)
            {
                error = "Debe seleccionar ciclo, colegio y nivel (grado).";
                return false;
            }
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
