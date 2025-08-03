using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class AsignarProfesorMateriaController : Controller
    {
        // GET: AsignarProfesorMateriaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: AsignarProfesorMateriaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AsignarProfesorMateriaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AsignarProfesorMateriaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: AsignarProfesorMateriaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AsignarProfesorMateriaController/Edit/5
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

        // GET: AsignarProfesorMateriaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AsignarProfesorMateriaController/Delete/5
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
