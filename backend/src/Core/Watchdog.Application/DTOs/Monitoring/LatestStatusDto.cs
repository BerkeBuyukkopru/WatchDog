using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Monitoring
{
    // Swagger'daki o çirkin 0'lardan ve Enum sayılarından arındırılmış, 
    // React'in tam olarak beklediği "temiz" veri modeli.
    public class LatestStatusDto
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public string AppName { get; set; }
        public string Status { get; set; } // "1" veya "3" yerine "Healthy" veya "Unhealthy" yazacak
        public long TotalDuration { get; set; }
        public DateTime Timestamp { get; set; }
        public double AppCpuUsage { get; set; }
        public double SystemCpuUsage { get; set; }
        public double AppRamUsage { get; set; }
        public double SystemRamUsage { get; set; }
        public double FreeDiskGb { get; set; }
        public string? DependencyDetails { get; set; }
        public double TotalRamMb { get; set; }
        public double TotalCpuPercentage { get; set; }
        public double TotalDiskGb { get; set; }
        public int TotalCpuCores { get; set; }
    }
}
