using System;
using Watchdog.Domain.Common;
using Watchdog.Domain.Enums;

namespace Watchdog.Domain.Entities
{
    public class AiInsight : BaseEntity
    {
        public Guid AppId { get; set; }
        public Guid? AiProviderId { get; set; }
        public virtual AiProvider? AiProvider { get; set; }
        public virtual MonitoredApp? App { get; set; }
        public InsightType InsightType { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
        public bool IsResolved { get; set; } = false;
    }
}