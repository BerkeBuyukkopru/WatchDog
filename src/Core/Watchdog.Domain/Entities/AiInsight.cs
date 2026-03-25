using System;
using Watchdog.Domain.Enums;

namespace Watchdog.Domain.Entities
{
    public class AiInsight
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AppId { get; set; }

        public virtual MonitoredApp? App { get; set; }

        public InsightType InsightType { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Evidence { get; set; } = string.Empty;

        public bool IsResolved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}