using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Linq;
using Watchdog.Domain.Entities;
using Watchdog.Application.Interfaces.ExternalClients;
using MailKit.Net.Smtp;
using MimeKit;

namespace Watchdog.Infrastructure.Notifications
{
    public class MailSender : INotificationSender
    {
        private readonly ILogger<MailSender> _logger;
        private readonly MailSettings _settings;

        public MailSender(ILogger<MailSender> logger, IOptions<MailSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task SendDowntimeAlertAsync(Incident incident, MonitoredApp app)
        {
            await SendEmailAsync(_settings.ToEmail, 
                $"🚨 KRİTİK KESİNTİ: {app.Name}", 
                $"<h3>Sistem Çöktü!</h3><p><b>Uygulama:</b> {app.Name}</p><p><b>Hata:</b> {incident.ErrorMessage}</p><p><b>Zaman:</b> {incident.StartedAt:dd.MM.yyyy HH:mm:ss} (UTC)</p>");
        }

        public async Task SendRecoveryAlertAsync(Incident incident, MonitoredApp app)
        {
            await SendEmailAsync(_settings.ToEmail, 
                $"✅ SİSTEM KURTARILDI: {app.Name}", 
                $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #dff0d8; border-radius: 10px;'>
                        <h3 style='color: #3c763d;'>Sistem Tekrar Ayakta!</h3>
                        <p><b>Uygulama:</b> {app.Name}</p>
                        <p><b>Düzelme Zamanı:</b> {incident.ResolvedAt:dd.MM.yyyy HH:mm:ss} (UTC)</p>
                        <p>Sistem şu an sağlıklı yanıt veriyor.</p>
                        <hr style='border: 0; border-top: 1px solid #dff0d8;'>
                        <p style='font-size: 11px; color: #999;'>WatchDog Otomatik Bildirim Sistemi</p>
                    </div>");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                // MailHog veya yerel sunucularda SSL genellikle kapalıdır (false).
                await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl);

                // Eğer kullanıcı adı/şifre gerekliyse buraya eklenebilir. 
                // MailHog auth istemez.
                // await client.AuthenticateAsync(_settings.Username, _settings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation(">>> E-POSTA SMTP (MailKit) ÜZERİNDEN BAŞARIYLA GÖNDERİLDİ: {To}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("!!! SMTP GÖNDERİM HATASI: {Message}", ex.Message);
                
                // Hata durumunda loglarda içeriği görelim (Fallback)
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine($"[GÖNDERİLEMEYEN MAİL]");
                Console.WriteLine($"Alıcı: {toEmail}");
                Console.WriteLine($"Konu: {subject}");
                Console.WriteLine(new string('=', 50) + "\n");
            }
        }
    }
}