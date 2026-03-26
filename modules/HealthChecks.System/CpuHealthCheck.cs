using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class CpuHealthCheck : IHealthCheck
    {
        private readonly double _serverCpuThreshold;
        private readonly double _appCpuThreshold;

        public string Name => "System.CPU";

        public CpuHealthCheck(double serverCpuThreshold = 90.0, double appCpuThreshold = 90.0)
        {
            _serverCpuThreshold = serverCpuThreshold;
            _appCpuThreshold = appCpuThreshold;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                using var serverCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                serverCpuCounter.NextValue(); // Windows PerformanceCounter ilk okumada 0 döner, bu yüzden tetikliyoruz.

                await Task.Delay(500, cancellationToken);

                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;
                var cpuUsage = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsage / (Environment.ProcessorCount * totalMsPassed);
                var appCpuPercent = Math.Round(cpuUsageTotal * 100, 2);

                var serverCpuPercent = Math.Round(serverCpuCounter.NextValue(), 2);

                var status = HealthStatus.Healthy;
                var message = "CPU değerleri normal.";

                if (serverCpuPercent >= _serverCpuThreshold)
                {
                    status = HealthStatus.Degraded;
                    message = $"Sunucu geneli CPU darboğazı! Sunucu: %{serverCpuPercent}, Uygulama: %{appCpuPercent}";
                }
                else if (appCpuPercent >= _appCpuThreshold)
                {
                    status = HealthStatus.Degraded;
                    message = $"Uygulama aşırı CPU tüketiyor! Uygulama: %{appCpuPercent}";
                }

                // AI
                var metrics = new Dictionary<string, object>
                {
                    { "app_cpu_percent", appCpuPercent },
                    { "server_cpu_percent", serverCpuPercent },
                    { "app_cpu_threshold", _appCpuThreshold },
                    { "server_cpu_threshold", _serverCpuThreshold }
                };

                return new HealthCheckResult { Status = status, Description = message, Data = metrics };
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("CPU metrikleri okunamadı. Windows Performance Counters erişimini kontrol edin.", ex);
            }
        }
    }
}
