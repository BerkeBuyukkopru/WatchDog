using System;
using System.Collections.Generic;
using System.Text;
using HealthChecks.Abstractions.Enums;

namespace HealthChecks.Abstractions
{
    public class HealthCheckResult
    {
        // Kontrolün durumu (Healthy, Unhealthy vb.)
        public HealthStatus Status { get; set; }

        // Teknik detay veya kullanıcı dostu açıklama mesajı
        public string Description { get; set; } = string.Empty;

        // Kontrolün toplam ne kadar sürdüğü (Performans takibi için kritik)
        public TimeSpan Duration { get; set; }

        // Eğer bir hata (Exception) fırladıysa, hatanın kendisi
        public Exception? Exception { get; set; }

        // WDG014 Kuralı: Donanım metriklerini (CPU, RAM vb.) taşıyacak veri sözlüğü
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        // Kullanımı kolaylaştıran yardımcı metodlar
        public static HealthCheckResult Healthy(string description = "Healthy")
            => new() { Status = HealthStatus.Healthy, Description = description };

        public static HealthCheckResult Unhealthy(string description, Exception? exception = null)
            => new() { Status = HealthStatus.Unhealthy, Description = description, Exception = exception };
        public static HealthCheckResult Degraded(string description, Dictionary<string, object>? data = null)
            => new() { Status = HealthStatus.Degraded, Description = description, Data = data ?? new() };
    }
}