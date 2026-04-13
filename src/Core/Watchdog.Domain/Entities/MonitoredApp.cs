using System;

namespace Watchdog.Domain.Entities
{
    /// <summary>
    /// İzlenen uygulamaların temel bilgilerini ve durumunu tutan ana sınıf.
    /// </summary>
    public class MonitoredApp
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Uygulamanın görünen adı
        public string Name { get; set; } = string.Empty;

        // Sağlık kontrolü yapılacak olan uç nokta (URL)
        public string HealthUrl { get; set; } = string.Empty;

        // Kontrol sıklığı (saniye cinsinden)
        public int PollingIntervalSeconds { get; set; }

        // Bildirim gidecek e-posta adresleri (Virgülle ayrılmış: "mail1@test.com,mail2@test.com")
        public string? NotificationEmails { get; set; }

        // Diğer projelerin bu uygulamaya veri göndermesi için gereken anahtar
        public string? ApiKey { get; set; }

        // Kaydın oluşturulma tarihi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Soft Delete (Yumuşak Silme) bayrağı. 
        // Veritabanından fiziksel silme yerine bu alanı false yaparak pasife çekiyoruz.
        public bool IsActive { get; set; } = true;
    }
}