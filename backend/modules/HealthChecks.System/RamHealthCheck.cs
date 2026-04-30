using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                // 1. Uygulama RAM (MB)
                var process = Process.GetCurrentProcess();
                var appWorkingSetMb = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2);

                // 2. Sistem RAM Bilgileri
                double totalRamMb = 0;
                double availableRamMb = 0;

                if (OperatingSystem.IsWindows())
                {
                    var gcMemoryInfo = GC.GetGCMemoryInfo();
                    totalRamMb = Math.Round(gcMemoryInfo.TotalAvailableMemoryBytes / (1024.0 * 1024.0), 2);
                    var loadMb = Math.Round(gcMemoryInfo.MemoryLoadBytes / (1024.0 * 1024.0), 2);
                    availableRamMb = totalRamMb - loadMb;
                }
                else if (OperatingSystem.IsLinux())
                {
                    (totalRamMb, availableRamMb) = GetLinuxMemoryInfo();
                }

                // 3. Yüzdelik Hesaplama
                var usedRamMb = totalRamMb - availableRamMb;
                var systemRamPercent = totalRamMb > 0 ? Math.Round((usedRamMb / totalRamMb) * 100, 2) : 0;

                var status = HealthStatus.Healthy;
                var message = $"Bellek değerleri normal. (Sistem Kullanımı: %{systemRamPercent})";

                if (availableRamMb <= _minServerAvailableMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Sunucuda boş RAM kritik seviyede! Kalan: {availableRamMb} MB (Sistem: %{systemRamPercent})";
                }
                else if (appWorkingSetMb >= _maxAppAllocatedMb)
                {
                    status = HealthStatus.Degraded;
                    message = $"Uygulamada bellek sızıntısı riski! Uygulama Tüketimi: {appWorkingSetMb} MB";
                }

                var metrics = new Dictionary<string, object>
                {
                    { "system_ram_percent", systemRamPercent },
                    { "process_ram_mb", appWorkingSetMb },
                    { "server_available_ram_mb", availableRamMb },
                    { "server_total_ram_mb", totalRamMb },
                    { "app_ram_threshold_mb", _maxAppAllocatedMb },
                    { "server_min_ram_threshold_mb", _minServerAvailableMb }
                };

                return Task.FromResult(new HealthCheckResult { Status = status, Description = message, Data = metrics });
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RAM metrikleri okunamadı.", ex));
            }
        }

        private (double total, double available) GetLinuxMemoryInfo()
        {
            try
            {
                // /proc/meminfo: MemTotal, MemAvailable (kB)
                var lines = File.ReadAllLines("/proc/meminfo");
                double total = 0;
                double available = 0;

                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                        total = ParseMemInfoLine(line);
                    else if (line.StartsWith("MemAvailable:"))
                        available = ParseMemInfoLine(line);
                }

                return (Math.Round(total / 1024.0, 2), Math.Round(available / 1024.0, 2));
            }
            catch { return (0, 0); }
        }

        private double ParseMemInfoLine(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && double.TryParse(parts[1], out var kb)) return kb;
            return 0;
        }
    }
}