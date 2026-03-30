using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO; // Derleme hatasını çözen kütüphane. DriveInfo ve Path sınıfları için zorunlu.
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HealthChecks.System
{
    // public yapılarak API projesindeki Dependency Injection (DI) motorunun bu sınıfı görebilmesi sağlandı.
    public class StorageHealthCheck : IHealthCheck
    {
        private readonly float _minFreeSpaceGb;

        public string Name => "System.Storage";

        public StorageHealthCheck(float minFreeSpaceGb = 5f)
        {
            _minFreeSpaceGb = minFreeSpaceGb;
        }

        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Uygulamanın çalıştığı ana diski(Root) buluyoruz.
                var drivePath = Path.GetPathRoot(Directory.GetCurrentDirectory());

                if (string.IsNullOrEmpty(drivePath))
                {
                    drivePath = Path.DirectorySeparatorChar.ToString();
                }

                // Disk bilgilerini okuma ve Byte -> GB dönüşümü.
                var driveInfo = new DriveInfo(drivePath);
                var freeSpaceGb = (float)Math.Round(driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
                var totalSizeGb = (float)Math.Round(driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0), 2);

                var status = HealthStatus.Healthy;
                var message = "Disk alanı yeterli.";

                // Eşik Kontrolü.
                if (freeSpaceGb <= _minFreeSpaceGb)
                {
                    status = HealthStatus.Degraded; // Çökmüş (Unhealthy) yerine Degraded (Yavaşlamış/Riskli) demek mimari açıdan daha doğrudur
                    message = $"Kritik: Diskte yeterli boş alan kalmadı! Kalan: {freeSpaceGb} GB";
                }

                // Dashboard ve AI için metrik çantası hazırlanıyor.
                var metrics = new Dictionary<string, object>
                {
                    { "free_disk_gb", freeSpaceGb },
                    { "total_disk_gb", totalSizeGb },
                    { "min_required_gb", _minFreeSpaceGb },
                    { "drive", drivePath }
                };

                return Task.FromResult(new HealthCheckResult { Status = status, Description = message, Data = metrics });
            }
            catch (Exception ex)
            {
                // Yetki hatası (I/O) durumunda sistemi direkt Unhealthy işaretliyoruz.
                return Task.FromResult(HealthCheckResult.Unhealthy("Disk (Storage) metrikleri okunamadı. İzinleri kontrol edin.", ex));
            }
        }
    }
}