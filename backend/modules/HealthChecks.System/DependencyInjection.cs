using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.System
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSystemHealthChecks(
            this IServiceCollection services,
            double serverCpuThreshold = 90.0,
            double appCpuThreshold = 90.0,
            float minServerAvailableMb = 1024f,
            float maxAppAllocatedMb = 1024f,
            float minFreeSpaceGb = 5f)

        {
            services.AddTransient<IHealthCheck>(sp => new CpuHealthCheck(serverCpuThreshold, appCpuThreshold));
            services.AddTransient<IHealthCheck>(sp => new RamHealthCheck(minServerAvailableMb, maxAppAllocatedMb));
            services.AddTransient<IHealthCheck>(sp => new StorageHealthCheck(minFreeSpaceGb));

            return services;
        }
    }
}