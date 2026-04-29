using System;
using System.Collections.Generic;
using System.Linq; // OrderBy vb. için eklendi
using System.Threading.Tasks;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.AI
{
    // ARA FAZ: Günlük ve Haftalık Karşılaştırmalı Kapasite Tahmin Motoru (AIOps)
    public class GenerateStrategicInsightUseCase : IUseCaseAsync<GenerateStrategicInsightRequest, AiInsight?>
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IAiInsightRepository _insightRepository;
        private readonly IAiClientFactory _aiClientFactory;
        private readonly IPromptBuilder _promptBuilder;
        private readonly IAiProviderRepository _aiProviderRepository;
        private readonly IStatusBroadcaster _statusBroadcaster;

        public GenerateStrategicInsightUseCase(
            IMonitoredAppRepository appRepository,
            ISnapshotRepository snapshotRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory,
            IPromptBuilder promptBuilder,
            IAiProviderRepository aiProviderRepository,
            IStatusBroadcaster statusBroadcaster)
        {
            _appRepository = appRepository;
            _snapshotRepository = snapshotRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
            _promptBuilder = promptBuilder;
            _aiProviderRepository = aiProviderRepository;
            _statusBroadcaster = statusBroadcaster;
        }

        public async Task<AiInsight?> ExecuteAsync(GenerateStrategicInsightRequest request)
        {
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            // --- YENİ MANTIK: UYGULAMAYA ÖZEL AI SEÇİMİ ---
            AiProvider targetProviderEntity = null;
            if (app.ActiveAiProviderId.HasValue)
            {
                targetProviderEntity = await _aiProviderRepository.GetByIdAsync(app.ActiveAiProviderId.Value);
            }

            // Eğer uygulamaya özel seçilmemişse global aktif olanı bul (Yedek)
            if (targetProviderEntity == null)
            {
                targetProviderEntity = await _aiProviderRepository.GetActiveProviderAsync();
            }

            var dailyStats = await _snapshotRepository.GetDailyEnrichedSnapshotsAsync(request.AppId, 8);
            if (dailyStats == null || dailyStats.Count < 1) return null;

            var sortedStats = dailyStats.OrderByDescending(d => d.Date).ToList();
            var targetDay = sortedStats.FirstOrDefault();

            // --- ESNEK MANTIK: 7 gün öncesini ara, bulamazsan eldeki en eski veriyi al ---
            var targetBaselineDate = targetDay.Date.AddDays(-7).Date;
            var baselineDay = sortedStats
                .Where(d => d.Date.Date <= targetBaselineDate.AddDays(1) && d.Date != targetDay.Date)
                .OrderBy(d => Math.Abs((d.Date.Date - targetBaselineDate).Days))
                .FirstOrDefault();

            // Eğer 7 gün civarı veri yoksa, targetDay'den farklı herhangi bir günü al (Karşılaştırma için)
            if (baselineDay == null && sortedStats.Count > 1)
            {
                baselineDay = sortedStats.LastOrDefault(d => d.Date.Date < targetDay.Date.Date);
            }

            if (baselineDay == null) 
            {
                // Karşılaştıracak hiçbir geçmiş veri yoksa, tek gün üzerinden "Durum Raporu" üretelim
                baselineDay = targetDay; // Baseline'ı kendisi yapıp farkları 0 gösteririz
            }

            var last7Days = dailyStats.Where(d => d.Date > targetDay.Date.AddDays(-7)).ToList();
            double weeklyAvgCpu = Math.Round(last7Days.Average(d => d.AvgCpu), 2);
            double weeklyAvgRam = Math.Round(last7Days.Average(d => d.AvgRam), 2);

            string targetErrors = targetDay.TopErrors.Any() ? string.Join(", ", targetDay.TopErrors) : "None";
            string baselineErrors = baselineDay.TopErrors.Any() ? string.Join(", ", baselineDay.TopErrors) : "None";

            string aiPrompt = _promptBuilder.BuildStrategicPrompt(
                app, baselineDay, targetDay,
                weeklyAvgCpu, weeklyAvgRam,
                baselineErrors, targetErrors);

            // --- FACTORY'E UYGULAMANIN KENDİ AI ID'SİNİ GÖNDERİYORUZ ---
            var aiClient = await _aiClientFactory.CreateClientAsync(app.ActiveAiProviderId);
            var aiResponseText = await aiClient.AnalyzeAsync(aiPrompt);

            var insight = new AiInsight
            {
                AppId = request.AppId,
                AiProviderId = targetProviderEntity?.Id, // Analizi yapan gerçek AI kaydedildi
                InsightType = InsightType.StrategicForecast,
                Message = aiResponseText,
                Evidence = $"[Karşılaştırma] {baselineDay.Date:dd/MM} vs {targetDay.Date:dd/MM} | Haftalık Trend CPU: {weeklyAvgCpu}%"
            };

            await _insightRepository.AddAsync(insight);

            var newInsightDto = new Watchdog.Application.DTOs.AI.AiInsightDto
            {
                Id = insight.Id,
                AppName = app.Name,
                Message = insight.Message,
                Evidence = insight.Evidence,
                InsightType = insight.InsightType.ToString(),
                IsResolved = insight.IsResolved,
                CreatedAt = insight.CreatedAt
            };

            await _statusBroadcaster.BroadcastNewInsightAsync(newInsightDto);

            return insight;
        }
    }
}