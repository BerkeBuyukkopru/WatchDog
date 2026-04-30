using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.System
{
    public class CpuHealthCheck : IHealthCheck
    {
        private readonly double _serverCpuThreshold;
        private readonly double _appCpuThreshold;

        // Statik değişkenler: İki ölçüm arasındaki farkı (Delta) hesaplamak için kullanılır
        private static TimeSpan _lastAppProcessTime = TimeSpan.Zero;
        private static DateTime _lastAppSampleTime = DateTime.MinValue;
        private static readonly object _appLock = new object();

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
                // 1. Sistem (Server) CPU Ölçümü Başlangıcı
                (long total, long idle) systemStart = (0, 0);
                PerformanceCounter? winCounter = null;

                if (OperatingSystem.IsWindows())
                {
#pragma warning disable CA1416
                    winCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    winCounter.NextValue();
#pragma warning restore CA1416
                }
                else if (OperatingSystem.IsLinux())
                {
                    systemStart = GetLinuxCpuTimes();
                }

                // 2. Örnekleme Aralığı (Sistem CPU'su için kısa bir bekleme hala gerekli)
                await Task.Delay(500, cancellationToken);

                // 3. Uygulama (Process) CPU Hesaplama - Kümülatif Delta Mantığı
                var process = Process.GetCurrentProcess();
                var currentAppProcessTime = process.TotalProcessorTime;
                var currentAppSampleTime = DateTime.UtcNow;
                double processCpuPercent = 0;

                lock (_appLock)
                {
                    if (_lastAppSampleTime != DateTime.MinValue)
                    {
                        var cpuUsedMs = (currentAppProcessTime - _lastAppProcessTime).TotalMilliseconds;
                        var elapsedMs = (currentAppSampleTime - _lastAppSampleTime).TotalMilliseconds;

                        if (elapsedMs > 0)
                        {
                            // Formül: (Kullanılan CPU Süresi / (Çekirdek Sayısı * Geçen Gerçek Süre)) * 100
                            processCpuPercent = Math.Round((cpuUsedMs / (Environment.ProcessorCount * elapsedMs)) * 100, 3);
                        }
                    }
                    else
                    {
                        // İlk çalıştırmada fark hesaplanamaz, 0 döner ve referans noktası oluşur.
                        processCpuPercent = 0;
                    }

                    _lastAppProcessTime = currentAppProcessTime;
                    _lastAppSampleTime = currentAppSampleTime;
                }

                // 4. Sistem CPU Hesaplama
                double systemCpuPercent = 0;
                if (winCounter != null)
                {
#pragma warning disable CA1416
                    systemCpuPercent = Math.Round(winCounter.NextValue(), 3);
                    winCounter.Dispose();
#pragma warning restore CA1416
                }
                else if (OperatingSystem.IsLinux())
                {
                    var systemEnd = GetLinuxCpuTimes();
                    var totalDelta = systemEnd.total - systemStart.total;
                    var idleDelta = systemEnd.idle - systemStart.idle;

                    if (totalDelta > 0)
                    {
                        var usage = 1.0 - (double)idleDelta / totalDelta;
                        systemCpuPercent = Math.Round(usage * 100, 3);
                    }
                }

                var status = HealthStatus.Healthy;
                var message = $"CPU değerleri normal. (Sistem: %{systemCpuPercent})";

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

                var metrics = new Dictionary<string, object>
                {
                    { "system_cpu_percent", systemCpuPercent },
                    { "process_cpu_percent", processCpuPercent },
                    { "server_cpu_threshold", _serverCpuThreshold },
                    { "app_cpu_threshold", _appCpuThreshold }
                };

                return new HealthCheckResult { Status = status, Description = message, Data = metrics };
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("CPU metrikleri okunamadı.", ex);
            }
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
    }
}