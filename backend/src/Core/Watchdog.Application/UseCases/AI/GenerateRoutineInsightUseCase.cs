using System;
using System.Collections.Generic;
using System.Linq; // Pencere filtrelemeleri (Where) için eklendi
using System.Threading.Tasks;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.AI
{
    // Saatlik Rutin Kapasite Analizi Senaryosu.
    // Bu sınıf Worker'dan (veya API'den) tetiklenir, son 24 saatin loglarını toplayıp AI Fabrikasına gönderir ve gelen cevabı AiInsight tablosuna kaydeder.
    public class GenerateRoutineInsightUseCase : IUseCaseAsync<GenerateRoutineInsightRequest, AiInsight?>
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IAiInsightRepository _insightRepository;
        private readonly IAiClientFactory _aiClientFactory;
        private readonly ISystemConfigurationRepository _systemConfigRepository;
        private readonly IAiProviderRepository _aiProviderRepository;
        private readonly IPromptBuilder _promptBuilder;
        private readonly IStatusBroadcaster _statusBroadcaster;

        public GenerateRoutineInsightUseCase(
            IMonitoredAppRepository appRepository,
            ISnapshotRepository snapshotRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory,
            ISystemConfigurationRepository systemConfigRepository,
            IAiProviderRepository aiProviderRepository,
            IPromptBuilder promptBuilder,
            IStatusBroadcaster statusBroadcaster)
        {
            _appRepository = appRepository;
            _snapshotRepository = snapshotRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
            _systemConfigRepository = systemConfigRepository;
            _aiProviderRepository = aiProviderRepository;
            _promptBuilder = promptBuilder;
            _statusBroadcaster = statusBroadcaster;
        }

        public async Task<AiInsight?> ExecuteAsync(GenerateRoutineInsightRequest request)
        {
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            // 1. Eşik Değerlerini Çek
            var config = await _systemConfigRepository.GetAsync();
            double cpuLimit = config?.CriticalCpuThreshold ?? 90.0;
            double ramLimit = config?.CriticalRamThreshold ?? 90.0;
            double latencyLimit = config?.CriticalLatencyThreshold ?? 1000.0;

            // 2. Aktif Yapay Zekayı Çek (UYGULAMAYA ÖZEL VEYA GLOBAL)
            AiProvider targetProviderEntity = null;
            if (app.ActiveAiProviderId.HasValue)
            {
                targetProviderEntity = await _aiProviderRepository.GetByIdAsync(app.ActiveAiProviderId.Value);
            }

            // Eğer uygulamaya özel seçilmemişse global olanı kayıtlara geçmek için bul
            if (targetProviderEntity == null)
            {
                targetProviderEntity = await _aiProviderRepository.GetActiveProviderAsync();
            }

            var sinceTime = DateTime.UtcNow.AddHours(-request.HoursToAnalyze);
            var snapshots = await _snapshotRepository.GetSnapshotsSinceAsync(request.AppId, sinceTime);

            if (snapshots == null || !snapshots.Any()) return null;

            // --- KESİNTİ KONTROLÜ (Ölü Adamın Nabzı Sorunu Çözümü) ---
            int outageCount = snapshots.Count(s => s.Status == HealthStatus.Unhealthy);
            bool hasOutages = outageCount > 0;

            double avgCpu24h = Math.Round((double)snapshots.Average(s => s.CpuUsage), 2);
            double avgRam24h = Math.Round((double)snapshots.Average(s => s.RamUsage), 2);
            double avgLatency24h = Math.Round((double)snapshots.Average(s => s.TotalDuration), 2);

            var twoHoursAgo = DateTime.UtcNow.AddHours(-2);
            var snapshots2h = snapshots.Where(s => s.Timestamp >= twoHoursAgo).ToList();

            double avgCpu2h = snapshots2h.Any() ? Math.Round((double)snapshots2h.Average(s => s.CpuUsage), 2) : avgCpu24h;
            double avgRam2h = snapshots2h.Any() ? Math.Round((double)snapshots2h.Average(s => s.RamUsage), 2) : avgRam24h;
            double avgLatency2h = snapshots2h.Any() ? Math.Round((double)snapshots2h.Average(s => s.TotalDuration), 2) : avgLatency24h;

            var snapshotsForPeak = snapshots2h.Any() ? snapshots2h : snapshots.Take(20).ToList();

            var peakCpuRecord = snapshotsForPeak.OrderByDescending(s => s.CpuUsage).First();
            double maxCpu2h = Math.Round((double)peakCpuRecord.CpuUsage, 2);
            string peakCpuTime = peakCpuRecord.Timestamp.ToLocalTime().ToString("HH:mm");

            double maxRam2h = Math.Round((double)snapshotsForPeak.Max(s => s.RamUsage), 2);
            double maxLatency2h = Math.Round((double)snapshotsForPeak.Max(s => s.TotalDuration), 2);

            var dependencyIssues = snapshots
                .Where(s => !string.IsNullOrWhiteSpace(s.DependencyDetails) && s.DependencyDetails.Contains("Unhealthy", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.DependencyDetails)
                .Distinct()
                .Take(5)
                .ToList();

            bool isCpuStable = avgCpu2h < cpuLimit && avgCpu24h < cpuLimit && maxCpu2h < (cpuLimit + 15);
            bool isRamStable = avgRam2h < ramLimit && avgRam24h < ramLimit;
            bool isLatencyStable = avgLatency2h < latencyLimit;
            bool isDependenciesClean = !dependencyIssues.Any();

            // SADECE her şey temizse VE hiç kesinti yaşanmadıysa statik mesaj dön.
            if (isCpuStable && isRamStable && isLatencyStable && isDependenciesClean && !hasOutages)
            {
                var stableInsight = new AiInsight
                {
                    AppId = request.AppId,
                    AiProviderId = targetProviderEntity?.Id, // Hangi AI'nın aktif olduğu bilgisi mühürlendi
                    InsightType = InsightType.SystemStable,
                    Message = $"STATUS: STABLE. Sistem metrikleri Dashboard üzerinde belirlenen ({cpuLimit}% CPU, {ramLimit}% RAM, {latencyLimit}ms Latency) sınırlarının altındadır. Önemli bir anlık patlama (spike) gözlenmedi.",
                    Evidence = $"CPU Avg/Max: {avgCpu2h}%/{maxCpu2h}% | RAM Avg/Max: {avgRam2h}%/{maxRam2h}% | Latency Peak: {maxLatency2h}ms | Bağımlılıklar: Temiz"
                };

                await _insightRepository.AddAsync(stableInsight);
                await _statusBroadcaster.BroadcastNewInsightAsync(stableInsight);

                return stableInsight;
            }

            string dependencyContext = dependencyIssues.Any()
                            ? "STATUS: DEGRADED | ERRORS: " + string.Join(", ", dependencyIssues)
                            : "STATUS: HEALTHY | ALL SUB-SERVICES OPERATIONAL";

            string aiPrompt = _promptBuilder.BuildRoutinePrompt(
                app, cpuLimit, ramLimit, latencyLimit,
                avgCpu24h, avgRam24h, avgLatency24h,
                avgCpu2h, avgRam2h, avgLatency2h,
                maxCpu2h, maxRam2h, maxLatency2h,
                peakCpuTime, dependencyContext,
                outageCount);

            // 3. FACTORY'E UYGULAMANIN KENDİ AI TERCİHİNİ GÖNDERİYORUZ
            var aiClient = await _aiClientFactory.CreateClientAsync(app.ActiveAiProviderId);
            var aiResponseText = await aiClient.AnalyzeAsync(aiPrompt);

            var insight = new AiInsight
            {
                AppId = request.AppId,
                AiProviderId = targetProviderEntity?.Id, // Analizi yapan sağlayıcı kimliği kaydedildi
                InsightType = InsightType.ScalingAdvice,
                Message = aiResponseText,
                Evidence = $"CPU Peak: {maxCpu2h}% at {peakCpuTime} | Kesinti Sayısı: {outageCount} | Bağımlılıklar: {(dependencyIssues.Any() ? "Sorunlu" : "Temiz")}"
            };

            await _insightRepository.AddAsync(insight);
            await _statusBroadcaster.BroadcastNewInsightAsync(insight);

            return insight;
        }
    }
}