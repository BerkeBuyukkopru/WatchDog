using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.RabbitMQ;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMQHealthCheck(this IServiceCollection services, string connectionString)
    {
        services.AddTransient<IHealthCheck>(provider => new RabbitMQHealthCheck(connectionString));
        return services;
    }
}