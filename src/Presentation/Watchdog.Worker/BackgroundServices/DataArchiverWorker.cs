using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.UseCases.HealthMonitoring;

namespace Watchdog.Worker.BackgroundServices
{
    public class DataArchiverWorker : BackgroundService
    {
        private readonly ILogger<DataArchiverWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DataArchiverWorker(ILogger<DataArchiverWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Archiver Worker (Soğuk Veri Arşivleme Motoru) başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Bağımsız bir işlem olduğu için yeni bir Scope (Kapsam) açıyoruz
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var archiveUseCase = scope.ServiceProvider.GetRequiredService<ArchiveSnapshotsUseCase>();

                        _logger.LogInformation("Sıcak/Soğuk veri analizi ve arşivleme işlemi başlatılıyor...");

                        // 30 günden eski verileri tespit et, GZip ile sıkıştır ve DB'den sil
                        await archiveUseCase.ExecuteAsync(olderThanDays: 30);

                        _logger.LogInformation("Arşivleme işlemi başarıyla tamamlandı.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Arşivleme sırasında beklenmeyen bir hata oluştu.");
                }

                // Sistem performansı için döngüyü uyutuyoruz.
                // Gerçek senaryoda bu Task.Delay hesaplanarak tam gece 03:00'e ayarlanır.
                // Test edebilmen için şimdilik 1 saatte bir uyanacak şekilde ayarlıyoruz:
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}