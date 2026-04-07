using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    public class ArchiveSnapshotsUseCase
    {
        private readonly ISnapshotRepository _snapshotRepository;

        // Arşivlerin kaydedileceği sunucu klasörü
        private readonly string _archiveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WatchDogArchives");

        public ArchiveSnapshotsUseCase(ISnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;

            // Klasör yoksa oluştur
            if (!Directory.Exists(_archiveDirectory))
            {
                Directory.CreateDirectory(_archiveDirectory);
            }
        }

        public async Task ExecuteAsync(int olderThanDays = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

            // 1. Veritabanından belirlenen günden eski verileri (Soğuk Veri) çek
            var oldSnapshots = await _snapshotRepository.GetSnapshotsOlderThanAsync(cutoffDate);

            if (oldSnapshots == null || !oldSnapshots.Any())
                return; // Arşivlenecek eski veri yoksa işlemi bitir

            // Dosya adı: Örn: Archive_20260407_1530.json.gz
            var fileName = $"Archive_{DateTime.UtcNow:yyyyMMdd_HHmm}.json.gz";
            var filePath = Path.Combine(_archiveDirectory, fileName);

            try
            {
                // 2. Verileri JSON formatına çevir ve GZip ile sıkıştırarak diske yaz
                var jsonString = JsonSerializer.Serialize(oldSnapshots);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
                using (var streamWriter = new StreamWriter(gzipStream))
                {
                    await streamWriter.WriteAsync(jsonString);
                }

                // 3. Dosyanın diskte %100 oluştuğundan emin ol
                if (File.Exists(filePath))
                {
                    // 4. Sıkıştırma başarılıysa, verileri veritabanından tamamen sil (Hard Delete)
                    await _snapshotRepository.RemoveRangeAsync(oldSnapshots);
                }
            }
            catch (IOException)
            {
                // UC-9 Kuralı: Disk doluysa veya yetki yoksa (IOException) 
                // hata yutulur ve veritabanından SİLME işlemi (RemoveRange) çalıştırılmaz. 
                // Orijinal verilere dokunulmaz.
            }
        }
    }
}