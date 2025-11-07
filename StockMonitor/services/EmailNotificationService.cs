using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMonitor.Interfaces;
using StockMonitor.Settings;

namespace StockMonitor.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly SmtpSettings _settings;

        // Pedimos as configurações (SmtpSettings) via Injeção de Dependência
        public EmailNotificationService(IOptions<SmtpSettings> settings, ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
            _settings = settings.Value; // Pega o objeto de configuração
        }

        public async Task SendNotificationAsync(string subject, string body)
        {
            if (string.IsNullOrEmpty(_settings.SmtpSenha))
            {
                _logger.LogError("Erro: O segredo 'SmtpSettings:SmtpSenha' não foi configurado.");
                return;
            }

            try
            {
                using SmtpClient client = new SmtpClient(_settings.SmtpServidor, _settings.SmtpPorta);
                client.Credentials = new NetworkCredential(_settings.SmtpUsuario, _settings.SmtpSenha);
                client.EnableSsl = true;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_settings.SmtpUsuario);
                mail.To.Add(_settings.EmailDestino);
                mail.Subject = subject;
                mail.Body = body;

                await client.SendMailAsync(mail); 

                _logger.LogInformation("Alerta enviado por e-mail para {Destino}.", _settings.EmailDestino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail.");
            }
        }
    }
}