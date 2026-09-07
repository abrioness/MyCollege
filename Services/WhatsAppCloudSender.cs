using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WebColegio.Configuration;

namespace WebColegio.Services
{
    public interface IWhatsAppSender
    {
        bool EstaConfigurado { get; }
        Task<(bool Ok, string? Error)> EnviarTextoAsync(string telefonoE164, string mensaje, CancellationToken ct = default);
        Task<(bool Ok, string? Error)> EnviarPlantillaMoraAsync(
            string telefonoE164,
            string nombreContacto,
            string nombreAlumno,
            string detallePendiente,
            string total,
            CancellationToken ct = default);
    }

    public class WhatsAppCloudSender : IWhatsAppSender
    {
        private readonly WhatsAppSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public WhatsAppCloudSender(IOptions<WhatsAppSettings> settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public bool EstaConfigurado =>
            !string.IsNullOrWhiteSpace(_settings.AccessToken)
            && !string.IsNullOrWhiteSpace(_settings.PhoneNumberId);

        public Task<(bool Ok, string? Error)> EnviarTextoAsync(string telefonoE164, string mensaje, CancellationToken ct = default)
        {
            var body = new
            {
                messaging_product = "whatsapp",
                to = telefonoE164,
                type = "text",
                text = new { preview_url = false, body = mensaje }
            };
            return EnviarAsync(body, ct);
        }

        public Task<(bool Ok, string? Error)> EnviarPlantillaMoraAsync(
            string telefonoE164,
            string nombreContacto,
            string nombreAlumno,
            string detallePendiente,
            string total,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.TemplateName))
                return EnviarTextoAsync(telefonoE164, detallePendiente, ct);

            var body = new
            {
                messaging_product = "whatsapp",
                to = telefonoE164,
                type = "template",
                template = new
                {
                    name = _settings.TemplateName,
                    language = new { code = _settings.TemplateLanguage ?? "es" },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new object[]
                            {
                                new { type = "text", text = Truncar(nombreContacto, 60) },
                                new { type = "text", text = Truncar(nombreAlumno, 60) },
                                new { type = "text", text = Truncar(detallePendiente, 200) },
                                new { type = "text", text = Truncar(total, 40) }
                            }
                        }
                    }
                }
            };
            return EnviarAsync(body, ct);
        }

        private async Task<(bool Ok, string? Error)> EnviarAsync(object payload, CancellationToken ct)
        {
            if (!EstaConfigurado)
                return (false, "Configure WhatsApp:AccessToken y WhatsApp:PhoneNumberId en appsettings.");

            var url = $"https://graph.facebook.com/{_settings.ApiVersion.Trim('/')}/{_settings.PhoneNumberId}/messages";
            using var client = _httpClientFactory.CreateClient("WhatsAppGraph");
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var resp = await client.SendAsync(req, ct);
                var texto = await resp.Content.ReadAsStringAsync(ct);
                if (resp.IsSuccessStatusCode)
                    return (true, null);
                return (false, Truncar(texto, 400));
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static string Truncar(string? valor, int max)
        {
            var t = string.IsNullOrWhiteSpace(valor) ? "-" : valor.Trim();
            return t.Length <= max ? t : t[..max];
        }
    }
}
