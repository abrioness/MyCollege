using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class TipoColegiaturaController : Controller
    {
        // GET: TipoColegiaturaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: TipoColegiaturaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TipoColegiaturaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoColegiaturaController/Create
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

        // GET: TipoColegiaturaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TipoColegiaturaController/Edit/5
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

        // GET: TipoColegiaturaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TipoColegiaturaController/Delete/5
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
