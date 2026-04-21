using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.AI
{
// Dashboard'a gönderilen veri (ApiKey gizli tutulur)
    public class AiProviderDto
    {
        public Guid Id { get; set; } // int'ten Guid'e güncellendi
        public string Name { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string? ApiUrl { get; set; } // Kullanıcı URL'i görüp düzenleyebilmeli
        public bool IsActive { get; set; }
    }
}
