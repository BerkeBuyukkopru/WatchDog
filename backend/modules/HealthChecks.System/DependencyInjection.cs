using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace HealthChecks.System
{
    public static class DependencyInjection
    {
        // 1. Sensörlerin Kaydedildiği Yer (Orijinal Kod)
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

        // 2. YENİ OTONOM YETENEK: Kütüphanenin kendi API Uç Noktası (Endpoint)
        public static IEndpointConventionBuilder MapWatchdogHealthChecks(this IEndpointRouteBuilder endpoints, string pattern = "/health")
        {
            return endpoints.MapGet(pattern, async (IEnumerable<IHealthCheck> healthChecks) =>
            {
                var checkResults = new Dictionary<string, object>();
                var metrics = new Dictionary<string, object>();
                bool isHealthy = true;

                foreach (var check in healthChecks)
                {
                    var result = await check.CheckHealthAsync();

                    if (result.Status != HealthStatus.Healthy)
                        isHealthy = false;

                    // Bağımlılık durumlarını "checks" içine koyuyoruz
                    checkResults[check.Name] = new
                    {
                        Status = result.Status.ToString(),
                        Description = result.Description,
                        Error = result.Exception?.Message
                    };

                    // İŞTE KRİTİK NOKTA: CPU ve RAM gibi sayısal verileri "metrics" içine dolduruyoruz
                    if (result.Data != null)
                    {
                        foreach (var item in result.Data)
                        {
                            metrics[item.Key] = item.Value;
                        }
                    }
                }

                // Watchdog Worker'ın aradığı o kusursuz format!
                return Results.Ok(new
                {
                    status = isHealthy ? "Healthy" : "Degraded",
                    checks = checkResults,
                    metrics = metrics
                });
            });
        }
    }
}