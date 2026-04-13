using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    // Yapay zeka sağlayıcılarının (OpenAI, Ollama vb.) bilgilerini tutan entity.
    // Kurumsal standart gereği Guid (UUID) kimlik yapısı kullanılmıştır.
    public class AiProvider : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string? ApiUrl { get; set; }
        public string? ApiKey { get; set; }
        public bool IsActive { get; set; }
    }
}
