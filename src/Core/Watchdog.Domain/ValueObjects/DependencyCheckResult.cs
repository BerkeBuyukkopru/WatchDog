using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Enums;

namespace Watchdog.Domain.ValueObjects
{
    public record DependencyCheckResult
    {
        // Kontrol edilen alt bağımlılığın adı (Örn: "Redis_Cache", "SqlServer_Main")
        public string TargetName { get; init; } = string.Empty;

        // O anki sağlık durumu (Healthy, Degraded, Unhealthy)
        public HealthStatus Status { get; init; }

        // Bu hedefe atılan ping/sorgu işleminin milisaniye cinsinden süresi
        public double DurationMilliseconds { get; init; }

        // Eğer bir hata varsa (Timeout, Connection Refused vb.) hatanın detayı
        public string? ErrorMessage { get; init; }

        // Donanım veya servise özel ekstra metrikler (Örn: { "FreeSpaceGB": 45 })
        public Dictionary<string, object>? ExtraMetrics { get; init; }
    }
}
