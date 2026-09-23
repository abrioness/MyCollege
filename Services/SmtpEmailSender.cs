using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using WebColegio.Configuration;

namespace WebColegio.Services
{
    public interface IEmailSender
    {
        bool EstaConfigurado { get; }
        Task<(bool Ok, string? Error)> EnviarAsync(string destinatario, string asunto, string cuerpoHtml);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public bool EstaConfigurado =>
            !string.IsNullOrWhiteSpace(_settings.Host)
            && !string.IsNullOrWhiteSpace(_settings.From)
            && _settings.Port > 0
            && !_settings.Host.Contains("ejemplo.com", StringComparison.OrdinalIgnoreCase)
            && !_settings.From.Contains("ejemplo.com", StringComparison.OrdinalIgnoreCase);

        public async Task<(bool Ok, string? Error)> EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            if (!EstaConfigurado)
                return (false, "SMTP no configurado. Complete Email:Host y Email:From.");

            if (string.IsNullOrWhiteSpace(destinatario))
                return (false, "El destinatario no tiene correo.");

            try
            {
                using var mensaje = new MailMessage
                {
                    From = new MailAddress(_settings.From.Trim(), _settings.FromName ?? string.Empty),
                    Subject = asunto,
                    Body = cuerpoHtml,
                    IsBodyHtml = true
                };
                mensaje.To.Add(destinatario.Trim());
                if (_settings.NoReply)
                {
                    mensaje.ReplyToList.Clear();
                    mensaje.ReplyToList.Add(new MailAddress(_settings.From.Trim(), "No responder"));
                    mensaje.Headers.Add("Auto-Submitted", "auto-generated");
                    mensaje.Headers.Add("X-Auto-Response-Suppress", "All");
                }

                using var client = new SmtpClient(_settings.Host.Trim(), _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                if (!string.IsNullOrWhiteSpace(_settings.User))
                    client.Credentials = new NetworkCredential(_settings.User, _settings.Password ?? string.Empty);

                await client.SendMailAsync(mensaje);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
