using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs
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
        public double CpuUsage { get; set; }
        public double RamUsage { get; set; }
        public double FreeDiskGb { get; set; }
        // DependencyDetails (ham JSON) kısmını dışarıya basmıyoruz, arayüzü kirletmeye gerek yok.
    }
}
