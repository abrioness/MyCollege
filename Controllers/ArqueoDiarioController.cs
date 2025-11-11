using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class ArqueoDiarioController : Controller
    {
        // GET: ArqueoDiarioController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ArqueoDiarioController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ArqueoDiarioController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ArqueoDiarioController/Create
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

        // GET: ArqueoDiarioController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ArqueoDiarioController/Edit/5
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

        // GET: ArqueoDiarioController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ArqueoDiarioController/Delete/5
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
