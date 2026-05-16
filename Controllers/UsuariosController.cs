using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
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
        public async Task<ActionResult> Index()
        {

            var _usuarios = await _Iservices.GetUsuariosAsync();
            var _recintos = await _Iservices.GetRecintosAsync();
            var _roles = await _Iservices.GetRolAsync();
            

            var viewmodel = new UsuarioViewModel
            {
                ListaUsuarios = _usuarios,
                ListRol=_roles,
                ListRecintos=_recintos

            };

            return View(viewmodel);
        }

        // GET: UsuariosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsuariosController/Create
        public async Task<ActionResult> Create()
        {
           
            var _usuario = await _Iservices.GetUsuariosAsync();
            var viewModel = new UsuarioViewModel
            {
                ListaUsuarios = _usuario,
                RolSelectList = (await _Iservices.GetRolAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdRol.ToString(),
                                  Text = r.NombreRol,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
                RecintosSelectList = (await _Iservices.GetRecintosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdRecinto.ToString(),
                                  Text = r.Recinto,
                                  //Selected = r.IdPregunta == respuestas.IdPregunta
                              }).ToList(),
            };

            return View(viewModel);
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Create(UsuarioViewModel viewmodel)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool response=false;
                bool existeLogin;
                string nombre = viewmodel.usuarios.NombreUsuario;
                string cedula = viewmodel.usuarios.Cedula;
                string password = viewmodel.Password;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);

                existeLogin = await _Iservices.validarUsuarios(cedula);//validar si existe el usuario

                if (existeLogin)
                {
                    TempData["Mensaje"] = "El usuario ya se encuentra registrado.";
                    TempData["Tipo"] = "informacion";
                    return RedirectToAction("Create");
                }

                else
                {

                    //viewmodel.usuarios.NombreUsuario = nombre;

                    viewmodel.usuarios.Password = passwordBytes;
                    viewmodel.usuarios.UsuarioRegistro = idUsuario;
                    viewmodel.usuarios.FechaRegistro = DateTime.Now;
                    viewmodel.usuarios.Activo = true;

                     response=await _Iservices.PostUsuarios(viewmodel.usuarios);
                }
                if(response)
                {
                    TempData["Mensaje"] = "Usuaio registrado correctamente.";
                    TempData["Tipo"] = "success";
                    return RedirectToAction("Create");
                }
                else
                {
                    TempData["Mensaje"] = "No se logro procesar el registro del usuario.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }
            
            }
            catch
            {
                return View();
            }
        }
        // GET: UsuariosController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            TblUsuarios usuarioId;
            try
            {
                usuarioId = await _Iservices.GetUsuarioIdAsync(id);
            }
            catch
            {
                return NotFound();
            }

            if (usuarioId == null || usuarioId.IdUsuario <= 0)
                return NotFound();

            var viewmodel = new UsuarioViewModel
            {
                usuarios = usuarioId,
                RolSelectList = (await _Iservices.GetRolAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdRol.ToString(),
                                  Text = r.NombreRol,
                              }).ToList(),
                RecintosSelectList = (await _Iservices.GetRecintosAsync())
                              .Select(r => new SelectListItem
                              {
                                  Value = r.IdRecinto.ToString(),
                                  Text = r.Recinto,
                              }).ToList(),
            };

            return View(viewmodel);
        }

        // POST: UsuariosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UsuarioViewModel usuario)
        {
            async Task CargarListasEditAsync()
            {
                usuario.RolSelectList = (await _Iservices.GetRolAsync())
                    .Select(r => new SelectListItem { Value = r.IdRol.ToString(), Text = r.NombreRol }).ToList();
                usuario.RecintosSelectList = (await _Iservices.GetRecintosAsync())
                    .Select(r => new SelectListItem { Value = r.IdRecinto.ToString(), Text = r.Recinto }).ToList();
            }

            try
            {
                if (usuario?.usuarios == null || usuario.usuarios.IdUsuario <= 0)
                {
                    TempData["Mensaje"] = "Datos de usuario no válidos.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Index));
                }

                await CargarListasEditAsync();

                if (!string.IsNullOrWhiteSpace(usuario.Password))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.Password.Trim());
                    usuario.usuarios.Password = Encoding.UTF8.GetBytes(hashedPassword);
                }
                else
                {
                    usuario.usuarios.Password = null!;
                }

                int idUsuarioActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                usuario.usuarios.UsuarioActualiza = idUsuarioActual;
                usuario.usuarios.FechaActualiza = DateTime.Now;

                bool response = await _Iservices.UpdateUsuario(usuario.usuarios);
                if (response)
                {
                    TempData["Mensaje"] = "Usuario actualizado correctamente.";
                    TempData["Tipo"] = "success";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Mensaje"] = "No se logró actualizar el usuario. Revise los datos o intente nuevamente.";
                TempData["Tipo"] = "warning";
                return View(usuario);
            }
            catch (Exception)
            {
                if (usuario != null)
                {
                    await CargarListasEditAsync();
                    TempData["Mensaje"] = "Ocurrió un error al procesar los datos. Por favor, intente nuevamente.";
                    TempData["Tipo"] = "warning";
                    return View(usuario);
                }

                TempData["Mensaje"] = "Ocurrió un error al procesar los datos.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
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
