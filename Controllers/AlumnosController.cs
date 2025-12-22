using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
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
        // GET: AlumnosController
        [Authorize]
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {

                var _alumnos = await _Iservices.GetAlumnosAsync();
             _alumnos = _alumnos
            .Where(r => r.Activo==true)
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

            // Aplicar filtros de manera acumulativa sin ejecutar la consulta
            if (fechainicio.HasValue)
            {
                // Normalizar la fecha de inicio al inicio del día (00:00:00)
                var fechaInicioNormalizada = fechainicio.Value.Date;
                query = query.Where(a => a.FechaRegistro >= fechaInicioNormalizada);
            }
            if (fechafin.HasValue)
            {
                // Normalizar la fecha de fin al final del día (23:59:59)
                var fechaFinNormalizada = fechafin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.FechaRegistro <= fechaFinNormalizada);
            }

            // Ejecutar la consulta SOLO al final, después de aplicar todos los filtros
            var alumnosFiltrados = query.ToList();

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
            bool response = false;
            bool existe = false;
            try
            {
                var buscarIdAlumnos=await _Iservices.GetAlumnosAsync();
                alumnos.IdPeriodo = await _Iservices.GetPeriodoAsync().ContinueWith(p => p.Result.FirstOrDefault(a => a.Activo && a.Actual)?.IdPeriodo) ?? 0;
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                existe = await _Iservices.ValidarAlumnoDuplicado(alumnos.CodigoUnico);
                if (existe)
                {
                    TempData["Mensaje"] = "Ya existe un alumno con el mismo Código";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }
                if (alumnos != null)
                {
                    //alumnos.Activo = true;
                    alumnos.UsuarioRegistro = idUsuario;
                    alumnos.FechaRegistro = DateTime.Now;
                    response = await _Iservices.PostAlumnosAsync(alumnos);
                    if(response)
                    {
                        var idAlumno = buscarIdAlumnos.Max(a => a.IdAlumno);
                        TempData["Mensaje"] = "Se registro correctamente al Estudiante";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Details", "Alumnos", new { id = idAlumno + 1 });
                    }
                    
                    else
                    {
                        TempData["Mensaje"] = "No se proceso el registro.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create");
                    }
                }
                return NoContent();
            }
            catch
            {
                return View();
            }
        }

        // GET: AlumnosController/Edit/5

        public async Task<ActionResult> Edit(int id)
        {
            var alumnos = await _Iservices.GetAlumnoIdAsync(id);
             if (id == 0)
            {
                return NotFound(); // si no existe
            }

            var viewmodel = new AlumnosViewModel
            {
                alumnos = alumnos,
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

                if (!ModelState.IsValid)
                 {
                    //viewModel.alumnos.Activo = true;
                    viewModel.alumnos.UsuarioActualiza = idUsuario;
                    viewModel.alumnos.FechaActualiza = DateTime.Now;
                    var actualizado = await _Iservices.UpdateAlumnos(viewModel.alumnos);

                if (actualizado)
                {
                    TempData["Mensaje"] = "Datos del estudiante se actualizadar&oacuten correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Error al actualizar los datos del alumno.");
            }

           
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
                return View(viewModel);
            }
            catch
            {
                return View();
            }
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
