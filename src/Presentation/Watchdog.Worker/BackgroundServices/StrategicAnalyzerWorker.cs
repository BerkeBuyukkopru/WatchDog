using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.UseCases.AI;

namespace Watchdog.Worker.BackgroundServices
{
    // AIOps Stratejik Analiz İşçisi: Genellikle gece saatlerinde (veya günde 1 kez) çalışarak geçmiş günlerin karşılaştırmalı trend analizini yapar.
    public class StrategicAnalyzerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StrategicAnalyzerWorker> _logger;

        public StrategicAnalyzerWorker(IServiceScopeFactory scopeFactory, ILogger<StrategicAnalyzerWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WatchDog: Strategic Analyzer Worker (AIOps Tahmin Motoru) başlatıldı.");

            // KRİTİK ÇÖZÜM: Seeder'ın işini bitirmesini bekle (Çift kayıt engelleme)
            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // --- KURUMSAL LOG GÜNCELLEMESİ ---
                    _logger.LogInformation("[STRATEGIC-AI] WatchDog: Günlük/Haftalık stratejik kapasite tahmini başlatılıyor...");

                    using var scope = _scopeFactory.CreateScope();

                    var userService = (WorkerCurrentUserService)scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                    userService.Username = "StrategicAnalyzerWorker";

                    var appRepository = scope.ServiceProvider.GetRequiredService<IMonitoredAppRepository>();
                    var useCase = scope.ServiceProvider.GetRequiredService<GenerateStrategicInsightUseCase>();

                    var apps = await appRepository.GetAllAsync();

                    foreach (var app in apps)
                    {
                        var request = new GenerateStrategicInsightRequest { AppId = app.Id };
                        var insight = await useCase.ExecuteAsync(request);

                        if (insight != null)
                        {
                            // --- KURUMSAL LOG GÜNCELLEMESİ ---
                            _logger.LogInformation($"[FORECAST-REPORT] [{app.Name}] Stratejik Tahmin Raporu Üretildi:\n{insight.Message}\n--------------------------------------------------");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[STRATEGIC-AI] WatchDog: StrategicAnalyzerWorker çalışırken hata oluştu.");
                }

                // KURUMSAL STANDART: Günde 1 kez çalışması idealdir. 
                // --- KURUMSAL STANDART: Her gün UTC 22:00'de (TR 01:00) çalıştır ---
                var nowUtc = DateTime.UtcNow;
                var nextRunTimeUtc = nowUtc.Date.AddHours(22); // Bugün UTC 22:00

                // Eğer saati kaçırdıysak (örneğin UTC 23:00'te sunucu açıldıysa), yarına kur
                if (nowUtc >= nextRunTimeUtc)
                {
                    nextRunTimeUtc = nextRunTimeUtc.AddDays(1);
                }

                var delay = nextRunTimeUtc - nowUtc;

                _logger.LogInformation($"[STRATEGIC-AI] WatchDog: Bir sonraki Stratejik Analiz {delay.TotalHours:F1} saat sonra (TR 01:00 / UTC 22:00) çalışacak.");

                try
                {
                    // Kapanış sinyali (stoppingToken) geldiğinde fırlatılan hatayı yakalıyoruz.
                    await Task.Delay(delay, stoppingToken);

                    // TEST İÇİN GEÇİCİ OLARAK 1 DAKİKAYA İNDİRİLDİ
                    //_logger.LogInformation($"[STRATEGIC-AI] WatchDog: TEST MODU - Bir sonraki Stratejik Analiz 1 DAKİKA sonra çalışacak.");
                    //await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Uygulama kapatılırken veya yeniden başlatılırken Task.Delay iptal edilir. Bu beklenen bir durumdur.
                    _logger.LogInformation("[STRATEGIC-AI] WatchDog: Worker güvenli bir şekilde durduruluyor (Graceful Shutdown).");
                    break; // Döngüyü kır ve işlemi güvenli bir şekilde bitir
                }
            }
        }
    }
}