using System;
using System.Text.Json;
using System.Threading.Tasks;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases
{
    // YENİ: Sözleşmemizi imzaladık. Dışarıdan AppId alır, geriye HealthSnapshot döner.
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

            // 2. Altyapı Elçimize (Infrastructure) ping attır
            var probeResult = await _probeClient.CheckHealthAsync(app.HealthUrl, request.CancellationToken);

            // 3. Başarılıysa JSON verisini ayrıştır (Parse) - İş Kuralları burada başlar
            double realCpu = 0, realRamPercent = 0, realDisk = 0;

            if (probeResult.Status == HealthStatus.Healthy && !string.IsNullOrEmpty(probeResult.JsonContent))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(probeResult.JsonContent);
                    if (jsonDoc.RootElement.TryGetProperty("metrics", out var metricsElement))
                    {
                        if (metricsElement.TryGetProperty("system_cpu_percent", out var cpuProp)) realCpu = cpuProp.GetDouble();
                        if (metricsElement.TryGetProperty("system_ram_percent", out var ramProp)) realRamPercent = ramProp.GetDouble();
                        if (metricsElement.TryGetProperty("free_disk_gb", out var diskProp)) realDisk = diskProp.GetDouble();
                    }
                }
                catch
                {
                    // JSON okunamasa bile uygulamanın ayakta olduğunu biliyoruz.
                }
            }

            // 4. Veritabanına kaydedilecek Snapshot nesnesini hazırla
            var snapshot = new HealthSnapshot
            {
                AppId = app.Id,
                Timestamp = DateTime.UtcNow,
                Status = probeResult.Status,
                TotalDuration = probeResult.DurationMilliseconds,
                CpuUsage = realCpu,
                RamUsage = realRamPercent,
                FreeDiskGb = realDisk,
                DependencyDetails = probeResult.JsonContent
            };

            // 5. Veritabanına kaydet (Worker'daki DbContext yükünü Repository'ye devrettik)
            await _snapshotRepository.AddAsync(snapshot);

            // 6. Kesinti var mı diye AI / Kural Motorunu tetikle
            await _analyzeUseCase.ExecuteAsync(snapshot);

            // 7. React'a yayınlaması için sonucu Worker'a geri dön
            return snapshot;
        }
    }
}