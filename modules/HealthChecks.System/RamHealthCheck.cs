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

        // Program.cs'deki ayarların (DI) patlamaması için parametreleri MB olarak tutmaya devam ediyoruz.
        public RamHealthCheck(float minServerAvailableMb = 1024f, float maxAppAllocatedMb = 1024f)
        {
            _minServerAvailableMb = minServerAvailableMb;
            _maxAppAllocatedMb = maxAppAllocatedMb;
        }

        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Uygulamanın Kendi Tükettiği RAM (MB)
                var process = Process.GetCurrentProcess();
                var appWorkingSetMb = Math.Round(process.WorkingSet64 / (1024f * 1024f), 2);

                // 2. Sunucudaki BOŞ RAM (MB)
                using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                var serverAvailableRamMb = Math.Round(ramCounter.NextValue(), 2);

                // 3. YENİ: Sistemin TOPLAM RAM'ini okuma (.NET'in kendi özelliği)
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                var totalPhysicalMemoryMb = Math.Round(gcMemoryInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0), 2);

                // 4. YENİ: Yüzdelik Kullanım Hesaplaması: ((Toplam - Boş) / Toplam) * 100
                var usedRamMb = totalPhysicalMemoryMb - serverAvailableRamMb;
                var ramUsagePercent = Math.Round((usedRamMb / totalPhysicalMemoryMb) * 100, 2);

                var status = HealthStatus.Healthy;
                var message = $"Bellek değerleri normal. (Kullanım: %{ramUsagePercent})";

                if (serverAvailableRamMb <= _minServerAvailableMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Sunucuda boş RAM kritik seviyede! Kalan: {serverAvailableRamMb} MB (Kullanım: %{ramUsagePercent})";
                }
                else if (appWorkingSetMb >= _maxAppAllocatedMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Uygulamada bellek sızıntısı (Leak) riski! Tüketilen: {appWorkingSetMb} MB";
                }

                // 5. Worker motorunun alıp veritabanına yazacağı Metrik Çantası
                var metrics = new Dictionary<string, object>
                {
                    { "app_allocated_ram_mb", appWorkingSetMb },
                    { "server_available_ram_mb", serverAvailableRamMb },
                    { "server_total_ram_mb", totalPhysicalMemoryMb }, // Bilgi amaçlı toplam RAM'i de gönderiyoruz
                    { "ram_usage_percent", ramUsagePercent },         // YENİ: Worker artık bunu okuyacak!
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