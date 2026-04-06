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
    // UC-7: Saatlik Rutin Kapasite Analizi Senaryosu.
    // Bu sınıf Worker'dan (veya API'den) tetiklenir, son 1 saatin loglarını toplayıp AI Fabrikasına gönderir ve gelen cevabı AiInsight tablosuna kaydeder. Dış dünya (Ollama/OpenAI) bağımsızdır.
    public class GenerateRoutineInsightUseCase : IUseCaseAsync<GenerateRoutineInsightRequest, AiInsight?>
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IAiInsightRepository _insightRepository;
        private readonly IAiClientFactory _aiClientFactory;

        public GenerateRoutineInsightUseCase(
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

        public async Task<AiInsight?> ExecuteAsync(GenerateRoutineInsightRequest request)
        {
            // Uygulamanın sistemde olup olmadığını kontrol et
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            // İstenen zaman dilimine ait geçmiş sağlık verilerini (logları) çek
            var sinceTime = DateTime.UtcNow.AddHours(-request.HoursToAnalyze);
            var snapshots = await _snapshotRepository.GetSnapshotsSinceAsync(request.AppId, sinceTime);

            // Eğer hiç kayıt yoksa AI'ı yormaya gerek yok
            if (snapshots == null || !snapshots.Any()) return null;

            // Domain/Entities/HealthSnapshot.cs içindeki GERÇEK property isimleri kullanıldı.
            double avgCpu = Math.Round(snapshots.Average(s => s.CpuUsage), 2);
            double avgRam = Math.Round(snapshots.Average(s => s.RamUsage), 2);
            double avgResponseTime = Math.Round(snapshots.Average(s => s.TotalDuration), 2);

            // Alt Bağımlılık (Dependency) Özetleme Algoritması
            // Token tasarrufu yapmak ve AI'ı doğru yönlendirmek için sadece sorunlu (Unhealthy vb.) olan bağımlılıkları süzüyoruz.
            var dependencyIssues = snapshots
                .Where(s => !string.IsNullOrWhiteSpace(s.DependencyDetails) && s.DependencyDetails.Contains("Unhealthy", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.DependencyDetails)
                .Distinct() // Aynı hataları defalarca AI'a yollamamak için tekilleştir
                .Take(5) // En farklı 5 hatayı al
                .ToList();

            // 1. Bağımlılık metnini net, İngilizce log formatına çeviriyoruz
            string dependencyContext = dependencyIssues.Any()
                ? "STATUS: DEGRADED | ERRORS: " + string.Join(", ", dependencyIssues)
                : "STATUS: HEALTHY | ALL SUB-SERVICES OPERATIONAL";

            // 2. Kurumsal SRE (Site Reliability Engineering) Promptu
            string aiPrompt = $@"
                SYSTEM ROLE: You are an automated Site Reliability Engineering (SRE) diagnostic engine. Your ONLY purpose is to output a technical diagnostic report. 

                STRICT RULES:
                - DO NOT output any greetings, pleasantries, or introductory remarks (e.g., 'Here is the analysis', 'I have analyzed').
                - DO NOT output any concluding remarks.
                - OUTPUT ONLY the three requested sections below. Do not add formatting like markdown code blocks (```) around the entire text.

                [TELEMETRY DATA]
                App: {app.Name}
                Avg CPU: {avgCpu}%
                Avg RAM: {avgRam} MB
                Avg Latency: {avgResponseTime} ms
                Dependencies: {dependencyContext}

                [REQUIRED OUTPUT FORMAT]
                ROOT CAUSE ANALYSIS: (Provide 1-2 sentences strictly evaluating the telemetry data.)
                CAPACITY STATUS: (Provide 1 sentence evaluating current resource load.)
                ACTIONABLE ADVICE: (Provide 1-2 direct technical steps for scaling, optimization, or maintenance.)
";

            // FACTORY DESENİ KULLANIMI: Sistemde o an aktif olan AI motorunu (OpenAI veya Ollama) iste
            var aiClient = await _aiClientFactory.CreateClientAsync();

            // Seçili motora (Interface üzerinden) soruyu sor ve cevabı bekle
            var aiResponseText = await aiClient.AnalyzeAsync(aiPrompt);

            // Gelen cevabı Domain Entity'sine (AiInsight) çevir
            var insight = new AiInsight
            {
                AppId = request.AppId,
                // Domain/Enums/InsightType.cs içindeki GERÇEK enum ismi kullanıldı.
                InsightType = InsightType.ScalingAdvice,
                Message = aiResponseText,
                Evidence = $"CPU: {avgCpu}%, RAM: {avgRam}MB | Bağımlılıklar: {(dependencyIssues.Any() ? "Sorunlu" : "Temiz")}"
            };

            // Tavsiyeyi veritabanına kaydet
            await _insightRepository.AddAsync(insight);

            return insight;
        }
    }
}
