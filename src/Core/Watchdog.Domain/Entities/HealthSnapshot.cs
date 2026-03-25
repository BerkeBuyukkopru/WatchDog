using System;
using Watchdog.Domain.Enums;

namespace Watchdog.Domain.Entities
{
    public class HealthSnapshot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppId { get; set; }
        public virtual MonitoredApp? App { get; set; }

        // C# uyarılarını gidermek için varsayılan değer atıyoruz
        public HealthStatus Status { get; set; }

        public long TotalDuration { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public double CpuUsage { get; set; }
        public double RamUsageMb { get; set; }
        public double FreeDiskGb { get; set; }

        // JSON detayı boş gelebileceği için ? (nullable) işareti koyuyoruz
        public string? DependencyDetails { get; set; }
    }
}