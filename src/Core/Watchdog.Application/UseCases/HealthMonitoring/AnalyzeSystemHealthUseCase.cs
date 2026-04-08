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

        // (REFACTORING) Bağımlılık Enjeksiyonu (DI) için yeni arayüzler eklendi.
        private readonly IPromptBuilder _promptBuilder;
        private readonly ISystemConfigurationRepository _systemConfigRepository;

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory,
            IPromptBuilder promptBuilder, // YENİ
            ISystemConfigurationRepository systemConfigRepository) // YENİ
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
            _promptBuilder = promptBuilder; // YENİ
            _systemConfigRepository = systemConfigRepository; // YENİ
        }

        public async Task ExecuteAsync(HealthSnapshot latestSnapshot)
        {
            var app = await _appRepository.GetByIdAsync(latestSnapshot.AppId);
            if (app == null) return;

            // TEST İÇİN LOG: Her gelen pingi terminalde gör
            Console.WriteLine($">>>> [MONITOR] {app.Name} Pinglendi. Durum: {latestSnapshot.Status}");

            var activeIncident = await _incidentRepository.GetActiveIncidentAsync(app.Id);
            bool hasActiveIncident = activeIncident != null;

            if (!hasActiveIncident && latestSnapshot.Status == HealthStatus.Unhealthy)
            {
                // Son 3 kaydı çekiyoruz (3-Strike Kuralı için)
                var recentSnapshots = await _snapshotRepository.GetLatestSnapshotsAsync(app.Id, 3);

                if (IncidentRules.ShouldOpenIncident(recentSnapshots, hasActiveIncident))
                {
                    // KRİTİK LOG: 3-Strike doldu!
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

                    // --- YAPAY ZEKA TETİKLENİYOR ---
                    // Arka planda çalışması için Task.Run kullanıyoruz
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

            // WDG049: COOLDOWN KONTROLÜ
            var lastInsight = await _insightRepository.GetLatestInsightAsync(app.Id);
            if (lastInsight != null && lastInsight.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
            {
                Console.WriteLine($">>>> [AI-SKIP] Cooldown aktif. Son 5 dakika içinde analiz yapılmış.");
                return;
            }

            try
            {
                // (REFACTORING) Manuel "new PromptBuilder()" silindi. Artık Dependency Injection (SOLID) ile IPromptBuilder kullanıyoruz.
                // Dual-Mode (Dil kuralı) için veritabanından aktif sağlayıcıyı okuyoruz.
                var config = await _systemConfigRepository.GetAsync();
                string activeProvider = config?.ActiveAiProvider ?? "Ollama";

                // HATA ÇÖZÜMÜ: activeProvider parametresi metodun en başına eklendi.
                var prompt = _promptBuilder.BuildRootCausePrompt(activeProvider, recentSnapshots, app.Name);

                Console.WriteLine($">>>> [AI-REQUEST] Yapay Zeka motoruna istek atılıyor...");
                var aiClient = await _aiClientFactory.CreateClientAsync();
                var aiResponse = await aiClient.AnalyzeAsync(prompt);

                var newInsight = new AiInsight
                {
                    AppId = app.Id,
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

                // FALLBACK
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