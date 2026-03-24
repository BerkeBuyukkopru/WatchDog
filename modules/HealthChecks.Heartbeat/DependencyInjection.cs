using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Heartbeat;

public static class DependencyInjection
{
    // Bekçi fişi takarken defterin nasıl okunacağını (Func) ve kaç dakika bekleyeceğini (tolerance) söyler
    public static IServiceCollection AddHeartbeatHealthCheck(
        this IServiceCollection services,
        Func<CancellationToken, Task<DateTime?>> getLastPulseAsync, // CancellationToken eklendi
        TimeSpan tolerance)
    {
        services.AddTransient<IHealthCheck>(provider => new HeartbeatHealthCheck(getLastPulseAsync, tolerance));
        return services;
    }
}