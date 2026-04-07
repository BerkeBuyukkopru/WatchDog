using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.AI;
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

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("WatchDog: Günlük/Haftalık stratejik kapasite tahmini başlatılıyor...");

                    using var scope = _scopeFactory.CreateScope();
                    var appRepository = scope.ServiceProvider.GetRequiredService<IMonitoredAppRepository>();
                    var useCase = scope.ServiceProvider.GetRequiredService<GenerateStrategicInsightUseCase>();

                    var apps = await appRepository.GetAllAsync();

                    foreach (var app in apps)
                    {
                        var request = new GenerateStrategicInsightRequest { AppId = app.Id };
                        var insight = await useCase.ExecuteAsync(request);

                        if (insight != null)
                        {
                            _logger.LogInformation($"[{app.Name}] Stratejik Tahmin Raporu Üretildi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WatchDog: StrategicAnalyzerWorker çalışırken hata oluştu.");
                }

                // KURUMSAL STANDART: Günde 1 kez çalışması idealdir. 
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
