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
        public string? MaskedApiKey { get; set; }
        public bool IsActive { get; set; }
        public bool HasApiKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

    public class AiProviderSecretDto
    {
        public string? ApiKey { get; set; }
    }
}
