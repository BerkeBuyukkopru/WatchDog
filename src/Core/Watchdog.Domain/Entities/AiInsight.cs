using System;

namespace Watchdog.Domain.Entities
{
    public class AiInsight
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AppId { get; set; }

        public string InsightType { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Evidence { get; set; } = string.Empty;

        public bool IsResolved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}