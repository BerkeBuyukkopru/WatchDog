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
                // Uygulamanın Kendi Tükettiği RAM (MB) - APM Analizi için kritik [cite: 91, 421]
                var process = Process.GetCurrentProcess();
                var appWorkingSetMb = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2);

                // Sunucudaki toplam "Kullanılabilir" (Free) belleği okuyoruz.
                using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                var serverAvailableRamMb = Math.Round(ramCounter.NextValue(), 2);

                // Toplam fiziksel belleği öğreniyoruz. [cite: 329]
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                var totalPhysicalMemoryMb = Math.Round(gcMemoryInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0), 2);

                // Sistem Genel Yüzdelik Kullanımı: ((Toplam - Boş) / Toplam) * 100
                var usedRamMb = totalPhysicalMemoryMb - serverAvailableRamMb;
                var systemRamPercent = Math.Round((usedRamMb / totalPhysicalMemoryMb) * 100, 2);

                var status = HealthStatus.Healthy;
                var message = $"Bellek değerleri normal. (Sistem Kullanımı: %{systemRamPercent})";

                // Eşik Değer Kontrolleri 
                if (serverAvailableRamMb <= _minServerAvailableMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Sunucuda boş RAM kritik seviyede! Kalan: {serverAvailableRamMb} MB (Sistem: %{systemRamPercent})";
                }
                else if (appWorkingSetMb >= _maxAppAllocatedMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Uygulamada bellek sızıntısı (Leak) riski! Uygulama Tüketimi: {appWorkingSetMb} MB";
                }

                // AI Motoru için standartlaştırılmış Metrik Çantası
                var metrics = new Dictionary<string, object>
                {
                    { "system_ram_percent", systemRamPercent },    // Worker ana grafik için bunu okuyacak
                    { "process_ram_mb", appWorkingSetMb },        // AI Kök neden analizi için bunu kullanacak
                    { "server_available_ram_mb", serverAvailableRamMb },
                    { "server_total_ram_mb", totalPhysicalMemoryMb },
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