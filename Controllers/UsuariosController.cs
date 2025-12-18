using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;


namespace WebColegio.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IServicesApi _Iservices;
        public UsuariosController(IServicesApi services)
        {
            _Iservices = services;
        }

        // GET: UsuariosController
        public ActionResult Index()
        {
            return View();
        }

        // GET: UsuariosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {


            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Create(UsuarioViewModel viewmodel)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                bool existeLogin;
                string nombre = viewmodel.usuarios.NombreUsuario;
                string password = viewmodel.Password;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);

                existeLogin = await _Iservices.validarUsuarios(nombre);//validar si existe el usuario

                if (existeLogin)
                {
                    TempData["Mensaje"] = "El usuario ya se encuentra registrado.";
                    return BadRequest();
                }

                else
                {

                    viewmodel.usuarios.NombreUsuario = nombre;
                    viewmodel.usuarios.Password = passwordBytes;
                    viewmodel.usuarios.UsuarioRegistro = idUsuario;
                    viewmodel.usuarios.FechaRegistro = DateTime.Now;
                    viewmodel.usuarios.Activo = true;

                    await _Iservices.PostUsuarios(viewmodel.usuarios);
                }
                return RedirectToAction("Index", "Contenidos");
            }
            catch
            {
                return View();
            }
        }
        // GET: UsuariosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UsuariosController/Edit/5
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

        // GET: UsuariosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuariosController/Delete/5
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
