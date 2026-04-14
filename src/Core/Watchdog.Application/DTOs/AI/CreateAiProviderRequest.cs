using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.AI
{
    // Yeni bir Yapay Zeka sağlayıcısı eklemek için kullanılan veri paketi.
    public class CreateAiProviderRequest
    {
        // Sağlayıcının görünen adı (Örn: 'OpenAI GPT-4')
        public string Name { get; set; } = string.Empty;

        // Teknik model ismi (Örn: 'gpt-4-turbo')
        public string ModelName { get; set; } = string.Empty;

        // API'nin uç noktası (Örn: 'https://api.openai.com/v1')
        public string? ApiUrl { get; set; }

        // Sisteme erişim için gizli anahtar
        public string? ApiKey { get; set; }
    }
}
