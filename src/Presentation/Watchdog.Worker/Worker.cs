using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;
using Watchdog.Infrastructure.Persistence;

namespace Watchdog.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;

        // POLLY: Güvenlik Kalkanımız
        private readonly AsyncTimeoutPolicy _timeoutPolicy;

        // Çoklu Görev Yöneticisi: Hangi uygulamanın motoru çalışıyor takip etmek için (UC-1 Silme İşlemi Entegrasyonu)
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeTasks = new();

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpClient = new HttpClient();

            // POLLY KURALI: Bir siteye ping attığımızda 5 saniye içinde cevap gelmezse bekleme, fişini çek! (Timeout)
            _timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Pessimistic);
        }

        // --- 1. ANA YÖNETİCİ DÖNGÜ (Orkestra Şefi) ---
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Watchdog Asenkron Tarama Motoru (V2) Başladı!");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<WatchdogDbContext>();

                    // --- YAZILIMCI HİLESİ (KENDİ API'MİZİ EKLİYORUZ) ---
                    // Arkadaşın test etmek isterse bu yorum satırını açabilir.
                    
                   /* if (!await dbContext.MonitoredApps.AnyAsync(a => a.Name == "Benim Sensörlü API'm", stoppingToken))
                    {
                        var myApi = new MonitoredApp
                        {
                            Name = "Benim Sensörlü API'm",
                            HealthUrl = "https://localhost:7054/health",
                            PollingIntervalSeconds = 10,
                            ApiKey = Guid.NewGuid().ToString(),
                            CreatedAt = DateTime.UtcNow
                        };
                        dbContext.MonitoredApps.Add(myApi);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Sensörlü API veritabanına eklendi!");
                    }
                    */

                    var appsInDb = await dbContext.MonitoredApps.ToListAsync(stoppingToken);

                    // A) YENİ EKLENEN UYGULAMALARI BUL VE MOTORLARINI ÇALIŞTIR
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

                    // B) SİLİNEN UYGULAMALARI BUL VE MOTORLARINI DURDUR
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
                    var dbContext = scope.ServiceProvider.GetRequiredService<WatchdogDbContext>();
                    var app = await dbContext.MonitoredApps.FindAsync(new object[] { appId }, token);

                    if (app == null) break;

                    intervalSeconds = app.PollingIntervalSeconds;

                    await CheckAppHealthAsync(app, dbContext, token);
                }

                await Task.Delay(intervalSeconds * 1000, token);
            }
        }

        // --- 3. PING ATMA VE JSON OKUMA METODU (V3) ---
        private async Task CheckAppHealthAsync(MonitoredApp app, WatchdogDbContext dbContext, CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();
            HealthStatus currentStatus;

            double realCpu = 0;
            double realRamPercent = 0; // YENİ: Artık yüzdeyi tutacak
            double realDisk = 0;
            string dependencyDetailsJson = null;

            try
            {
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
                        using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);

                        if (jsonDoc.RootElement.TryGetProperty("metrics", out var metricsElement))
                        {
                            if (metricsElement.TryGetProperty("cpu_usage", out var cpuProp)) realCpu = cpuProp.GetDouble();

                            // YENİ: JSON'dan "ram_usage_percent" değerini çekiyoruz
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
                    currentStatus = HealthStatus.Degraded;
                }
            }
            catch (TimeoutRejectedException)
            {
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

            dbContext.HealthSnapshots.Add(snapshot);
            await dbContext.SaveChangesAsync(token);

            // YENİ: Konsola yazdırırken artık "RAM: 45MB" değil "RAM: %45" yazdırıyoruz
            _logger.LogInformation("{AppName} kontrol edildi. Durum: {Status}. CPU: %{Cpu}, RAM: %{Ram}", app.Name, currentStatus, realCpu, realRamPercent);
        }
    }
}