using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
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
            var notasFiltrados = query.ToList();
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
            var viewModel = new NotasViewModel
            {
                listNotas = listnota,
                alumnoNotas = v_alumNota,

                asignaturaSelectListItem = (await _Iservices.GetAsignaturaAsync())
                                            .Select(r => new SelectListItem
                                            {
                                                Value = r.IdAsignatura.ToString(),
                                                Text = r.NombreAsignatura
                                            }).ToList(),
                periodoSelectListItem = (await _Iservices.GetPeriodoAsync())
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
            var viewModel = new NotasViewModel
            {
                listNotas = listnota,
                alumnoNotas=v_alumNota,

                asignaturaSelectListItem = (await _Iservices.GetAsignaturaAsync())
                                            .Select(r => new SelectListItem
                                            {
                                                Value = r.IdAsignatura.ToString(),
                                                Text = r.NombreAsignatura
                                            }).ToList(),
                periodoSelectListItem = (await _Iservices.GetPeriodoAsync())
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
            var viewmodel = new NotasViewModel
            {

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

            };

            return View(viewmodel);
        }

        // POST: NotasController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TblNotas notas)
        {
            bool response = false;
            bool validarDuplicado= false;
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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

                    response = await _Iservices.PostNotasAsync(notas);
                    if (response)
                    {
                        TempData["Mensaje"] = "Se agrego la Nota del Alumno Correctamente.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("create");
                    }

                }
                return NoContent();
            }
            catch
            {
                return View();
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

                if (!ModelState.IsValid)
                {

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
