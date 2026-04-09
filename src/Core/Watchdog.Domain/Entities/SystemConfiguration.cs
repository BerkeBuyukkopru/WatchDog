using System;

// Bu sınıf panel kısmına yönetici olarka girdiğinde cpu eşiği, yapay zeka çeşitleri, ram eşik değeri gibi şeyleri ayrlamak içindir.

namespace Watchdog.Domain.Entities
{
    public class SystemConfiguration
    {
        // Tek satırlık Singleton yapısı korunuyor.
        public int Id { get; set; } = 1;

        public double CriticalCpuThreshold { get; set; } = 90.0;

        public double CriticalRamThreshold { get; set; } = 90.0;

        public double CriticalLatencyThreshold { get; set; } = 1000.0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}