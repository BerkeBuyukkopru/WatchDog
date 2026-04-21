using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.DTOs
{
    // Ping atıldıktan sonra Use Case'e dönecek olan sonuç paketi
    public class ProbeResult
    {
        public HealthStatus Status { get; set; }
        public long DurationMilliseconds { get; set; }
        public string? JsonContent { get; set; }
    }
}
