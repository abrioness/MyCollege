using Microsoft.Extensions.Options;
using WebColegio.Configuration;
using WebColegio.Helpers;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public class EmailAvisoService
    {
        private readonly WhatsAppAvisoService _pendientes;
        private readonly IEmailSender _sender;
        private readonly EmailCampanaStore _store;
        private readonly EmailSettings _settings;

        public EmailAvisoService(
            WhatsAppAvisoService pendientes,
            IEmailSender sender,
            EmailCampanaStore store,
            IOptions<EmailSettings> settings)
        {
            _pendientes = pendientes;
            _sender = sender;
            _store = store;
            _settings = settings.Value;
        }

        public bool SmtpConfigurado => _sender.EstaConfigurado;
        public int HoraEnvio => _settings.HoraEnvio;

        public Task<List<WhatsAppAvisoDestino>> ListarPendientesAsync()
            => _pendientes.ListarPendientesAsync();

        public EmailCampanaEstado EstadoCampana() => _store.Leer();

        public void Programar(DateTime fecha) => _store.Programar(fecha);

        public void CancelarProgramacion() => _store.CancelarProgramacion();

        public bool ProgramadaPendienteParaHoy(DateTime ahora) => _store.ProgramadaPendienteParaHoy(ahora);

        public Task<List<EmailEnvioLog>> EnviarAsync(IEnumerable<int> idAlumnos, bool marcarCampana, CancellationToken ct = default)
            => EnviarInternoAsync(idAlumnos, marcarCampana, fechaProgramadaConsumida: null, ct);

        public async Task<List<EmailEnvioLog>> EnviarCampanaAsync(CancellationToken ct = default)
        {
            var destinos = (await ListarPendientesAsync()).Where(d => !d.SinCorreo).Select(d => d.IdAlumno);
            var fechaProg = _store.Leer().FechaProgramada;
            var logs = await EnviarInternoAsync(destinos, marcarCampana: true, fechaProg, ct);
            if (logs.Count == 0)
                _store.Registrar(Array.Empty<EmailEnvioLog>(), marcarCampana: true, fechaProg);
            return logs;
        }

        private async Task<List<EmailEnvioLog>> EnviarInternoAsync(
            IEnumerable<int> idAlumnos,
            bool marcarCampana,
            DateTime? fechaProgramadaConsumida,
            CancellationToken ct)
        {
            var ids = idAlumnos.ToHashSet();
            var destinos = (await ListarPendientesAsync())
                .Where(d => ids.Contains(d.IdAlumno) && !d.SinCorreo)
                .ToList();

            var logs = new List<EmailEnvioLog>();
            foreach (var d in destinos)
            {
                ct.ThrowIfCancellationRequested();
                var (ok, error) = await EnviarDestinoAsync(d);
                logs.Add(new EmailEnvioLog
                {
                    Fecha = DateTime.Now,
                    IdAlumno = d.IdAlumno,
                    NombreAlumno = d.NombreAlumno,
                    Correo = d.Correo,
                    Exito = ok,
                    Detalle = ok ? null : MensajeUsuarioHelper.ParaUsuario(error, "No se pudo enviar el correo."),
                    Canal = "email"
                });

                await Task.Delay(400, ct);
            }

            if (logs.Count > 0)
                _store.Registrar(logs, marcarCampana, fechaProgramadaConsumida);
            return logs;
        }

        private async Task<(bool Ok, string? Error)> EnviarDestinoAsync(WhatsAppAvisoDestino d)
        {
            var colegio = string.IsNullOrWhiteSpace(_settings.NombreColegio)
                ? _settings.FromName
                : _settings.NombreColegio;
            var asunto = EmailMensajeBuilder.Asunto(colegio, d);
            var html = EmailMensajeBuilder.Html(colegio, d);
            return await _sender.EnviarAsync(d.Correo!, asunto, html);
        }
    }
}
