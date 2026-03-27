using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs
{
    public class SystemConfigDto
    {
        public string ActiveAiProvider { get; set; } = string.Empty;
        public string? AiApiUrl { get; set; }
        public string? AiApiKey { get; set; }
        public double CriticalCpuThreshold { get; set; }
        // Ekip arkadaşının koyduğu isme birebir uyduk:
        public double CriticalRamThreshold { get; set; }
    }
}
