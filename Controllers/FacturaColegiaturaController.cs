using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class FacturaColegiaturaController : Controller
    {
        // GET: FacturaColegiaturaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: FacturaColegiaturaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FacturaColegiaturaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FacturaColegiaturaController/Create
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

        // GET: FacturaColegiaturaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FacturaColegiaturaController/Edit/5
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

        // GET: FacturaColegiaturaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FacturaColegiaturaController/Delete/5
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
