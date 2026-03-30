using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs
{
    public class SystemConfigDto
    {
        // Hangi AI motorunun (Ollama, OpenAI, Claude vb.) aktif olduğunu belirler.
        public string ActiveAiProvider { get; set; } = string.Empty;

        // Yerel Ollama URL'i veya OpenAI API uç noktası.
        public string? AiApiUrl { get; set; }

        // API Key hassas bir veri olduğu için nullable (?) tanımlanmış.
        public string? AiApiKey { get; set; }

        // Sistemin 'Degraded' (Sarı) alarm vermesi için gereken CPU eşiği (Örn: 90.0).
        public double CriticalCpuThreshold { get; set; }

        public double CriticalRamThreshold { get; set; }
    }
}
