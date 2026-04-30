using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.AI
{
    public class AiInsightDto
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public string AppName { get; set; } = string.Empty; // Join ile uygulama adını buraya basacağız
        public string Message { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
        public string InsightType { get; set; } = string.Empty; // Enum'ı string'e çeviriyoruz
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
