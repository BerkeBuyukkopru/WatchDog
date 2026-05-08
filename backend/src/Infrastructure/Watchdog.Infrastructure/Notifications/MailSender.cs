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

        public async Task SendDowntimeAlertAsync(string toEmail, Incident incident, MonitoredApp app)
        {
            var statusTableHtml = GenerateStatusTableHtml(incident.ErrorMessage);

            string htmlBody = $@"
                <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f4f7fa; padding: 30px; color: #334155;'>
                    <div style='max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);'>
                        
                        <div style='padding: 25px; border-bottom: 1px solid #f1f5f9;'>
                            <table style='width: 100%;'>
                                <tr>
                                    <td>
                                        <h1 style='margin: 0; color: #e11d48; font-size: 18px; font-weight: 700; letter-spacing: -0.025em;'>WatchDog Bildirimi</h1>
                                    </td>
                                    <td style='text-align: right;'>
                                        <span style='background-color: #fff1f2; color: #e11d48; padding: 4px 10px; border-radius: 4px; font-size: 11px; font-weight: 700; text-transform: uppercase;'>Kritik</span>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div style='padding: 30px;'>
                            <p style='margin: 0 0 20px 0; font-size: 16px; line-height: 1.5;'>
                                Merhaba, <br><br>
                                <strong>{app.Name}</strong> uygulamasında bir kesinti tespit edildi. Sistem şu an çevrimdışı durumda ve müdahale bekleniyor.
                            </p>

                            <div style='background-color: #f8fafc; border-radius: 6px; padding: 20px; margin-bottom: 30px;'>
                                <table style='width: 100%; border-collapse: collapse; font-size: 13px;'>
                                    <tr>
                                        <td style='color: #64748b; padding-bottom: 8px;'>Uygulama:</td>
                                        <td style='color: #1e293b; font-weight: 600; padding-bottom: 8px; text-align: right;'>{app.Name}</td>
                                    </tr>
                                    <tr>
                                        <td style='color: #64748b; padding-bottom: 8px;'>Başlangıç:</td>
                                        <td style='color: #1e293b; font-weight: 600; padding-bottom: 8px; text-align: right;'>{incident.StartedAt:dd.MM.yyyy HH:mm:ss} (UTC)</td>
                                    </tr>
                                    <tr>
                                        <td style='color: #64748b;'>Durum:</td>
                                        <td style='color: #e11d48; font-weight: 700; text-align: right;'>Çevrimdışı (Kesinti)</td>
                                    </tr>
                                </table>
                            </div>

                            <h3 style='font-size: 14px; color: #1e293b; margin-bottom: 15px; border-bottom: 1px solid #f1f5f9; padding-bottom: 8px;'>Bileşen Özet Raporu</h3>
                            {statusTableHtml}
                        </div>

                        <div style='padding: 20px; background-color: #f8fafc; text-align: center; border-top: 1px solid #f1f5f9;'>
                            <p style='margin: 0; color: #94a3b8; font-size: 11px;'>
                                Bu e-posta WatchDog Otomatik İzleme Sistemi tarafından gönderilmiştir.<br>
                                &copy; 2026 WatchDog AI Monitoring
                            </p>
                        </div>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, 
                $"🚨 KRİTİK KESİNTİ: {app.Name}", 
                htmlBody);
        }

        private string GenerateStatusTableHtml(string jsonDetails)
        {
            try 
            {
                var details = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonDetails);
                if (details == null) return "<p style='color: #e11d48;'>Hata detayı okunamadı.</p>";

                var sb = new StringBuilder();
                bool hasIssues = false;

                foreach (var item in details)
                {
                    string status = "Unknown";
                    string description = string.Empty;

                    if (item.Value.ValueKind == JsonValueKind.Object)
                    {
                        if (item.Value.TryGetProperty("status", out var s)) status = s.GetString() ?? "Unknown";
                        if (item.Value.TryGetProperty("description", out var d)) description = d.GetString() ?? string.Empty;
                    }

                    // SADECE SORUNLU BİLEŞENLERİ GÖSTER (Healthy olanları gizle)
                    if (status.Equals("Healthy", StringComparison.OrdinalIgnoreCase)) continue;

                    if (!hasIssues)
                    {
                        sb.Append("<table style='width: 100%; border-collapse: collapse;'>");
                        hasIssues = true;
                    }

                    string statusColor = "#e11d48"; // Kritik kırmızı

                    sb.Append($@"
                        <tr style='border-bottom: 1px solid #f1f5f9;'>
                            <td style='padding: 15px 0;'>
                                <div style='color: #1e293b; font-size: 14px; font-weight: 700;'>{item.Key}</div>
                                {(!string.IsNullOrEmpty(description) ? $"<div style='color: #64748b; font-size: 12px; margin-top: 4px;'>{description}</div>" : "")}
                            </td>
                            <td style='padding: 15px 0; text-align: right; vertical-align: top;'>
                                <span style='font-size: 11px; font-weight: 800; color: {statusColor}; text-transform: uppercase;'>
                                    {status}
                                </span>
                            </td>
                        </tr>");
                }
                
                if (!hasIssues)
                {
                    return "<p style='color: #059669; font-size: 13px; font-weight: 500;'>Tüm alt bileşenler şu an sağlıklı yanıt veriyor.</p>";
                }

                sb.Append("</table>");
                return sb.ToString();
            }
            catch 
            {
                return $"<div style='color: #e11d48; font-family: monospace; font-size: 12px; background: #fef2f2; padding: 10px; border: 1px solid #fee2e2;'>{jsonDetails}</div>";
            }
        }

        public async Task SendRecoveryAlertAsync(string toEmail, Incident incident, MonitoredApp app)
        {
            string htmlBody = $@"
                <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f4f7fa; padding: 30px; color: #334155;'>
                    <div style='max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);'>
                        
                        <div style='padding: 25px; border-bottom: 1px solid #f1f5f9;'>
                            <table style='width: 100%;'>
                                <tr>
                                    <td>
                                        <h1 style='margin: 0; color: #059669; font-size: 18px; font-weight: 700; letter-spacing: -0.025em;'>WatchDog Bildirimi</h1>
                                    </td>
                                    <td style='text-align: right;'>
                                        <span style='background-color: #ecfdf5; color: #059669; padding: 4px 10px; border-radius: 4px; font-size: 11px; font-weight: 700; text-transform: uppercase;'>Kurtarıldı</span>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div style='padding: 30px;'>
                            <p style='margin: 0 0 20px 0; font-size: 16px; line-height: 1.5;'>
                                Harika haber! <br><br>
                                <strong>{app.Name}</strong> uygulaması yapılan sağlık kontrollerine tekrar olumlu yanıt vermeye başladı. Tüm servisler sağlıklı duruma döndü.
                            </p>

                            <div style='background-color: #f0fdf4; border: 1px solid #dcfce7; border-radius: 6px; padding: 20px; margin-bottom: 30px;'>
                                <table style='width: 100%; border-collapse: collapse; font-size: 13px;'>
                                    <tr>
                                        <td style='color: #64748b; padding-bottom: 8px;'>Uygulama:</td>
                                        <td style='color: #1e293b; font-weight: 600; padding-bottom: 8px; text-align: right;'>{app.Name}</td>
                                    </tr>
                                    <tr>
                                        <td style='color: #64748b; padding-bottom: 8px;'>Düzelme Zamanı:</td>
                                        <td style='color: #1e293b; font-weight: 600; padding-bottom: 8px; text-align: right;'>{incident.ResolvedAt:dd.MM.yyyy HH:mm:ss} (UTC)</td>
                                    </tr>
                                    <tr>
                                        <td style='color: #64748b;'>Yeni Durum:</td>
                                        <td style='color: #059669; font-weight: 700; text-align: right;'>Çevrimiçi (Sağlıklı)</td>
                                    </tr>
                                </table>
                            </div>

                            <p style='color: #64748b; font-size: 14px; line-height: 1.6; margin-bottom: 0;'>
                                Sistem şu an tam kapasiteyle çalışıyor. İzleme süreci devam etmektedir.
                            </p>
                        </div>

                        <div style='padding: 20px; background-color: #f8fafc; text-align: center; border-top: 1px solid #f1f5f9;'>
                            <p style='margin: 0; color: #94a3b8; font-size: 11px;'>
                                Bu e-posta WatchDog Otomatik İzleme Sistemi tarafından gönderilmiştir.<br>
                                &copy; 2026 WatchDog AI Monitoring
                            </p>
                        </div>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, 
                $"✅ SİSTEM KURTARILDI: {app.Name}", 
                htmlBody);
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