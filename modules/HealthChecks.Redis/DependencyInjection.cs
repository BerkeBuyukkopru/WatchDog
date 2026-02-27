using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddRedisHealthCheck(this IServiceCollection services, string connectionString)
    {
        services.AddTransient<IHealthCheck>(provider => new RedisHealthCheck(connectionString));
        return services;
    }
}