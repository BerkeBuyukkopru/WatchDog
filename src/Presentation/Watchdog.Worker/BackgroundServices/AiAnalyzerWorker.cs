using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.UseCases.AI;

namespace Watchdog.Worker.BackgroundServices
{
    // Sistemin arka planındaki Yapay Zeka orkestra şefi.
    // Belirli periyotlarla (örneğin saatte bir) uyanır, sistemdeki tüm uygulamalar için GenerateRoutineInsightUseCase'i tetikleyip analiz sonuçlarını kaydettirir.

    public class AiAnalyzerWorker : BackgroundService
    {
        // Singleton (Worker) içinde Scoped (Veritabanı vb.) servisleri kullanabilmek için Factory şarttır!
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AiAnalyzerWorker> _logger;

        public AiAnalyzerWorker(IServiceScopeFactory scopeFactory, ILogger<AiAnalyzerWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WatchDog: AI Analyzer Worker (Yapay Zeka İşçisi) başlatıldı.");

            // Worker durdurulana kadar sonsuz döngüde çalışır
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("WatchDog: Rutin AI kapasite analizi döngüsü başlıyor...");

                    // SCOPE AÇILIŞI: Veritabanı ve AI nesneleri için güvenli bir bellek alanı yaratıyoruz
                    using var scope = _scopeFactory.CreateScope();

                    var appRepository = scope.ServiceProvider.GetRequiredService<IMonitoredAppRepository>();

                    // Not: DI (Dependency Injection) tarafında bu UseCase'i sisteme kaydetmiş olmamız gerekiyor.
                    var useCase = scope.ServiceProvider.GetRequiredService<GenerateRoutineInsightUseCase>();

                    // Sistemde kayıtlı ve izlenen tüm uygulamaları getir (Uygulamaları çekecek GetAllAsync veya GetMonitoredAppsAsync gibi bir metodun olduğunu varsayıyoruz)
                    var apps = await appRepository.GetAllAsync();

                    // Her uygulama için teker teker AI analizi yap
                    foreach (var app in apps)
                    {
                        // DÜZELTME: Log mesajı 24 saatlik veriyi temsil edecek şekilde güncellendi.
                        _logger.LogInformation($"[{app.Name}] uygulaması için son 24 saatlik AI analizi talep ediliyor...");

                        var request = new GenerateRoutineInsightRequest
                        {
                            AppId = app.Id,
                            HoursToAnalyze = 24 // 24 saatlik büyük veriyi UseCase'e yolluyoruz.
                        };

                        // Beyni (UseCase) çalıştır
                        var insight = await useCase.ExecuteAsync(request);

                        if (insight != null)
                        {
                            _logger.LogInformation($"[{app.Name}] AI Tavsiyesi Üretildi: {insight.Message}");
                        }
                        else
                        {
                            _logger.LogDebug($"[{app.Name}] Yeterli veri olmadığı için AI analizi atlandı.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // AI çökse, internet gitse veya API limiti dolsa bile Worker ÖLMEMELİ. Bu yüzden hatayı loglayıp döngüye devam ediyoruz.
                    _logger.LogError(ex, "WatchDog: AiAnalyzerWorker çalışırken kritik bir hata oluştu.");
                }

                // UYKU MODU: İşlem bittikten sonra belirlediğimiz süre kadar uyu. Test aşamasında hızlı görmek için 1 Dakika, Canlıda (Production) 1 Saat idealdir.
                //Test:
               await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
               // await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                
            }
        }
    }
}