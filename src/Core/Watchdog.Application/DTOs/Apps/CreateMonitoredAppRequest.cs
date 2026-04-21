using System.ComponentModel.DataAnnotations;

namespace Watchdog.Application.DTOs.Apps
{
    public class CreateMonitoredAppRequest
    {
        [Required(ErrorMessage = "Uygulama adı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "İzlenecek URL zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string HealthUrl { get; set; } = string.Empty;

        [Range(10, 3600, ErrorMessage = "İzleme aralığı 10 saniye ile 1 saat arasında olmalıdır.")]
        public int PollingIntervalSeconds { get; set; } = 60;

        // BURADAKİ NotificationEmails ve AdminEmail SATIRLARI SİLİNDİ!
    }
}