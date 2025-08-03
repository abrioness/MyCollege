using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class PeriodoEvaluacionController : Controller
    {
        // GET: PeriodoEvaluacionController
        public ActionResult Index()
        {
            return View();
        }

        // GET: PeriodoEvaluacionController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PeriodoEvaluacionController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PeriodoEvaluacionController/Create
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

        // GET: PeriodoEvaluacionController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PeriodoEvaluacionController/Edit/5
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

        // GET: PeriodoEvaluacionController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PeriodoEvaluacionController/Delete/5
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
