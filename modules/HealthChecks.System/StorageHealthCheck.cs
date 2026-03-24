using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System.Diagnostics;

namespace HealthChecks.System
{
    internal class StorageHealthCheck: IHealthCheck
    {
        private readonly float _minFreeSpaceGb;

        public string Name => "System.Storage";

        public StorageHealthCheck(float minFreeSpaceGb = 1024f)
        {
            _minFreeSpaceGb = minFreeSpaceGb;
        }

        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var drivePath = Path.GetPathRoot(Directory.GetCurrentDirectory());

                if(string.IsNullOrEmpty(drivePath))
                {
                    drivePath = Path.DirectorySeparatorChar.ToString();
                }

                var driveInfo = new DriveInfo(drivePath);
                var freeSpaceGb = (float)Math.Round(driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
                var totalSizeGb = (float)Math.Round(driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0), 2);

                var status = HealthStatus.Healthy;
                var message = "Disk alanı yeterli.";

                if (freeSpaceGb <= _minFreeSpaceGb)
                {
                    status = HealthStatus.Unhealthy;
                    message = $"Kritik: Diskte yeterli boş alan kalmadı! Kalan: {freeSpaceGb} GB";
                }

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
                return Task.FromResult(HealthCheckResult.Unhealthy("Disk (Storage) metrikleri okunamadı. İzinleri kontrol edin.", ex));
            }
        }
    }
}
