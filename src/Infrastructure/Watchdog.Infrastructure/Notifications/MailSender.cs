using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Notifications
{
    // Bildirim Gönderim Servisi. Infrastructure katmanında yer alarak dış dünya (SMTP) ile iletişim kurar.
    public class MailSender: INotificationSender
    {
        private readonly ILogger<MailSender> _logger;

        public MailSender(ILogger<MailSender> logger)
        {
            _logger = logger;
        }

        // Kesinti oluştuğunda (3-Strike sonrası) çalışır.
        public async Task SendDowntimeAlertAsync(Incident incident, MonitoredApp app)
        {
            string subject = $"🚨 KRİTİK KESİNTİ ALARMI: {app.Name} Yanıt Vermiyor!";
            string body = $"İzlenen sistemlerden '{app.Name}' ({app.HealthUrl}) adresine ulaşılamıyor veya üst üste 3 kez hata alındı (3-Strike).\nÇöküş Zamanı: {incident.StartedAt}\nHata Detayı: {incident.ErrorMessage}";

            await SendEmailAsync(subject, body);

            // Terminal üzerinden takibi kolaylaştırmak için Error seviyesinde log atıyoruz.
            _logger.LogError(">>> E-POSTA GÖNDERİLDİ: DOWNTIME ALERT -> {AppName}", app.Name);
        }

        //Sistem düzeldiğinde yöneticiye "Rahat uyu" mesajı gönderir.
        public async Task SendRecoveryAlertAsync(Incident incident, MonitoredApp app)
        {
            string subject = $"SİSTEM KURTARILDI: {app.Name} Yeniden Ayakta!";
            string body = $"Kesinti yaşayan '{app.Name}' ({app.HealthUrl}) uygulaması tekrar sağlıklı yanıt vermeye başlamıştır.\nÇözülme Zamanı: {incident.ResolvedAt}";

            await SendEmailAsync(subject, body);

            _logger.LogInformation(">>> E-POSTA GÖNDERİLDİ: RECOVERY -> {AppName}", app.Name);
        }

        private async Task SendEmailAsync(string subject, string body)
        {
            // Not: Gerçek SMTP (SmtpClient) kodları buraya gelecek. 
            await Task.CompletedTask;
        }
    }
}
