using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class PagoColegiaturaController : Controller
    {
        // GET: PagoColegiaturaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: PagoColegiaturaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PagoColegiaturaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PagoColegiaturaController/Create
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

        // GET: PagoColegiaturaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PagoColegiaturaController/Edit/5
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

        // GET: PagoColegiaturaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PagoColegiaturaController/Delete/5
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
