using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.Ssl
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSslHealthCheck(this IServiceCollection services, string host, int daysBeforeExpiration = 15)
        {
            services.AddTransient<IHealthCheck>(serviceProvider =>
            {
                return new SslHealthCheck(host, daysBeforeExpiration);
            });
            return services;
        }
    }
}
