using System;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    // İzlenen uygulamaların temel bilgilerini ve durumunu tutan ana sınıf.
    public class MonitoredApp : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string HealthUrl { get; set; } = string.Empty;
        public int PollingIntervalSeconds { get; set; }
        public string? NotificationEmails { get; set; }
        public string? ApiKey { get; set; }

        // İŞ MANTIĞI: İzleme aktif mi pasif mi? (Silme ile karıştırılmamalı)
        public bool IsActive { get; set; } = true;
    }
}