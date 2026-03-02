using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Tcp
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTcpHealthCheck(this IServiceCollection services, string host, int port, string? payload = null, string? response = null)
        {
            services.AddTransient<IHealthCheck>(serviceProvider =>
            {
                return new TcpHealthCheck(host, port, payload, response);
            });
            return services;
        }
    }
}
