using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HealthChecks.Ssl
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSslHealthCheck(this IServiceCollection services, Func<string> hostProvider, int daysBeforeExpiration = 15)
        {
            services.AddTransient<IHealthCheck>(serviceProvider =>
            {
                return new SslHealthCheck(hostProvider, daysBeforeExpiration);
            });
            return services;
        }
    }
}