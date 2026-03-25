using System;

namespace Watchdog.Domain.Entities
{
    public class Incident
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AppId { get; set; }

        public virtual MonitoredApp? App { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
    }
}