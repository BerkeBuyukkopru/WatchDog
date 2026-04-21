using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HealthChecks.Tcp
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTcpHealthCheck(this IServiceCollection services, Func<string> hostProvider, Func<int> portProvider, string? payload = null, string? response = null)
        {
            services.AddTransient<IHealthCheck>(serviceProvider =>
            {
                return new TcpHealthCheck(hostProvider, portProvider, payload, response);
            });
            return services;
        }
    }
}