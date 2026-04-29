using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Linq; // .Select() ve .ToArray() için gerekli
using Watchdog.Domain.Entities;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Infrastructure.Notifications
{
    public class MailSender : INotificationSender
    {
        private readonly ILogger<MailSender> _logger;
        private readonly MailSettings _settings;
        private readonly HttpClient _httpClient;

        public MailSender(ILogger<MailSender> logger, IOptions<MailSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _httpClient = new HttpClient();
        }

        // === 1. SİSTEM ÇÖKTÜĞÜNDE ÇALIŞAN METOT (DOWNTIME) ===
        public async Task SendDowntimeAlertAsync(Incident incident, MonitoredApp app)
        {
            _logger.LogWarning("===> [API] {AppName} için HTTP üzerinden DOWNTIME maili hazırlanıyor...", app.Name);

            // Alıcı yönetimi (Sadece varsayılan yöneticiye gönderilir)
            var targetEmails = new[] { _settings.ToEmail };

            // Mailtrap API beklediği JSON formatı
            var emailData = new
            {
                from = new { email = _settings.From, name = _settings.DisplayName },
                to = targetEmails.Select(e => new { email = e }).ToArray(),
                subject = $"🚨 KRİTİK KESİNTİ: {app.Name}",
                html = $"<h3>Sistem Çöktü!</h3><p><b>Uygulama:</b> {app.Name}</p><p><b>Hata:</b> {incident.ErrorMessage}</p><p><b>Zaman:</b> {incident.StartedAt:dd.MM.yyyy HH:mm:ss} (UTC)</p>"
            };

            await SendViaApiAsync(emailData, app.Name);
        }

        // === 2. SİSTEM DÜZELDİĞİNDE ÇALIŞAN METOT (RECOVERY) ===
        public async Task SendRecoveryAlertAsync(Incident incident, MonitoredApp app)
        {
            _logger.LogWarning("===> [API] {AppName} için HTTP üzerinden KURTARMA maili hazırlanıyor...", app.Name);

            var targetEmails = new[] { _settings.ToEmail };

            var emailData = new
            {
                from = new { email = _settings.From, name = _settings.DisplayName },
                to = targetEmails.Select(e => new { email = e }).ToArray(),
                subject = $"✅ SİSTEM KURTARILDI: {app.Name}",
                html = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #dff0d8; border-radius: 10px;'>
                        <h3 style='color: #3c763d;'>Sistem Tekrar Ayakta!</h3>
                        <p><b>Uygulama:</b> {app.Name}</p>
                        <p><b>Düzelme Zamanı:</b> {incident.ResolvedAt:dd.MM.yyyy HH:mm:ss} (UTC)</p>
                        <p>Sistem şu an sağlıklı yanıt veriyor.</p>
                        <hr style='border: 0; border-top: 1px solid #dff0d8;'>
                        <p style='font-size: 11px; color: #999;'>WatchDog Otomatik Bildirim Sistemi</p>
                    </div>"
            };

            await SendViaApiAsync(emailData, app.Name);
        }

        // === 3. GENEL MAİL GÖNDERİM METODU (Şifre Sıfırlama vb. için) ===
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            _logger.LogInformation("===> [API] {Email} adresi için özel mail (Örn: Şifre Sıfırlama) hazırlanıyor...", toEmail);

            // Mailtrap API'nin beklediği JSON formatı
            var emailData = new
            {
                from = new { email = _settings.From, name = _settings.DisplayName },
                to = new[] { new { email = toEmail } }, // Sadece parametreden gelen kişiye atar
                subject = subject,
                html = htmlMessage
            };

            // Mevcut motorunu kullanarak maili API'ye yolla
            await SendViaApiAsync(emailData, "Auth System");
        }

        // === 4. ORTAK API GÖNDERİM MOTORU ===
        private async Task SendViaApiAsync(object payload, string appName)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Mailtrap API Endpoint (InboxID appsettings'ten okunuyor)
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://sandbox.api.mailtrap.io/api/send/{_settings.InboxId}");

                // Hazırladığımız içeriği request'e bağlıyoruz
                request.Content = content;

                // API Token'ı header'a ekliyoruz (appsettings'ten okunuyor)
                request.Headers.Add("Api-Token", _settings.ApiToken);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(">>> E-POSTA API ÜZERİNDEN BAŞARIYLA GÖNDERİLDİ: {AppName}", appName);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("⚠️ MAILTRAP KOTA DOLDU: Günlük mail gönderim limitine ulaştınız. Mailler şu an gerçek adrese gitmiyor ancak terminale loglanıyor.");
                    // Opsiyonel: Mail içeriğini terminale basarak kullanıcının görmesini sağlayabiliriz.
                    Console.WriteLine($">>>> [MOCK-MAIL] {appName} için gönderilmek istenen mail kotalar nedeniyle gönderilemedi.");
                }
                else
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    _logger.LogCritical("!!! API HATASI: {Code} | Detay: {Detail}", response.StatusCode, errorDetail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("!!! HTTP GÖNDERİM HATASI: {Message}", ex.Message);
            }
        }
    }
}