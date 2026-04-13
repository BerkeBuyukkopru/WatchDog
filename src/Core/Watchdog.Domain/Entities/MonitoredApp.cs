using System;

//Panelde görülen url, uygulama ismi, ve tek seferlik bir apı key gösterilir karşı projeye göndermek için.

namespace Watchdog.Domain.Entities
{
    public class MonitoredApp
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Uyarıları gidermek için = string.Empty; ekledik
        public string Name { get; set; } = string.Empty;
        public string HealthUrl { get; set; } = string.Empty;
        public int PollingIntervalSeconds { get; set; }

        // Bildirim gidecek kişilerin listesi (Virgülle ayrılmış)
        public string? NotificationEmails { get; set; }

        // ApiKey başlangıçta boş olabileceği için nullable (?) yapıyoruz
        public string? ApiKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true; // Uygulama oluşturulduğunda varsayılan olarak her zaman aktiftir.
    }
}