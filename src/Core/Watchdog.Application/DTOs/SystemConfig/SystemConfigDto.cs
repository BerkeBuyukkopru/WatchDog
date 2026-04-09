using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.SystemConfig
{
    public class SystemConfigDto
    {
        // YENİ MİMARİ: AI ayarları yeni eklenecek olan AiProviderDto'ya taşınacak.
        // Bu DTO artık sadece sistem alarm sınırlarını (Threshold) taşıyor.

        // Sistemin 'Degraded' (Sarı) alarm vermesi için gereken CPU eşiği (Örn: 90.0).
        public double CriticalCpuThreshold { get; set; }

        public double CriticalRamThreshold { get; set; }

        // Gecikme (Latency) sınırı (Entity'de vardı, UI'dan yönetilebilmesi için DTO'ya da ekledik)
        public double CriticalLatencyThreshold { get; set; }
    }
}
