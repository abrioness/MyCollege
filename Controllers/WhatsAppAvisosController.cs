using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    [Authorize(Roles = "Admin,UserSystem,Cajero")]
    public class WhatsAppAvisosController : Controller
    {
        private readonly WhatsAppAvisoService _avisos;

        public WhatsAppAvisosController(WhatsAppAvisoService avisos)
        {
            _avisos = avisos;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var destinos = await _avisos.ListarPendientesAsync();
            var estado = _avisos.EstadoCampana();
            return View(new WhatsAppAvisoViewModel
            {
                ApiConfigurada = _avisos.ApiConfigurada,
                EnvioAutomatico = _avisos.EnvioAutomatico,
                UltimaCampana = estado.UltimaCampanaFecha?.ToString("dd/MM/yyyy HH:mm"),
                Destinos = destinos,
                Historial = estado.Envios.Take(40).ToList(),
                MensajeEstado = _avisos.ApiConfigurada
                    ? "La API de WhatsApp está configurada. Puede enviar masivo o dejar el automático del día 1."
                    : "Sin token de Meta. Puede avisar uno a uno con el botón de WhatsApp Web. Para el envío automático del día 1 configure AccessToken y PhoneNumberId."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarUno(int idAlumno)
        {
            var logs = await _avisos.EnviarAsync(new[] { idAlumno }, marcarCampana: false);
            var log = logs.FirstOrDefault();
            TempData["Tipo"] = log?.Exito == true ? "success" : "warning";
            TempData["Mensaje"] = log == null
                ? "No se encontró teléfono o saldo pendiente para ese alumno."
                : log.Exito
                    ? $"Mensaje enviado a {log.NombreAlumno}."
                    : $"No se envió: {log.Detalle}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarSeleccion(int[] idAlumnos)
        {
            if (idAlumnos == null || idAlumnos.Length == 0)
            {
                TempData["Tipo"] = "warning";
                TempData["Mensaje"] = "Seleccione al menos un alumno.";
                return RedirectToAction(nameof(Index));
            }

            var logs = await _avisos.EnviarAsync(idAlumnos, marcarCampana: false);
            var ok = logs.Count(l => l.Exito);
            TempData["Tipo"] = ok > 0 ? "success" : "warning";
            TempData["Mensaje"] = $"Enviados {ok} de {logs.Count}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarCampana()
        {
            var logs = await _avisos.EnviarCampanaAsync();
            var ok = logs.Count(l => l.Exito);
            TempData["Tipo"] = ok > 0 ? "success" : "warning";
            TempData["Mensaje"] = logs.Count == 0
                ? "No hay tutores con teléfono y saldo pendiente."
                : $"Campaña: {ok} enviados de {logs.Count}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
