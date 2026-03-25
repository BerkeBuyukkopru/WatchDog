using System;

namespace Watchdog.Domain.Entities
{
    public class SystemConfiguration
    {
        public int Id { get; set; } = 1;

        public string ActiveAiProvider { get; set; } = string.Empty;

        public string? AiApiUrl { get; set; }

        public string? AiApiKey { get; set; }

        public double CriticalCpuThreshold { get; set; } = 90.0;

        public double MaxRamThresholdMb { get; set; } = 2048.0;
    }
}