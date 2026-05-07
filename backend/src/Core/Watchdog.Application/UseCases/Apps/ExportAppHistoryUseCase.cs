using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.Apps
{
    public class ExportAppHistoryUseCase
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IMonitoredAppRepository _appRepository;

        public ExportAppHistoryUseCase(ISnapshotRepository snapshotRepository, IMonitoredAppRepository appRepository)
        {
            _snapshotRepository = snapshotRepository;
            _appRepository = appRepository;
        }

        public async Task<(byte[] Content, string FileName)> ExecuteAsync(Guid appId, int days)
        {
            // 1. Uygulamayı doğrula
            var app = await _appRepository.GetByIdAsync(appId);
            if (app == null) throw new Exception("Uygulama bulunamadı.");

            // 2. Gün aralığını kısıtla (1-30 gün)
            if (days < 1) days = 1;
            if (days > 30) days = 30;

            // 3. Verileri çek
            var since = DateTime.UtcNow.AddDays(-days);
            var snapshots = await _snapshotRepository.GetSnapshotsSinceAsync(appId, since);

            // 4. CSV Formatına dönüştür
            var csv = new StringBuilder();
            // Excel için ayraç belirteci (sep=,)
            csv.AppendLine("sep=,");
            // Header
            csv.AppendLine("Tarih,Durum,Gecikme(ms),Uygulama CPU(%),Uygulama RAM(%),Sistem CPU(%),Sistem RAM(%),Bos Disk(GB),Detaylar");

            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

            foreach (var s in snapshots.OrderByDescending(x => x.Timestamp))
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(s.Timestamp, turkeyTimeZone);
                
                // Detaylar JSON içindeki virgülleri temizle ki CSV formatı bozulmasın
                var sanitizedDetails = (s.DependencyDetails ?? "").Replace(",", " | ").Replace("\r", "").Replace("\n", " ");

                csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4},{5},{6},{7},\"{8}\"",
                    localTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    s.Status,
                    s.TotalDuration,
                    s.AppCpuUsage,
                    s.AppRamUsage,
                    s.SystemCpuUsage,
                    s.SystemRamUsage,
                    s.FreeDiskGb,
                    sanitizedDetails
                ));
            }

            var fileName = $"{app.Name.Replace(" ", "_")}_{days}_Gunluk_Veri_{DateTime.Now:yyyyMMdd}.csv";
            return (Encoding.UTF8.GetBytes(csv.ToString()), fileName);
        }
    }
}
