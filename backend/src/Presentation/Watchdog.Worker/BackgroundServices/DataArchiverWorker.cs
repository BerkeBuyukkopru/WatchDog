using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Common;
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
            _logger.LogInformation("Data Archiver Worker (Aylık Arşivleme Motoru - State-Driven) başlatıldı.");

            // KRİTİK ÇÖZÜM: API tarafındaki DatabaseSeeder'ın veritabanını hazırlaması 
            // ve çift kayıt (Race Condition) oluşmaması için Worker'a 5 saniye kalkış avansı veriyoruz.
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Bağımsız bir işlem olduğu için yeni bir Scope (Kapsam) açıyoruz
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userService = (WorkerCurrentUserService)scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                        userService.Username = "DataArchiverWorker";

                        var archiveUseCase = scope.ServiceProvider.GetRequiredService<ArchiveSnapshotsUseCase>();

                        // Zeka ve tarih hesaplaması artık UseCase'in içinde (SystemConfigurations tablosundan okunuyor).
                        // Bu yüzden dışarıdan "olderThanDays" parametresi göndermiyoruz.
                        await archiveUseCase.ExecuteAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Arşivleme döngüsü sırasında beklenmeyen bir hata oluştu.");
                }

                // Sistem performansı için döngüyü uyutuyoruz.
                // İş mantığı ve tarih kontrolü artık UseCase içinde olduğu için,
                // Worker'ın sadece günde 1 kez uyanıp "İş var mı?" diye sorması yeterlidir.
                // Not: Test ederken burayı TimeSpan.FromMinutes(1) yapabilirsin.
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}