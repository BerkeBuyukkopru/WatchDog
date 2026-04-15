using System;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    // Bu sınıf panel kısmına yönetici olarak girdiğinde cpu eşiği, yapay zeka çeşitleri, ram eşik değeri gibi şeyleri ayarlamak içindir.
    public class SystemConfiguration : BaseEntity<int>
    {
        public double CriticalCpuThreshold { get; set; } = 90.0;

        public double CriticalRamThreshold { get; set; } = 90.0;

        public double CriticalLatencyThreshold { get; set; } = 1000.0;

        // YENİ EKLENEN: UC-9 Arşivleme motorunun hangi ayda kaldığını hatırlamasını sağlayan hafıza
        public DateTime? LastArchivedDate { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}