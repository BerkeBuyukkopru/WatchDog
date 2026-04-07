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

        public GenerateStrategicInsightUseCase(
            IMonitoredAppRepository appRepository,
            ISnapshotRepository snapshotRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory)
        {
            _appRepository = appRepository;
            _snapshotRepository = snapshotRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
        }

        public async Task<AiInsight?> ExecuteAsync(GenerateStrategicInsightRequest request)
        {
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            // Veriyi Çek: Son 8 günün (Dün ve geçen haftanın aynı gününü kapsayacak şekilde) özetini al.
            var dailyStats = await _snapshotRepository.GetDailyEnrichedSnapshotsAsync(request.AppId, 8);
            if (dailyStats == null || dailyStats.Count < 2) return null; // Karşılaştırma yapacak kadar veri yoksa çık.

            // Günleri Eşleştir (Target vs Baseline)
            // Sistem gece çalıştığı için "Dün" bizim hedef (Target) günümüzdür.
            var targetDay = dailyStats.OrderByDescending(d => d.Date).FirstOrDefault();

            // Geçen haftanın aynı günü (Baseline)
            var baselineDay = dailyStats.FirstOrDefault(d => d.Date.Date == targetDay.Date.AddDays(-7).Date);

            // Eğer geçen haftanın verisi yoksa (uygulama yeni eklenmişse) analizi atla.
            if (baselineDay == null) return null;

            // Haftalık Genel Trend Hesaplaması (Son 7 günün ortalamaları)
            var last7Days = dailyStats.Where(d => d.Date > targetDay.Date.AddDays(-7)).ToList();
            double weeklyAvgCpu = Math.Round(last7Days.Average(d => d.AvgCpu), 2);
            double weeklyAvgRam = Math.Round(last7Days.Average(d => d.AvgRam), 2);

            // Hataları Birleştir (Prompt'a göndermek için)
            string targetErrors = targetDay.TopErrors.Any() ? string.Join(", ", targetDay.TopErrors) : "None";
            string baselineErrors = baselineDay.TopErrors.Any() ? string.Join(", ", baselineDay.TopErrors) : "None";

            // Kurumsal AIOps Promptu (Karşılaştırmalı ve Tahminleyici)
            string aiPrompt = $@"
                SYSTEM ROLE: You are an advanced AIOps and Capacity Planning AI. Your goal is to analyze historical trends, identify anomalies between matching days, and provide capacity forecasts.

                STRICT RULES:
                - Output ONLY the three requested sections below. No greetings, no markdown blocks around the text.
                - Keep it highly professional and technical.

                [COMPARATIVE DATA (Day-Over-Day)]
                App: {app.Name}
                Baseline (Last Week {baselineDay.Date:dddd}): Avg CPU: {baselineDay.AvgCpu}%, Max CPU: {baselineDay.MaxCpu}% | Avg RAM: {baselineDay.AvgRam}%, Max RAM: {baselineDay.MaxRam}% | Top Errors: {baselineErrors}
                Target (Yesterday {targetDay.Date:dddd}): Avg CPU: {targetDay.AvgCpu}%, Max CPU: {targetDay.MaxCpu}% (Peak at {targetDay.PeakHour}) | Avg RAM: {targetDay.AvgRam}%, Max RAM: {targetDay.MaxRam}% | Top Errors: {targetErrors}

                [WEEKLY TREND]
                7-Day Rolling Averages -> CPU: {weeklyAvgCpu}%, RAM: {weeklyAvgRam}%

                [REQUIRED OUTPUT FORMAT]
                COMPARATIVE ROOT CAUSE: (Compare Baseline vs Target. Identify why CPU/RAM changed or why specific errors occurred during the Peak Hour.)
                WEEKLY FORECAST: (Based on the 7-day trend and this specific day's behavior, forecast the resource risk for next week.)
                STRATEGIC RECOMMENDATION: (Provide 1-2 architectural or scaling recommendations to handle future load.)
";

            var aiClient = await _aiClientFactory.CreateClientAsync();
            var aiResponseText = await aiClient.AnalyzeAsync(aiPrompt);

            // Sonucu Kaydet (Yeni InsightType.StrategicForecast ile)
            var insight = new AiInsight
            {
                AppId = request.AppId,
                InsightType = InsightType.StrategicForecast, // YENİ TÜR (Sarı/Vizyoner)
                Message = aiResponseText,
                Evidence = $"[Karşılaştırma] {baselineDay.Date:dd/MM} vs {targetDay.Date:dd/MM} | Haftalık Trend CPU: {weeklyAvgCpu}%"
            };

            await _insightRepository.AddAsync(insight);
            return insight;
        }
    }
}
