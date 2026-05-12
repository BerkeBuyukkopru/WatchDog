using System;

namespace Watchdog.Application.Interfaces.Monitoring
{
    public class CentralSystemMetricsDto
    {
        public double SystemCpu { get; set; }
        public double SystemRam { get; set; }
        public double FreeDiskGb { get; set; }
        public double TotalDiskGb { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
