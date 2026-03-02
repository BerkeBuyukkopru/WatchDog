using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Heartbeat;

public static class DependencyInjection
{
    // Bekçi fişi takarken defterin nasıl okunacağını (Func) ve kaç dakika bekleyeceğini (tolerance) söyler
    public static IServiceCollection AddHeartbeatHealthCheck(
        this IServiceCollection services,
        Func<Task<DateTime?>> getLastPulseAsync,
        TimeSpan tolerance)
    {
        services.AddTransient<IHealthCheck>(provider => new HeartbeatHealthCheck(getLastPulseAsync, tolerance));
        return services;
    }
}