using System;

// Bu sınıf panel kısmına yönetici olarka girdiğinde cpu eşiği, yapay zeka çeşitleri, ram eşik değeri gibi şeyleri ayrlamak içindir.

namespace Watchdog.Domain.Entities
{
    public class SystemConfiguration
    {
        // Veritabanında her zaman 1. satırda kalması için varsayılan değer verildi.
        public int Id { get; set; } = 1;

        public string ActiveAiProvider { get; set; } = string.Empty;

        // Hangi modelin (gpt-4o-mini, phi3, llama3 vb.) çalışacağını tutar
        public string AiModelName { get; set; } = "phi3";

        public string? AiApiUrl { get; set; }

        public string? AiApiKey { get; set; }

        public double CriticalCpuThreshold { get; set; } = 90.0;

        public double CriticalRamThreshold { get; set; } = 90.0;

        // YENİ: Gecikme (Latency) sınırı da artık Dashboard'dan yönetilebilir:
        public double CriticalLatencyThreshold { get; set; } = 1000.0;

        // Kurumsal takip için eklenen tarih damgası:
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}