using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

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
        public async Task<ActionResult> Index()
        {

            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _sexos = await _Iservices.GetSexosAsync();
            //var _grupos = await _Iservices.GetGruposAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _turnos = await _Iservices.GetTurnosAsync();
            //var _modalidades = await _Iservices.GetModalidadesAsync();
            //var _recintos = await _Iservices.GetRecintosAsync();

            var VieModelAlumnos = new ColeccionCatalogos
            {
                alumno = _alumnos,
                sexos = _sexos,
                //grupos = _grupos,
                grados = _grados,
                turnos = _turnos,
                //modalidades = _modalidades,
                //recintos = _recintos
            };

            return View(VieModelAlumnos);
        }

        // GET: AlumnosController/Details/5
        public ActionResult Details(int id)
        {

            return View();
        }

       

        // GET: AlumnosController/Create
        public async Task<ActionResult> Create()
        {
            
            var viewmodel = new AlumnosViewModel
            {

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

        // POST: AlumnosController/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TblAlumno alumnos)
        {
            bool response = false;
            try
            {
                if (alumnos != null)
                {
                   response= await _Iservices.PostAlumnosAsync(alumnos);
                    if(response)
                    {
                        TempData["Message"] = "Alumno creado correctamente";
                        return RedirectToAction(nameof(Index));
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
        public ActionResult Edit(int id)
        {
            //var alumno = _context.TblAlumnos.FirstOrDefault(a => a.Id == id);

            if (id == 0)
            {
                return NotFound(); // si no existe
            }

            return View(); // enviamos el modelo a la vista
        }

        // POST: AlumnosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [FromForm] TblAlumno alumno)
        {
            if (!ModelState.IsValid)
            {
                return View(alumno); // si hay errores, devuelve a la vista con los datos
            }

            //try
            //{
            //    var alumnoDB = _context.TblAlumnos.FirstOrDefault(a => a.Id == id);

            //    if (alumnoDB == null)
            //    {
            //        return NotFound();
            //    }

            //    // Actualizamos los campos que se pueden editar
            //    alumnoDB.Nombre = alumno.Nombre;
            //    alumnoDB.Apellido = alumno.Apellido;
            //    alumnoDB.Activo = alumno.Activo;

            //    _context.Update(alumnoDB);
            //    _context.SaveChanges();

               return RedirectToAction(nameof(Index));
            //}
            //catch
            //{
            //    return View(alumno);
            //}
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
