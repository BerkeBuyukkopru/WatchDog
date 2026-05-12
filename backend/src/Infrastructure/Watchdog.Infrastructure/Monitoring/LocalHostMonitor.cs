using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Watchdog.Application.Interfaces.Monitoring;

namespace Watchdog.Infrastructure.Monitoring
{
    public class LocalHostMonitor : ILocalHostMonitor, IDisposable
    {
        private PerformanceCounter? _winCpuCounter;
        private (long total, long idle) _lastLinuxCpu = (0, 0);
        private bool _isWindowsCpuAvailable = false;

        public LocalHostMonitor()
        {
            InitializeCpuCounter();
        }

        private void InitializeCpuCounter()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
#pragma warning disable CA1416
                    _winCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _winCpuCounter.NextValue(); // Isınma turu
                    _isWindowsCpuAvailable = true;
#pragma warning restore CA1416
                }
                else if (OperatingSystem.IsLinux())
                {
                    _lastLinuxCpu = GetLinuxCpuTimes();
                }
            }
            catch (Exception ex)
            {
                // Yetki hatası veya sistem kısıtlaması durumunda uygulamayı patlatmıyoruz.
                Console.WriteLine($">>>> [MONITOR-ERROR] CPU sayacı başlatılamadı (Bulut kısıtlaması olabilir): {ex.Message}");
                _isWindowsCpuAvailable = false;
            }
        }

        public CentralSystemMetricsDto GetCurrentHostMetrics()
        {
            var metrics = new CentralSystemMetricsDto
            {
                LastUpdated = DateTime.UtcNow,
                SystemCpu = 0,
                SystemRam = 0
            };

            // 1. CPU ÖLÇÜMÜ
            try
            {
                if (OperatingSystem.IsWindows() && _isWindowsCpuAvailable && _winCpuCounter != null)
                {
#pragma warning disable CA1416
                    metrics.SystemCpu = Math.Round(_winCpuCounter.NextValue(), 2);
#pragma warning restore CA1416
                }
                else if (OperatingSystem.IsLinux())
                {
                    var currentLinuxCpu = GetLinuxCpuTimes();
                    var totalDelta = currentLinuxCpu.total - _lastLinuxCpu.total;
                    var idleDelta = currentLinuxCpu.idle - _lastLinuxCpu.idle;

                    if (totalDelta > 0)
                    {
                        var usage = 1.0 - (double)idleDelta / totalDelta;
                        metrics.SystemCpu = Math.Max(0, Math.Min(100, Math.Round(usage * 100, 2)));
                    }
                    _lastLinuxCpu = currentLinuxCpu;
                }
            }
            catch { /* Sessiz hata yönetimi */ }

            // 2. RAM ÖLÇÜMÜ (GC.GetGCMemoryInfo her yerde çalışır)
            try
            {
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                
                // .NET 5+ ile gelen TotalAvailableMemoryBytes, Docker/Cloud limitlerini de dikkate alır.
                if (gcMemoryInfo.TotalAvailableMemoryBytes > 0)
                {
                    var usedRam = gcMemoryInfo.MemoryLoadBytes;
                    var totalRam = gcMemoryInfo.TotalAvailableMemoryBytes;
                    metrics.SystemRam = Math.Round(((double)usedRam / totalRam) * 100, 2);
                }
                else if (OperatingSystem.IsLinux())
                {
                    // Fallback: MemInfo (Eski Linux kernel veya kısıtlı ortamlar için)
                    var (totalRamMb, availableRamMb) = GetLinuxMemoryInfo();
                    var usedRamMb = totalRamMb - availableRamMb;
                    if (totalRamMb > 0)
                    {
                        metrics.SystemRam = Math.Round((usedRamMb / totalRamMb) * 100, 2);
                    }
                }
            }
            catch { }

            // 3. DİSK ÖLÇÜMÜ
            try
            {
                var drivePath = OperatingSystem.IsWindows() ? Path.GetPathRoot(Directory.GetCurrentDirectory()) : "/";
                if (string.IsNullOrEmpty(drivePath)) drivePath = "/";

                var driveInfo = new DriveInfo(drivePath);
                if (driveInfo.IsReady)
                {
                    metrics.FreeDiskGb = Math.Round(driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
                    metrics.TotalDiskGb = Math.Round(driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0), 2);
                }
            }
            catch
            {
                // Disk okuma izni yoksa varsayılan 0 kalır.
            }

            return metrics;
        }

        private (long total, long idle) GetLinuxCpuTimes()
        {
            try
            {
                var lines = File.ReadAllLines("/proc/stat");
                var cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
                if (cpuLine == null) return (0, 0);

                var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5) return (0, 0);

                var user = long.Parse(parts[1]);
                var nice = long.Parse(parts[2]);
                var system = long.Parse(parts[3]);
                var idle = long.Parse(parts[4]);
                var iowait = parts.Length > 5 ? long.Parse(parts[5]) : 0;
                var irq = parts.Length > 6 ? long.Parse(parts[6]) : 0;
                var softirq = parts.Length > 7 ? long.Parse(parts[7]) : 0;
                var steal = parts.Length > 8 ? long.Parse(parts[8]) : 0;

                var total = user + nice + system + idle + iowait + irq + softirq + steal;
                return (total, idle);
            }
            catch { return (0, 0); }
        }

        private (double total, double available) GetLinuxMemoryInfo()
        {
            try
            {
                var lines = File.ReadAllLines("/proc/meminfo");
                double total = 0;
                double available = 0;

                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:")) total = ParseMemInfoLine(line);
                    else if (line.StartsWith("MemAvailable:")) available = ParseMemInfoLine(line);
                    else if (line.StartsWith("MemFree:") && available == 0) available = ParseMemInfoLine(line);
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

        public void Dispose()
        {
            try
            {
#pragma warning disable CA1416
                _winCpuCounter?.Dispose();
#pragma warning restore CA1416
            }
            catch { }
        }
    }
}
