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

        // HATA ÇÖZÜMÜ: Aktif sağlayıcıyı (OpenAI/Ollama) bulmak için config repository'si eklendi.
        private readonly ISystemConfigurationRepository _systemConfigRepository;

        public GenerateStrategicInsightUseCase(
            IMonitoredAppRepository appRepository,
            ISnapshotRepository snapshotRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory,
            IPromptBuilder promptBuilder,
            ISystemConfigurationRepository systemConfigRepository) // YENİ EKLENDİ
        {
            _appRepository = appRepository;
            _snapshotRepository = snapshotRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
            _promptBuilder = promptBuilder;
            _systemConfigRepository = systemConfigRepository; // YENİ EKLENDİ
        }

        public async Task<AiInsight?> ExecuteAsync(GenerateStrategicInsightRequest request)
        {
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            // Ekip Arkadaşıma Not: YENİ EKLENDİ. Hangi zekanın çalıştığını builder'a söylemek için config çekiyoruz.
            var config = await _systemConfigRepository.GetAsync();
            string activeProvider = config?.ActiveAiProvider ?? "Ollama";

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
            // Ekip Arkadaşıma Not: (REFACTORING YAPILDI)
            // Haftalık kapasite tahmin promptumuzu da builder nesnemizden alıyoruz. 
            // HATA ÇÖZÜMÜ: activeProvider parametresi en başa eklendi.
            string aiPrompt = _promptBuilder.BuildStrategicPrompt(
                activeProvider,
                app, baselineDay, targetDay,
                weeklyAvgCpu, weeklyAvgRam,
                baselineErrors, targetErrors);

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
