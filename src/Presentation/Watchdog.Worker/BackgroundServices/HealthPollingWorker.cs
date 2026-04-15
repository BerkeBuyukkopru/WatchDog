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
    // Sistemin 7/24 uyanık kalan asenkron zamanlayıcısı. 
    // İş kurallarını (Business Logic) bilmez, sadece toplu tarama Use Case'ini tetikler.
    public class HealthPollingWorker : BackgroundService
    {
        private readonly ILogger<HealthPollingWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public HealthPollingWorker(ILogger<HealthPollingWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // --- ANA YÖNETİCİ DÖNGÜ (Sadece Zamanlayıcı) ---
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Watchdog Sadeleştirilmiş Tarama Motoru Başladı!");

            // KRİTİK ÇÖZÜM: Seeder'ın işini bitirmesini bekle (Çift kayıt engelleme)
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userService = (WorkerCurrentUserService)scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                        userService.Username = "HealthPollingWorker";

                        // Toplu tarama Use Case'ini çağırıyoruz
                        var pollAllAppsUseCase = scope.ServiceProvider.GetRequiredService<PollAllAppsUseCase>();

                        _logger.LogInformation("Toplu tarama tetikleniyor: {Time}", DateTimeOffset.Now);

                        // İşin tüm yükünü Use Case'e devrediyoruz
                        await pollAllAppsUseCase.ExecuteAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // Hata olursa Worker çökmesin diye loglayıp devam ediyoruz
                    _logger.LogError(ex, "Tarama döngüsü sırasında beklenmeyen bir hata oluştu.");
                }

                // Bir sonraki genel kontrol için 15 saniye bekle
                await Task.Delay(15000, stoppingToken);
            }
        }
    }
}