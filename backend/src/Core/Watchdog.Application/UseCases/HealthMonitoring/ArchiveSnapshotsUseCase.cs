using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities; // SystemConfiguration için eklendi

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    public class ArchiveSnapshotsUseCase
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly ISystemConfigurationRepository _configRepository;

        private readonly string _archiveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WatchDogArchives");

        public ArchiveSnapshotsUseCase(
            ISnapshotRepository snapshotRepository,
            ISystemConfigurationRepository configRepository)
        {
            _snapshotRepository = snapshotRepository;
            _configRepository = configRepository;

            if (!Directory.Exists(_archiveDirectory))
            {
                Directory.CreateDirectory(_archiveDirectory);
            }
        }

        public async Task ExecuteAsync()
        {
            // 1. Senin repository'ni kullanarak sistem ayarlarını (Singleton) komple getiriyoruz
            var config = await _configRepository.GetAsync();

            // Veritabanı boşsa (proje ilk kez ayağa kalktıysa) geçici bir config nesnesi yarat
            if (config == null)
            {
                config = new SystemConfiguration();
            }

            // 2. Hafızadan "En son bitirilen tarihi" al. Daha önce hiç yapılmadıysa varsayılan başlangıç tarihi (Örn: 1 Ocak 2026) ata.
            DateTime lastFinishedDate = config.LastArchivedDate ?? new DateTime(2026, 01, 01);

            DateTime today = DateTime.UtcNow;
            DateTime targetEndDate = new DateTime(today.Year, today.Month, 1).AddTicks(-1);

            // 3. Geçmiş ayları süpürme döngüsü (Catch-up)
            while (lastFinishedDate < targetEndDate)
            {
                var processingMonthStart = new DateTime(lastFinishedDate.Year, lastFinishedDate.Month, 1).AddMonths(1);
                var processingMonthEnd = processingMonthStart.AddMonths(1).AddTicks(-1);

                if (processingMonthStart.Month == today.Month && processingMonthStart.Year == today.Year) break;

                Console.WriteLine($">>>> ARŞİVLEYİCİ ÇALIŞIYOR - {processingMonthStart:MMMM yyyy} VERİLERİ İŞLENİYOR...");

                await ProcessMonthlyArchive(processingMonthStart, processingMonthEnd);

                // 4. Veritabanındaki hafızayı SADECE bu sınıfı kullanarak güncelle
                lastFinishedDate = processingMonthStart;
                config.LastArchivedDate = lastFinishedDate;
                config.LastUpdated = DateTime.UtcNow;

                // Senin projendeki Singleton update metodunu çağırıyoruz
                await _configRepository.UpdateAsync(config);

                Console.WriteLine($">>>> [DATABASE-STATE] Hafıza güncellendi: {lastFinishedDate:MMMM yyyy} başarıyla mühürlendi.");
            }
        }

        // --- RAM Dostu Batch Zipleme Metodu (Aynı kalıyor) ---
        private async Task ProcessMonthlyArchive(DateTime start, DateTime end)
        {
            var fileName = $"Archive_{start:yyyy_MM}.json.gz";
            var filePath = Path.Combine(_archiveDirectory, fileName);
            int batchSize = 10000;
            int totalArchived = 0;

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
                using (var streamWriter = new StreamWriter(gzipStream))
                {
                    while (true)
                    {
                        var batch = await _snapshotRepository.GetSnapshotsByDateRangeAsync(start, end, batchSize);
                        if (batch == null || !batch.Any()) break;

                        foreach (var snapshot in batch)
                        {
                            var jsonString = JsonSerializer.Serialize(snapshot);
                            await streamWriter.WriteLineAsync(jsonString);
                        }

                        await _snapshotRepository.RemoveRangeAsync(batch);
                        totalArchived += batch.Count;
                        Console.WriteLine($">>>> Veritabanı rahatlatılıyor... Toplam {totalArchived} kayıt arşivlendi ve silindi.");
                    }
                }

                if (totalArchived == 0)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    Console.WriteLine($">>>> {start:MMMM yyyy} için arşivlenecek log bulunamadı. Boş dosya silindi.");
                }
                else
                {
                    Console.WriteLine($">>>> [BAŞARILI] {start:MMMM yyyy} ayına ait {totalArchived} kayıt tek dosyaya sıkıştırıldı!");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($">>>> [KRİTİK HATA] Diske yazılırken bir sorun oluştu: {ex.Message}");
            }
        }
    }
}