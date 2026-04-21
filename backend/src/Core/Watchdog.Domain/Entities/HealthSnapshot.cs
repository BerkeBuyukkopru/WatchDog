using System;
using System.Text.Json.Serialization; // 1. BU SATIRI EKLEYİN
using Watchdog.Domain.Common;
using Watchdog.Domain.Enums;

namespace Watchdog.Domain.Entities
{
    public class HealthSnapshot : BaseEntity
    {
        public Guid AppId { get; set; }
        [JsonIgnore]
        public virtual MonitoredApp? App { get; set; }
        public HealthStatus Status { get; set; }
        public long TotalDuration { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double CpuUsage { get; set; }
        public double RamUsage { get; set; }
        public double FreeDiskGb { get; set; }
        public string? DependencyDetails { get; set; }
    }
}