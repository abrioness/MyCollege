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
            var programada = estado.FechaProgramada;
            string mensaje;
            if (!_avisos.ApiConfigurada)
                mensaje = $"Los mensajes masivos deben salir del {_avisos.NumeroRemitenteMostrar}. Ese número ya está asignado; falta vincularlo en WhatsApp Business para que el sistema envíe solo. Mientras tanto puede abrir el chat uno a uno (entre a WhatsApp Web con ese número).";
            else if (programada.HasValue)
                mensaje = $"Los mensajes salen del {_avisos.NumeroRemitenteMostrar}. Campaña programada para el {programada.Value:dd/MM/yyyy} (a partir de las {_avisos.HoraEnvio:00}:00).";
            else
                mensaje = $"Los mensajes masivos se envían desde el {_avisos.NumeroRemitenteMostrar}. Puede enviar a uno, a los seleccionados o a todos, o programar una fecha.";

            return View(new WhatsAppAvisoViewModel
            {
                ApiConfigurada = _avisos.ApiConfigurada,
                EnvioAutomatico = _avisos.EnvioAutomatico,
                NumeroRemitente = _avisos.NumeroRemitenteMostrar,
                FechaProgramada = programada,
                UltimaCampana = estado.UltimaCampanaFecha?.ToString("dd/MM/yyyy HH:mm"),
                Destinos = destinos,
                Historial = estado.Envios.Take(40).ToList(),
                MensajeEstado = mensaje
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarUno(int idAlumno)
        {
            if (!AsegurarApi())
                return RedirectToAction(nameof(Index));

            var logs = await _avisos.EnviarAsync(new[] { idAlumno }, marcarCampana: false);
            var log = logs.FirstOrDefault();
            TempData["Tipo"] = log?.Exito == true ? "success" : "warning";
            TempData["Mensaje"] = log == null
                ? "No se encontró teléfono o saldo pendiente para ese alumno."
                : log.Exito
                    ? $"Mensaje enviado a {log.NombreAlumno} desde {_avisos.NumeroRemitenteMostrar}."
                    : "No se pudo enviar el mensaje.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarSeleccion(int[] idAlumnos)
        {
            if (!AsegurarApi())
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
            TempData["Mensaje"] = $"Enviados {ok} de {logs.Count} desde {_avisos.NumeroRemitenteMostrar}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarCampana()
        {
            if (!AsegurarApi())
                return RedirectToAction(nameof(Index));

            var logs = await _avisos.EnviarCampanaAsync();
            var ok = logs.Count(l => l.Exito);
            TempData["Tipo"] = ok > 0 ? "success" : "warning";
            TempData["Mensaje"] = logs.Count == 0
                ? "No hay tutores con teléfono y saldo pendiente."
                : $"Campaña desde {_avisos.NumeroRemitenteMostrar}: {ok} enviados de {logs.Count}.";
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

            if (_avisos.ApiConfigurada
                && fechaProgramada.Date == DateTime.Today
                && DateTime.Now.Hour >= _avisos.HoraEnvio)
            {
                var logs = await _avisos.EnviarCampanaAsync();
                var ok = logs.Count(l => l.Exito);
                TempData["Tipo"] = ok > 0 ? "success" : "warning";
                TempData["Mensaje"] = logs.Count == 0
                    ? "Se programó para hoy. No hay tutores con teléfono y saldo pendiente."
                    : $"La fecha es hoy y ya pasó la hora de envío. Campaña desde {_avisos.NumeroRemitenteMostrar}: {ok} enviados de {logs.Count}.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Tipo"] = "success";
            TempData["Mensaje"] = $"Campaña programada para el {fechaProgramada:dd/MM/yyyy} desde {_avisos.NumeroRemitenteMostrar}. Se enviará a partir de las {_avisos.HoraEnvio:00}:00.";
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

        private bool AsegurarApi()
        {
            if (_avisos.ApiConfigurada)
                return true;
            TempData["Tipo"] = "warning";
            TempData["Mensaje"] = $"El {_avisos.NumeroRemitenteMostrar} aún no está vinculado para envío masivo. Contacte al administrador.";
            return false;
        }
    }
}
