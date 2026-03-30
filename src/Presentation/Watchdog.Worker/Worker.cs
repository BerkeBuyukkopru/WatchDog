using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client; // SignalR Kütüphanesi
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;
using Watchdog.Infrastructure.Persistence;

namespace Watchdog.Worker
{
    // Sistemin 7/24 uyanık kalan tek hücresi. BackgroundService sayesinde uygulama kapanana kadar asenkron döngüsünü sürdürür.
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;

        // POLLY: Güvenlik Kalkanımız. Bir site "zombi" olursa (bağlantı açık ama veri yok), Polly 5. saniyede fişi çeker. Sistemin tıkanmasını önler.
        private readonly AsyncTimeoutPolicy _timeoutPolicy;

        // Çoklu Görev Yöneticisi: Hangi uygulamanın motoru çalışıyor takip etmek için (UC-1 Silme İşlemi Entegrasyonu)
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeTasks = new();

        // API'mizdeki yayın odasına bağlanacak olan köprü (Tünel)
        private readonly HubConnection _hubConnection;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpClient = new HttpClient();

            // POLLY KURALI: Bir siteye ping attığımızda 5 saniye içinde cevap gelmezse bekleme, fişini çek! (Timeout)
            _timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Pessimistic);

            // === SİGNALR KÖPRÜSÜNÜN İNŞASI ===
            // Not: Buradaki port numarasının Watchdog.Api'nin çalıştığı port (örn: 7054) olduğuna emin ol.
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7054/statushub")
                .WithAutomaticReconnect() // API geçici olarak kapansa bile otomatik tekrar dener
                .Build();
        }

        // === KÖPRÜYÜ AKTİF ETME ===
        // Worker servisi başlarken tünele bağlanıyoruz.
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
            _logger.LogInformation("Watchdog Asenkron Tarama Motoru (V2) Başladı!");

            while (!stoppingToken.IsCancellationRequested)
            {
                // BackgroundService(Singleton) içinden DbContext(Scoped) çağırmak için 'Scope' açıyoruz. Bu hamle, her döngüde temiz bir veritabanı bağlantısı sağlar (Memory Leak'i önler).
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<WatchdogDbContext>();

                    var appsInDb = await dbContext.MonitoredApps.ToListAsync(stoppingToken);

                    // YENİ EKLENEN UYGULAMALARI BUL VE MOTORLARINI ÇALIŞTIR
                    foreach (var app in appsInDb)
                    {
                        if (!_activeTasks.ContainsKey(app.Id))
                        {
                            // Ana durdurma sinyali (stoppingToken) ile bağlı bir alt sinyal oluşturuyoruz.
                            var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                            _activeTasks.TryAdd(app.Id, cts);

                            // Task.Run: Her uygulama kendi bağımsız 'Thread'inde (kanalında) koşar. Bir uygulamanın yavaşlığı diğerlerini asla etkilemez.
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
                            cts.Cancel(); // Uygulamanın sonsuz döngüsüne "Dur!" sinyali gönderir.
                            _logger.LogWarning("Uygulama silindiği için izleme görevi iptal edildi: {AppId}", idToRemove);
                        }
                    }
                }
                // 15 saniyede bir veritabanında yeni uygulama var mı diye kontrol et.
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
                    var dbContext = scope.ServiceProvider.GetRequiredService<WatchdogDbContext>();

                    // Kural motorumuzu DI (Resepsiyonist) üzerinden çağırıyoruz
                    var analyzeUseCase = scope.ServiceProvider.GetRequiredService<Watchdog.Application.UseCases.AnalyzeSystemHealthUseCase>();

                    var app = await dbContext.MonitoredApps.FindAsync(new object[] { appId }, token);

                    if (app == null) break;

                    intervalSeconds = app.PollingIntervalSeconds;

                    // Asıl ping ve veritabanı kayıt işlemi burada başlar.
                    await CheckAppHealthAsync(app, dbContext, analyzeUseCase, token);
                }
                // Uygulamanın kendi ayarındaki süre (Örn: 10sn) kadar uyu.
                await Task.Delay(intervalSeconds * 1000, token);
            }
        }

        // --- 3. PING ATMA VE JSON OKUMA METODU (V3) ---
        private async Task CheckAppHealthAsync(MonitoredApp app, WatchdogDbContext dbContext, Watchdog.Application.UseCases.AnalyzeSystemHealthUseCase analyzeUseCase, CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew(); // Yanıt süresini ölçmek için kronometre başlat.
            HealthStatus currentStatus;

            // Başlangıç metrikleri (Sıfırlama)
            double realCpu = 0;
            double realRamPercent = 0;
            double realDisk = 0;
            string dependencyDetailsJson = null;

            try
            {
                // Polly kalkanı altında HTTP isteğini fırlatıyoruz.
                var response = await _timeoutPolicy.ExecuteAsync(async () =>
                {
                    return await _httpClient.GetAsync(app.HealthUrl, token);
                });

                if (response.IsSuccessStatusCode)
                {
                    currentStatus = HealthStatus.Healthy;

                    var jsonString = await response.Content.ReadAsStringAsync(token);
                    dependencyDetailsJson = jsonString;

                    try
                    {
                        //Gelen JSON raporunu parse ediyoruz.
                        using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);

                        if (jsonDoc.RootElement.TryGetProperty("metrics", out var metricsElement))
                        {
                            if (metricsElement.TryGetProperty("cpu_usage", out var cpuProp)) realCpu = cpuProp.GetDouble();

                            // JSON'dan "ram_usage_percent" değerini çekiyoruz
                            if (metricsElement.TryGetProperty("ram_usage_percent", out var ramProp)) realRamPercent = ramProp.GetDouble();

                            if (metricsElement.TryGetProperty("free_disk_gb", out var diskProp)) realDisk = diskProp.GetDouble();
                        }
                    }
                    catch
                    {
                        _logger.LogWarning("{AppName} adresinden geçerli bir JSON metrik raporu okunamadı.", app.Name);
                    }
                }
                else
                {
                    currentStatus = HealthStatus.Degraded; // Site ayakta ama hata kodu (Örn: 500) dönüyor.
                }
            }
            catch (TimeoutRejectedException)
            {
                // Timeout veya DNS hatası gibi durumlarda sistem 'Unhealthy' (Kırmızı) olur.
                currentStatus = HealthStatus.Unhealthy;
                _logger.LogWarning("{AppName} zaman aşımına uğradı (Timeout)!", app.Name);
            }
            catch (Exception)
            {
                currentStatus = HealthStatus.Unhealthy;
            }

            stopwatch.Stop();

            // Veritabanı modelinde sütun adı "RamUsageMb" kalsa bile içine yüzde değerini basıyoruz
            var snapshot = new HealthSnapshot
            {
                AppId = app.Id,
                Timestamp = DateTime.UtcNow,
                Status = currentStatus,
                TotalDuration = stopwatch.ElapsedMilliseconds,
                CpuUsage = realCpu,
                RamUsage = realRamPercent, // Veri artık yüzde olarak kaydediliyor!
                FreeDiskGb = realDisk,
                DependencyDetails = dependencyDetailsJson
            };

            // Veritabanına fiziksel kaydı yapıyoruz.
            dbContext.HealthSnapshots.Add(snapshot);
            await dbContext.SaveChangesAsync(token);

            // Konsola yazdırırken artık "RAM: 45MB" değil "RAM: %45" yazdırıyoruz
            _logger.LogInformation("{AppName} kontrol edildi. Durum: {Status}. CPU: %{Cpu}, RAM: %{Ram}", app.Name, currentStatus, realCpu, realRamPercent);

            //  Kural motorunu tetikler (Incident/Email kontrolü)
            await analyzeUseCase.ExecuteAsync(snapshot);

            // Canlı yayını tetikler (React arayüzü için)
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("BroadcastNewStatus", snapshot, token);
                // Console'da kalabalık yapmaması için buraya istersen _logger koymayabilirsin, ama verinin gittiğinden eminiz.
            }
        }
    }
}