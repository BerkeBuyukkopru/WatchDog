using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;
using Watchdog.Domain.Rules;
using Watchdog.Application.UseCases.AI;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    public class AnalyzeSystemHealthUseCase : IUseCaseAsync<HealthSnapshot>
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly INotificationSender _notificationSender;
        private readonly IMonitoredAppRepository _appRepository;
        private readonly IAiInsightRepository _insightRepository;
        private readonly IAiClientFactory _aiClientFactory;
        private readonly IPromptBuilder _promptBuilder;

        // REFACTORING: Artık SystemConfig'e değil, AI Registry'ye (AiProviders tablosuna) bakıyoruz.
        private readonly IAiProviderRepository _aiProviderRepository;

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory,
            IPromptBuilder promptBuilder,
            IAiProviderRepository aiProviderRepository) // YENİ
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
            _promptBuilder = promptBuilder;
            _aiProviderRepository = aiProviderRepository; // YENİ
        }

        public async Task ExecuteAsync(HealthSnapshot latestSnapshot)
        {
            var app = await _appRepository.GetByIdAsync(latestSnapshot.AppId);
            if (app == null) return;

            Console.WriteLine($">>>> [MONITOR] {app.Name} Pinglendi. Durum: {latestSnapshot.Status}");

            var activeIncident = await _incidentRepository.GetActiveIncidentAsync(app.Id);
            bool hasActiveIncident = activeIncident != null;

            if (!hasActiveIncident && latestSnapshot.Status == HealthStatus.Unhealthy)
            {
                var recentSnapshots = await _snapshotRepository.GetLatestSnapshotsAsync(app.Id, 3);

                if (IncidentRules.ShouldOpenIncident(recentSnapshots, hasActiveIncident))
                {
                    Console.WriteLine($">>>> [CRITICAL] {app.Name} ÜST ÜSTE 3 KEZ YANIT VERMEDİ! Olay başlatılıyor...");

                    var newIncident = new Incident
                    {
                        AppId = app.Id,
                        StartedAt = DateTime.UtcNow,
                        ErrorMessage = string.IsNullOrEmpty(latestSnapshot.DependencyDetails)
                        ? "Sistem yanıt vermiyor."
                        : $"Hata Detayı: {latestSnapshot.DependencyDetails}"
                    };

                    await _incidentRepository.AddAsync(newIncident);
                    await _notificationSender.SendDowntimeAlertAsync(newIncident, app);

                    _ = Task.Run(async () => await TriggerRootCauseAnalysisAsync(app, recentSnapshots));
                }
            }
            else if (hasActiveIncident && latestSnapshot.Status == HealthStatus.Healthy)
            {
                if (IncidentRules.ShouldResolveIncident(latestSnapshot, hasActiveIncident))
                {
                    Console.WriteLine($">>>> [RECOVERY] {app.Name} düzeldi. Olay kapatılıyor.");
                    activeIncident.ResolvedAt = DateTime.UtcNow;
                    await _incidentRepository.UpdateAsync(activeIncident);
                    await _notificationSender.SendRecoveryAlertAsync(activeIncident, app);
                }
            }
        }

        private async Task TriggerRootCauseAnalysisAsync(MonitoredApp app, List<HealthSnapshot> recentSnapshots)
        {
            Console.WriteLine($">>>> [AI-START] {app.Name} için Kök Neden Analizi süreci başladı...");

            var lastInsight = await _insightRepository.GetLatestInsightAsync(app.Id);
            if (lastInsight != null && lastInsight.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
            {
                Console.WriteLine($">>>> [AI-SKIP] Cooldown aktif. Son 5 dakika içinde analiz yapılmış.");
                return;
            }

            try
            {
                // YENİ MİMARİ: Hangi sağlayıcının (Ollama, Groq vs) aktif olduğunu veritabanından çekiyoruz.
                var activeProviderEntity = await _aiProviderRepository.GetActiveProviderAsync();
                string activeProvider = activeProviderEntity?.Name ?? "Ollama";

                var prompt = _promptBuilder.BuildRootCausePrompt(activeProvider, recentSnapshots, app.Name);

                Console.WriteLine($">>>> [AI-REQUEST] Yapay Zeka motoruna istek atılıyor...");
                var aiClient = await _aiClientFactory.CreateClientAsync();
                var aiResponse = await aiClient.AnalyzeAsync(prompt);

                var newInsight = new AiInsight
                {
                    AppId = app.Id,
                    AiProviderId = activeProviderEntity?.Id,
                    InsightType = InsightType.CrashWarning,
                    Message = aiResponse,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _insightRepository.AddAsync(newInsight);
                Console.WriteLine($">>>> [AI-SUCCESS] Analiz tamamlandı ve veritabanına kaydedildi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>>> [AI-ERROR] Analiz sırasında hata oluştu: {ex.Message}");

                var fallbackInsight = new AiInsight
                {
                    AppId = app.Id,
                    Message = "AI Motoru meşgul. Manuel kontrol önerilir.",
                    InsightType = InsightType.CrashWarning,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _insightRepository.AddAsync(fallbackInsight);
            }
        }
    }
}