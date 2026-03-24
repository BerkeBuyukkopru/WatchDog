using System;
using System.Collections.Generic;
using HealthChecks.Abstractions.Enums;

namespace HealthChecks.Abstractions
{
    public class HealthCheckResult
    {
        public HealthStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public Exception? Exception { get; set; }

        // WDG014 Kuralı: Donanım metriklerini (CPU, RAM vb.) taşıyacak veri sözlüğü
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        // Kullanımı kolaylaştıran yardımcı metodlar
        public static HealthCheckResult Healthy(string description = "Healthy", Dictionary<string, object>? data = null)
            => new() { Status = HealthStatus.Healthy, Description = description, Data = data ?? new Dictionary<string, object>() };

        // Sistemin çalıştığı ama yavaşladığı durumlar için (Örn: CPU %90)
        public static HealthCheckResult Degraded(string description, Exception? exception = null, Dictionary<string, object>? data = null)
            => new() { Status = HealthStatus.Degraded, Description = description, Exception = exception, Data = data ?? new Dictionary<string, object>() };

        public static HealthCheckResult Unhealthy(string description, Exception? exception = null, Dictionary<string, object>? data = null)
            => new() { Status = HealthStatus.Unhealthy, Description = description, Exception = exception, Data = data ?? new() };
    }
}