﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebColegio.Controllers
{
    public class ProfesoresController : Controller
    {
        // GET: ProfesoresController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ProfesoresController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProfesoresController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProfesoresController/Create
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

        // GET: ProfesoresController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProfesoresController/Edit/5
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

        // GET: ProfesoresController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProfesoresController/Delete/5
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
