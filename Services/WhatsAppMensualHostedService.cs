using Microsoft.Extensions.Options;
using WebColegio.Configuration;

namespace WebColegio.Services
{
    public class WhatsAppMensualHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WhatsAppMensualHostedService> _logger;
        private readonly WhatsAppSettings _settings;

        public WhatsAppMensualHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppMensualHostedService> logger)
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
                    _logger.LogWarning(ex, "Error en la campaña automática de WhatsApp.");
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private async Task IntentarCampanaAsync(CancellationToken ct)
        {
            if (!_settings.EnvioAutomatico)
                return;

            var ahora = DateTime.Now;
            if (ahora.Day != 1 || ahora.Hour < _settings.HoraEnvio)
                return;

            using var scope = _scopeFactory.CreateScope();
            var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
            var tokens = scope.ServiceProvider.GetRequiredService<IApiTokenAccessor>();
            tokens.SetToken(jwt.CreateToken(new Models.TblUsuarios
            {
                IdUsuario = 0,
                IdRol = 1,
                NombreUsuario = "whatsapp-job",
                NombreCompleto = "Campaña WhatsApp"
            }, "Admin"));

            var avisos = scope.ServiceProvider.GetRequiredService<WhatsAppAvisoService>();
            if (avisos.CampanaDelMesYaEnviada())
                return;
            if (!avisos.ApiConfigurada)
            {
                _logger.LogWarning("Campaña WhatsApp del día 1 omitida: falta AccessToken o PhoneNumberId.");
                return;
            }

            _logger.LogInformation("Iniciando campaña WhatsApp de moras del {Fecha}.", ahora.ToString("yyyy-MM-dd"));
            var logs = await avisos.EnviarCampanaAsync(ct);
            var ok = logs.Count(l => l.Exito);
            _logger.LogInformation("Campaña WhatsApp terminada: {Ok}/{Total} enviados.", ok, logs.Count);
        }
    }
}
