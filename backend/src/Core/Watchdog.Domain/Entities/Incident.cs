using System;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    public class Incident : BaseEntity
    {
        public Guid AppId { get; set; }
        public virtual MonitoredApp? App { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public string FailedComponent { get; set; } = "System"; // Hangi bileşen patladı? (SQL, Redis, System vb.)
        public string ErrorMessage { get; set; } = string.Empty;
    }
}