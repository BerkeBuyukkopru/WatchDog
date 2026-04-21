using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.AI
{
    // Dashboard'dan güncelleme için gelen veri paketi
    public class UpdateAiProviderRequest
    {
        public Guid Id { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string? ApiUrl { get; set; }
        public string? ApiKey { get; set; } // Yeni API anahtarı buradan gelir
    }
}
