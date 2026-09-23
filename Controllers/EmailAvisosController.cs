using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    [Authorize(Roles = "Admin,UserSystem,Cajero")]
    public class EmailAvisosController : Controller
    {
        private readonly EmailAvisoService _avisos;

        public EmailAvisosController(EmailAvisoService avisos)
        {
            _avisos = avisos;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var destinos = await _avisos.ListarPendientesAsync();
            var estado = _avisos.EstadoCampana();
            var programada = estado.FechaProgramada;
            string mensaje;
            if (!_avisos.SmtpConfigurado)
                mensaje = "Configure el correo del colegio para enviar avisos. Mientras tanto puede revisar quién tiene correo y saldo pendiente.";
            else if (programada.HasValue)
                mensaje = $"Hay una campaña programada para el {programada.Value:dd/MM/yyyy} (se envía a partir de las {_avisos.HoraEnvio:00}:00). Puede enviar ahora o cambiar la fecha.";
            else
                mensaje = "Puede enviar a uno, a los seleccionados o a todos. También puede programar una fecha.";

            return View(new EmailAvisoViewModel
            {
                SmtpConfigurado = _avisos.SmtpConfigurado,
                UltimaCampana = estado.UltimaCampanaFecha?.ToString("dd/MM/yyyy HH:mm"),
                FechaProgramada = programada,
                Destinos = destinos,
                Historial = estado.Envios.Take(40).ToList(),
                MensajeEstado = mensaje
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarUno(int idAlumno)
        {
            if (!AsegurarSmtp())
                return RedirectToAction(nameof(Index));

            var logs = await _avisos.EnviarAsync(new[] { idAlumno }, marcarCampana: false);
            var log = logs.FirstOrDefault();
            TempData["Tipo"] = log?.Exito == true ? "success" : "warning";
            TempData["Mensaje"] = log == null
                ? "No se encontró correo o saldo pendiente para ese alumno."
                : log.Exito
                    ? $"Correo enviado a {log.NombreAlumno}."
                    : "No se pudo enviar el correo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarSeleccion(int[] idAlumnos)
        {
            if (!AsegurarSmtp())
                return RedirectToAction(nameof(Index));

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
            if (!AsegurarSmtp())
                return RedirectToAction(nameof(Index));

            var logs = await _avisos.EnviarCampanaAsync();
            var ok = logs.Count(l => l.Exito);
            TempData["Tipo"] = ok > 0 ? "success" : "warning";
            TempData["Mensaje"] = logs.Count == 0
                ? "No hay tutores con correo y saldo pendiente."
                : $"Campaña: {ok} enviados de {logs.Count}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Programar(DateTime fechaProgramada)
        {
            if (fechaProgramada.Date < DateTime.Today)
            {
                TempData["Tipo"] = "warning";
                TempData["Mensaje"] = "La fecha programada no puede ser anterior a hoy.";
                return RedirectToAction(nameof(Index));
            }

            _avisos.Programar(fechaProgramada.Date);

            if (_avisos.SmtpConfigurado
                && fechaProgramada.Date == DateTime.Today
                && DateTime.Now.Hour >= _avisos.HoraEnvio)
            {
                var logs = await _avisos.EnviarCampanaAsync();
                var ok = logs.Count(l => l.Exito);
                TempData["Tipo"] = ok > 0 ? "success" : "warning";
                TempData["Mensaje"] = logs.Count == 0
                    ? "Se programó para hoy. No hay tutores con correo y saldo pendiente."
                    : $"La fecha es hoy y ya pasó la hora de envío. Campaña: {ok} enviados de {logs.Count}.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Tipo"] = "success";
            TempData["Mensaje"] = $"Campaña programada para el {fechaProgramada:dd/MM/yyyy}. Se enviará a partir de las {_avisos.HoraEnvio:00}:00.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelarProgramacion()
        {
            _avisos.CancelarProgramacion();
            TempData["Tipo"] = "success";
            TempData["Mensaje"] = "Se canceló la campaña programada.";
            return RedirectToAction(nameof(Index));
        }

        private bool AsegurarSmtp()
        {
            if (_avisos.SmtpConfigurado)
                return true;
            TempData["Tipo"] = "warning";
            TempData["Mensaje"] = "El envío de correo no está configurado. Contacte al administrador.";
            return false;
        }
    }
}
