using System;
using Watchdog.Domain.Common;

// Bu sınıf panel kısmına yönetici olarka girdiğinde cpu eşiği, yapay zeka çeşitleri, ram eşik değeri gibi şeyleri ayrlamak içindir.

namespace Watchdog.Domain.Entities
{
    public class SystemConfiguration : BaseEntity<int>
    {
        public SystemConfiguration() => Id = 1;
        public double CriticalCpuThreshold { get; set; } = 90.0;
        public double CriticalRamThreshold { get; set; } = 90.0;
        public double CriticalLatencyThreshold { get; set; } = 1000.0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}