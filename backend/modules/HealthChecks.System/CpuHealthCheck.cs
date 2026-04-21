using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                // 1. Uygulamanın Başlangıç CPU Durumu
                var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                // 2. Sunucu Genel CPU'su için Counter
                using var serverCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                serverCpuCounter.NextValue(); // Windows PerformanceCounter ilk okumada 0 döner, bu yüzden tetikliyoruz.

                // 3. CPU kullanımını ölçmek için kısa bir süre bekliyoruz
                await Task.Delay(500, cancellationToken);

                // 4. Uygulamanın Bitiş CPU Durumu ve Hesaplanması
                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;
                var cpuUsageMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;

                var cpuUsageTotal = cpuUsageMs / (Environment.ProcessorCount * totalMsPassed);
                var processCpuPercent = Math.Round(cpuUsageTotal * 100, 2);

                // 5. Sunucu Genel CPU Yüzdesi
                var systemCpuPercent = Math.Round(serverCpuCounter.NextValue(), 2);

                var status = HealthStatus.Healthy;
                var message = $"CPU değerleri normal. (Sistem: %{systemCpuPercent})";

                // 6. Eşik Değer (Threshold) Kontrolleri
                if (systemCpuPercent >= _serverCpuThreshold)
                {
                    status = HealthStatus.Degraded;
                    message = $"Sunucu geneli CPU darboğazı! Sistem: %{systemCpuPercent}, Uygulama: %{processCpuPercent}";
                }
                else if (processCpuPercent >= _appCpuThreshold)
                {
                    status = HealthStatus.Degraded;
                    message = $"Uygulama aşırı CPU tüketiyor! Uygulama: %{processCpuPercent}";
                }

                // 7. Faz 3 AI Motoru için standartlaştırılmış Metrik Çantası
                var metrics = new Dictionary<string, object>
                {
                    { "system_cpu_percent", systemCpuPercent },      // Worker ana grafik için bunu okuyacak
                    { "process_cpu_percent", processCpuPercent },    // AI Kök neden analizi için bunu kullanacak
                    { "server_cpu_threshold", _serverCpuThreshold },
                    { "app_cpu_threshold", _appCpuThreshold }
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