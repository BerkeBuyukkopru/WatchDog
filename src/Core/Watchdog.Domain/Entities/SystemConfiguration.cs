using System;

// Bu sınıf panel kısmına yönetici olarka girdiğinde cpu eşiği, yapay zeka çeşitleri, ram eşik değeri gibi şeyleri ayrlamak içindir.

namespace Watchdog.Domain.Entities
{
    public class SystemConfiguration
    {
        // Veritabanında her zaman 1. satırda kalması için varsayılan değer verildi.
        public int Id { get; set; } = 1;

        public string ActiveAiProvider { get; set; } = string.Empty;

        public string? AiApiUrl { get; set; }

        public string? AiApiKey { get; set; }

        public double CriticalCpuThreshold { get; set; } = 90.0;

        public double CriticalRamThreshold { get; set; } = 90.0;

        // Kurumsal takip için eklenen tarih damgası:
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}