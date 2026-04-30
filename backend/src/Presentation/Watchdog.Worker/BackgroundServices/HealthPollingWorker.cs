using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.UseCases.HealthMonitoring;
using Watchdog.Application.DTOs.Monitoring; // Eklendi

namespace Watchdog.Worker.BackgroundServices
{
    public class HealthPollingWorker : BackgroundService
    {
        private readonly ILogger<HealthPollingWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Guid, DateTime> _lastPollTimes = new();

        public HealthPollingWorker(ILogger<HealthPollingWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Watchdog Akıllı Tarama Motoru Başladı!");

            await Task.Delay(5000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var appRepository = scope.ServiceProvider.GetRequiredService<IMonitoredAppRepository>();
                        
                        // Tüm aktif uygulamaları getir
                        var apps = await appRepository.GetAllAsync();

                        foreach (var app in apps)
                        {
                            if (!app.IsActive) continue;

                            _lastPollTimes.TryGetValue(app.Id, out var lastPoll);
                            var secondsSinceLastPoll = (DateTime.UtcNow - lastPoll).TotalSeconds;

                            if (secondsSinceLastPoll >= app.PollingIntervalSeconds)
                            {
                                _logger.LogDebug("{AppName} için tarama tetikleniyor...", app.Name);
                                
                                // KRİTİK DÜZELTME: Her tarama işlemi için KENDİ scope'unu oluşturuyoruz.
                                // Aksi takdirde Task.Run içindeki servisler (DbContext vb.) ana scope 
                                // kapandığı anda Dispose olur ve hata verir.
                                _ = Task.Run(async () => {
                                    try {
                                        using (var taskScope = _serviceProvider.CreateScope())
                                        {
                                            var pollSingleUseCase = taskScope.ServiceProvider.GetRequiredService<IUseCaseAsync<PollSingleAppRequest, Watchdog.Domain.Entities.HealthSnapshot?>>();
                                            var userService = (WorkerCurrentUserService)taskScope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                                            userService.Username = "HealthPollingWorker";

                                            await pollSingleUseCase.ExecuteAsync(new PollSingleAppRequest { AppId = app.Id, CancellationToken = stoppingToken });
                                        }
                                    } catch (Exception ex) {
                                        _logger.LogError(ex, "{AppName} taraması sırasında hata oluştu.", app.Name);
                                    }
                                }, stoppingToken);

                                _lastPollTimes[app.Id] = DateTime.UtcNow;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Tarama ana döngüsü hatası.");
                }
                // Döngü her 5 saniyede bir kontrol yapar (Sistemi yormamak için)
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}