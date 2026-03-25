using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Domain.Enums
{
    public enum HealthStatus
    {
        Healthy = 1,
        Degraded = 2,
        Unhealthy = 3
    }
}
