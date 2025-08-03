using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class NotasController : Controller
    {
        // GET: NotasController
        public ActionResult Index()
        {
            return View();
        }

        // GET: NotasController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: NotasController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NotasController/Create
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

        // GET: NotasController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: NotasController/Edit/5
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
