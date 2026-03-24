using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;

namespace HealthChecks.System
{
    public class RamHealthCheck : IHealthCheck
    {
        private readonly float _minServerAvailableMb;
        private readonly float _maxAppAllocatedMb;

        public string Name => "System.RAM";

        public RamHealthCheck(float minServerAvailableMb = 1024f, float maxAppAllocatedMb = 1024f)
        {
            _minServerAvailableMb = minServerAvailableMb;
            _maxAppAllocatedMb = maxAppAllocatedMb;
        }

        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var appWorkingSetMb = Math.Round(process.WorkingSet64 / (1024f * 1024f), 2);

                using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                var serverAvailableRamMb = Math.Round(ramCounter.NextValue(), 2);

                var status = HealthStatus.Healthy;
                var message = "Bellek değerleri normal.";

                if (serverAvailableRamMb <= _minServerAvailableMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Sunucuda boş RAM kritik seviyede! Kalan: {serverAvailableRamMb} MB";
                }

                else if (appWorkingSetMb >= _maxAppAllocatedMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Uygulamada bellek sızıntısı (Leak) riski! Tüketilen: {appWorkingSetMb} MB";
                }

                var metrics = new Dictionary<string, object>
                {
                    { "app_allocated_ram_mb", appWorkingSetMb },
                    { "server_available_ram_mb", serverAvailableRamMb },
                    { "app_ram_threshold_mb", _maxAppAllocatedMb },
                    { "server_min_ram_threshold_mb", _minServerAvailableMb }
                };

                return Task.FromResult(new HealthCheckResult
                {
                    Status = status,
                    Description = message,
                    Data = metrics
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RAM metrikleri okunamadı. Windows Performance Counters erişimini kontrol edin.", ex));
            }
        }
    }
}