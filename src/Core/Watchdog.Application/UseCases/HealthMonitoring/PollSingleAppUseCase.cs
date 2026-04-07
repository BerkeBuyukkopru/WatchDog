using System;
using System.Text.Json;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    // Sözleşmemizi imzaladık. Dışarıdan AppId alır, geriye HealthSnapshot döner.
    public class PollSingleAppUseCase : IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?>
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly IHealthProbeClient _probeClient;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IUseCaseAsync<HealthSnapshot> _analyzeUseCase;

        public PollSingleAppUseCase(
            IMonitoredAppRepository appRepository,
            IHealthProbeClient probeClient,
            ISnapshotRepository snapshotRepository,
            IUseCaseAsync<HealthSnapshot> analyzeUseCase) // UC-5 Kural Motoru
        {
            _appRepository = appRepository;
            _probeClient = probeClient;
            _snapshotRepository = snapshotRepository;
            _analyzeUseCase = analyzeUseCase;
        }

        public async Task<HealthSnapshot?> ExecuteAsync(PollSingleAppRequest request)
        {
            // 1. Uygulamayı veritabanından bul
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            // --- DEĞİŞİKLİK BURADA: Değişkenleri dışarıda tanımlıyoruz ---
            HealthStatus finalStatus = HealthStatus.Unhealthy;
            long finalDuration = 0;
            string errorOrJson = "";
            double realCpu = 0, realRamPercent = 0, realDisk = 0;

            try
            {
                // 2. Altyapı Elçimize (Infrastructure) ping attır
                var probeResult = await _probeClient.CheckHealthAsync(app.HealthUrl, request.CancellationToken);

                finalStatus = probeResult.Status;
                finalDuration = probeResult.DurationMilliseconds;
                errorOrJson = probeResult.JsonContent;

                // 3. Başarılıysa JSON verisini ayrıştır (Parse)
                if (finalStatus == HealthStatus.Healthy && !string.IsNullOrEmpty(errorOrJson))
                {
                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(errorOrJson);
                        if (jsonDoc.RootElement.TryGetProperty("metrics", out var metricsElement))
                        {
                            if (metricsElement.TryGetProperty("system_cpu_percent", out var cpuProp)) realCpu = cpuProp.GetDouble();
                            if (metricsElement.TryGetProperty("system_ram_percent", out var ramProp)) realRamPercent = ramProp.GetDouble();
                            if (metricsElement.TryGetProperty("free_disk_gb", out var diskProp)) realDisk = diskProp.GetDouble();
                        }
                    }
                    catch { /* JSON okunamasa bile uygulamanın ayakta olduğunu biliyoruz. */ }
                }
            }
            catch (Exception ex)
            {
                // İŞTE KRİTİK NOKTA! Ağ hatası alırsak program çökmeyecek. 
                // Durumu Unhealthy yapıp, hatayı AI'a göndermek üzere kaydedeceğiz.
                finalStatus = HealthStatus.Unhealthy;
                errorOrJson = $"Kritik Ağ Hatası (DNS/Bağlantı): {ex.Message}";
                Console.WriteLine($">>>> [NETWORK FAIL] {app.Name} pinglenemedi! Hata yutuldu ve AI'a paslanacak.");
            }

            // 4. Veritabanına kaydedilecek Snapshot nesnesini hazırla
            var snapshot = new HealthSnapshot
            {
                AppId = app.Id,
                Timestamp = DateTime.UtcNow,
                Status = finalStatus,
                TotalDuration = finalDuration,
                CpuUsage = realCpu,
                RamUsage = realRamPercent,
                FreeDiskGb = realDisk,
                DependencyDetails = errorOrJson // Yapay Zeka buradaki hatayı okuyup yorumlayacak!
            };

            // 5. Veritabanına kaydet 
            await _snapshotRepository.AddAsync(snapshot);

            // 6. Kesinti var mı diye AI / Kural Motorunu tetikle
            await _analyzeUseCase.ExecuteAsync(snapshot);

            // 7. React'a yayınlaması için sonucu Worker'a geri dön
            return snapshot;
        }
    }
}