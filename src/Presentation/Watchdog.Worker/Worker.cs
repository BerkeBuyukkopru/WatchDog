using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Domain.Entities;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Worker
{
    // Sistemin 7/24 uyanık kalan asenkron zamanlayıcısı. 
    // İş kurallarını (Business Logic) bilmez, sadece Use Case'leri tetikler.
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeTasks = new();
        private readonly HubConnection _hubConnection;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            // === SİGNALR KÖPRÜSÜNÜN İNŞASI ===
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7054/statushub")
                .WithAutomaticReconnect()
                .Build();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _hubConnection.StartAsync(cancellationToken);
                _logger.LogInformation("Worker, API'nin SignalR tüneline (StatusHub) başarıyla bağlandı.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("SignalR tüneline bağlanılamadı. API şu an ayakta olmayabilir. Detay: {Message}", ex.Message);
            }

            await base.StartAsync(cancellationToken);
        }

        // --- 1. ANA YÖNETİCİ DÖNGÜ (Orkestra Şefi) ---
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Watchdog Asenkron Tarama Motoru (V3 - Clean Architecture) Başladı!");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var appRepository = scope.ServiceProvider.GetRequiredService<IMonitoredAppRepository>();
                    var appsInDb = await appRepository.GetAllAsync();

                    // YENİ EKLENEN UYGULAMALARI BUL VE MOTORLARINI ÇALIŞTIR
                    foreach (var app in appsInDb)
                    {
                        if (!_activeTasks.ContainsKey(app.Id))
                        {
                            var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                            _activeTasks.TryAdd(app.Id, cts);

                            _ = Task.Run(() => StartPollingAppAsync(app.Id, cts.Token), cts.Token);

                            _logger.LogInformation("Yeni izleme görevi başlatıldı: {AppName} (Her {Interval} saniyede bir)", app.Name, app.PollingIntervalSeconds);
                        }
                    }

                    // SİLİNEN UYGULAMALARI BUL VE MOTORLARINI DURDUR
                    var dbAppIds = appsInDb.Select(a => a.Id).ToList();
                    var tasksToRemove = _activeTasks.Keys.Except(dbAppIds).ToList();

                    foreach (var idToRemove in tasksToRemove)
                    {
                        if (_activeTasks.TryRemove(idToRemove, out var cts))
                        {
                            cts.Cancel();
                            _logger.LogWarning("Uygulama silindiği için izleme görevi iptal edildi: {AppId}", idToRemove);
                        }
                    }
                }
                await Task.Delay(15000, stoppingToken);
            }
        }

        // --- 2. HER BİR UYGULAMANIN KENDİ BAĞIMSIZ DÖNGÜSÜ (İşçi) ---
        private async Task StartPollingAppAsync(Guid appId, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int intervalSeconds = 30;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var appRepository = scope.ServiceProvider.GetRequiredService<IMonitoredAppRepository>();
                    var app = await appRepository.GetByIdAsync(appId);

                    if (app == null) break;

                    intervalSeconds = app.PollingIntervalSeconds;

                    // === İŞİN BEYNİ BURASI ===
                    var pollUseCase = scope.ServiceProvider.GetRequiredService<IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?>>();

                    var request = new PollSingleAppRequest { AppId = appId, CancellationToken = token };
                    var snapshot = await pollUseCase.ExecuteAsync(request);

                    // Canlı yayını tetikler (React arayüzü için)
                    if (snapshot != null && _hubConnection.State == HubConnectionState.Connected)
                    {
                        await _hubConnection.InvokeAsync("BroadcastNewStatus", snapshot, token);
                        _logger.LogInformation("{AppName} tarandı. Durum: {Status}", app.Name, snapshot.Status);
                    }
                }
                await Task.Delay(intervalSeconds * 1000, token);
            }
        }
    }
}