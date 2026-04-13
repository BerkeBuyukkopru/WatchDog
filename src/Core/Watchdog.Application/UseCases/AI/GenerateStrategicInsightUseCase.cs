using System;
using System.Collections.Generic;
using System.Text;
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

        // Canlı yayın sözleşmesi
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

            // Hangi zekanın çalıştığını builder'a söylemek için yeni tablodan çekiyoruz.
            var activeProviderEntity = await _aiProviderRepository.GetActiveProviderAsync();
            string activeProvider = activeProviderEntity?.Name ?? "Ollama";

            var dailyStats = await _snapshotRepository.GetDailyEnrichedSnapshotsAsync(request.AppId, 8);
            if (dailyStats == null || dailyStats.Count < 2) return null;

            var targetDay = dailyStats.OrderByDescending(d => d.Date).FirstOrDefault();

            // --- KURUMSAL STANDART: Esnek Hedef Eşleştirme (Toleranslı Arama) ---
            var targetBaselineDate = targetDay.Date.AddDays(-7).Date;

            var baselineDay = dailyStats
                .Where(d => d.Date.Date >= targetBaselineDate.AddDays(-1) && d.Date.Date <= targetBaselineDate.AddDays(1)) // 6, 7 veya 8. güne esneklik tanı
                .OrderBy(d => Math.Abs((d.Date.Date - targetBaselineDate).Days)) // 7. güne en yakın olanı seç
                .FirstOrDefault();

            if (baselineDay == null) return null;

            var last7Days = dailyStats.Where(d => d.Date > targetDay.Date.AddDays(-7)).ToList();
            double weeklyAvgCpu = Math.Round(last7Days.Average(d => d.AvgCpu), 2);
            double weeklyAvgRam = Math.Round(last7Days.Average(d => d.AvgRam), 2);

            string targetErrors = targetDay.TopErrors.Any() ? string.Join(", ", targetDay.TopErrors) : "None";
            string baselineErrors = baselineDay.TopErrors.Any() ? string.Join(", ", baselineDay.TopErrors) : "None";

            string aiPrompt = _promptBuilder.BuildStrategicPrompt(
                app, baselineDay, targetDay,
                weeklyAvgCpu, weeklyAvgRam,
                baselineErrors, targetErrors);

            var aiClient = await _aiClientFactory.CreateClientAsync();
            var aiResponseText = await aiClient.AnalyzeAsync(aiPrompt);

            var insight = new AiInsight
            {
                AppId = request.AppId,
                InsightType = InsightType.StrategicForecast,
                Message = aiResponseText,
                Evidence = $"[Karşılaştırma] {baselineDay.Date:dd/MM} vs {targetDay.Date:dd/MM} | Haftalık Trend CPU: {weeklyAvgCpu}%"
            };

            await _insightRepository.AddAsync(insight);
            await _statusBroadcaster.BroadcastNewInsightAsync(insight);

            return insight;
        }
    }
}
