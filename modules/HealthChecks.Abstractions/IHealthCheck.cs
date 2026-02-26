using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.Abstractions
{
    internal interface IHealthCheck
    {
        // Modülün ayırt edici adı (Örn: "SqlServer_Watchdog", "Main_API_Ping")
        string Name { get; }

        // Asıl sağlık kontrolü işlemini gerçekleştiren asenkron metot
        Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
