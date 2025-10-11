using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class InventarioController : Controller
    {
        private readonly IServicesApi _Iservices;
        public InventarioController(IServicesApi services)
        {
            _Iservices = services;
        }


        // GET: InventarioController
        public async Task<ActionResult> Index()
        {
            var _listProducto= await _Iservices.GetProductosAsync();
            var viewModel = new ColeccionCatalogos
            {
                producto = _listProducto,
            };

            return View(viewModel);
        }

        // GET: InventarioController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: InventarioController/Create
        public async Task<ActionResult> Create()
        {
            var viewmodel = new InventarioViewModel
            {

                ListaProductos = (await _Iservices.GetProductosAsync())
                                 .Select(r => new SelectListItem
                                 {
                                     Value = r.IdProducto.ToString(),
                                     Text = r.NombreProducto,
                                     //Selected = r.IdPregunta == respuestas.IdPregunta
                                 }).ToList(),

            };
            return View(viewmodel);
        }

        // POST: InventarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TblInventario inventario)
        {
            bool response = false;
            try
            {
                //validarDuplicado = await _Iservices.ValidarNotas(notas.IdAsignatura, notas.IdPeriodo, notas.IdAlumno);
                //if (validarDuplicado == true)
                //{
                //    TempData["Mensaje"] = "El Alumno ya Posee un Registro de Nota con la Asignatura Seleccionada.";
                //    TempData["Tipo"] = "warning";
                //    return RedirectToAction("Create");
                //}

                if (inventario != null)
                {

                    response = await _Iservices.PostInventarioAsync(inventario);
                    if (response)
                    {
                        TempData["Mensaje"] = "Se registro el inventario Correctamente.";
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

        // GET: InventarioController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: InventarioController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: InventarioController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: InventarioController/Delete/5
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
