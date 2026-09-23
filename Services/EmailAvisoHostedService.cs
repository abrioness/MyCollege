using Microsoft.Extensions.Options;
using WebColegio.Configuration;

namespace WebColegio.Services
{
    public class EmailAvisoHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailAvisoHostedService> _logger;
        private readonly EmailSettings _settings;

        public EmailAvisoHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<EmailSettings> settings,
            ILogger<EmailAvisoHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await IntentarCampanaAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error en la campaña programada de correo.");
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task IntentarCampanaAsync(CancellationToken ct)
        {
            var ahora = DateTime.Now;
            if (ahora.Hour < _settings.HoraEnvio)
                return;

            using var scope = _scopeFactory.CreateScope();
            var avisos = scope.ServiceProvider.GetRequiredService<EmailAvisoService>();
            if (!avisos.ProgramadaPendienteParaHoy(ahora))
                return;

            if (!avisos.SmtpConfigurado)
            {
                _logger.LogWarning("Campaña de correo omitida: falta configurar SMTP.");
                return;
            }

            var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
            var tokens = scope.ServiceProvider.GetRequiredService<IApiTokenAccessor>();
            tokens.SetToken(jwt.CreateToken(new Models.TblUsuarios
            {
                IdUsuario = 0,
                IdRol = 1,
                NombreUsuario = "email-job",
                NombreCompleto = "Campaña correo"
            }, "Admin"));

            _logger.LogInformation("Iniciando campaña de correo de moras del {Fecha}.", ahora.ToString("yyyy-MM-dd"));
            var logs = await avisos.EnviarCampanaAsync(ct);
            var ok = logs.Count(l => l.Exito);
            _logger.LogInformation("Campaña de correo terminada: {Ok}/{Total} enviados.", ok, logs.Count);
        }
    }
}
