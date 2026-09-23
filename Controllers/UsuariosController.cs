using Microsoft.AspNetCore.Authorization;
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
using WebColegio.Helpers;
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
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _grados = await _Iservices.GetGradosAsync();

            var viewmodel = new UsuarioViewModel
            {
                ListaUsuarios = _usuarios,
                ListRol=_roles,
                ListRecintos=_recintos,
                ListaAlumnos = _alumnos ?? new List<TblAlumno>(),
                ListGrados = _grados ?? new List<Grados>()
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
                string cedula = viewmodel.usuarios.Cedula?.Trim() ?? string.Empty;
                viewmodel.usuarios.Cedula = string.IsNullOrWhiteSpace(cedula) ? viewmodel.usuarios.Cedula : cedula;
                string password = viewmodel.Password;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(hashedPassword);

                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    var existeCedula = await _Iservices.validarUsuarios(cedula);
                    if (existeCedula)
                    {
                        TempData["Mensaje"] = "Ya existe un usuario registrado con este número de cédula. No se puede guardar un duplicado.";
                        TempData["Tipo"] = "warning";
                        return RedirectToAction("Create");
                    }
                }

                viewmodel.usuarios.Password = passwordBytes;
                viewmodel.usuarios.UsuarioRegistro = idUsuario;
                viewmodel.usuarios.FechaRegistro = DateTime.Now;
                viewmodel.usuarios.Activo = true;

                response=await _Iservices.PostUsuarios(viewmodel.usuarios);
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

                var cedulaEdit = usuario.usuarios.Cedula?.Trim();
                usuario.usuarios.Cedula = string.IsNullOrWhiteSpace(cedulaEdit) ? usuario.usuarios.Cedula : cedulaEdit;
                if (!string.IsNullOrWhiteSpace(cedulaEdit))
                {
                    var existeCedula = await _Iservices.validarUsuarios(cedulaEdit, usuario.usuarios.IdUsuario);
                    if (existeCedula)
                    {
                        TempData["Mensaje"] = "El número de cédula ya pertenece a otro usuario. No se puede guardar un duplicado.";
                        TempData["Tipo"] = "warning";
                        return View(usuario);
                    }
                }

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

        // POST: UsuariosController/Eliminar/5 — eliminación física
        [Authorize(Roles = "Admin,UserSystem")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (id <= 0)
            {
                TempData["Mensaje"] = "Usuario no válido.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (id == idActual)
            {
                TempData["Mensaje"] = "No puede eliminar su propia cuenta mientras está en sesión.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            TblUsuarios? objetivo = null;
            try
            {
                objetivo = await _Iservices.GetUsuarioIdAsync(id);
            }
            catch
            {
                objetivo = null;
            }
            if (objetivo != null && objetivo.IdUsuario > 0)
            {
                var rolObjetivo = await _Iservices.GetRol(objetivo.IdRol);
                var nombreRol = AuthRoleHelper.ResolverNombreRol(rolObjetivo?.NombreRol, objetivo.IdRol);
                if (string.Equals(nombreRol, "UserSystem", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(rolObjetivo?.NombreRol, "UserSystem", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Mensaje"] = "No se puede eliminar la cuenta de sistema.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Index));
                }
            }

            var (ok, mensaje) = await _Iservices.DeleteUsuarioAsync(id);
            if (ok)
            {
                TempData["Mensaje"] = "La cuenta se eliminó de forma permanente.";
                TempData["Tipo"] = "success";
            }
            else
            {
                TempData["Mensaje"] = string.IsNullOrWhiteSpace(mensaje)
                    ? "No se pudo eliminar la cuenta."
                    : mensaje;
                TempData["Tipo"] = "warning";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
