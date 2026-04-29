using System;

namespace Watchdog.Application.DTOs.Monitoring
{
    public class IncidentDto
    {
        public Guid Id { get; set; }
        public string AppName { get; set; } = string.Empty;
        public string FailedComponent { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public bool IsResolved => ResolvedAt.HasValue;
    }
}
