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
using Watchdog.Application.UseCases.AI; // ÇÖZÜM 1: PromptBuilder'ın tanınması için eklendi

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    // Sistem Sağlık Analizi. Worker'dan gelen her ping sonucu bu süzgeçten geçer.
    public class AnalyzeSystemHealthUseCase : IUseCaseAsync<HealthSnapshot>
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly INotificationSender _notificationSender;
        private readonly IMonitoredAppRepository _appRepository;

        // --- YAPAY ZEKA BAĞIMLILIKLARI ---
        private readonly IAiInsightRepository _insightRepository;
        private readonly IAiClientFactory _aiClientFactory;

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory)
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
        }

        public async Task ExecuteAsync(HealthSnapshot latestSnapshot)
        {
            var app = await _appRepository.GetByIdAsync(latestSnapshot.AppId);
            if (app == null) return;

            var activeIncident = await _incidentRepository.GetActiveIncidentAsync(app.Id);
            bool hasActiveIncident = activeIncident != null;

            if (!hasActiveIncident && latestSnapshot.Status == HealthStatus.Unhealthy)
            {
                var recentSnapshots = await _snapshotRepository.GetLatestSnapshotsAsync(app.Id, 3);

                if (IncidentRules.ShouldOpenIncident(recentSnapshots, hasActiveIncident))
                {
                    // 1. Çöküş tespit edildi! Yeni kayıt oluştur.
                    var newIncident = new Incident
                    {
                        AppId = app.Id,
                        StartedAt = DateTime.UtcNow,
                        ErrorMessage = string.IsNullOrEmpty(latestSnapshot.DependencyDetails)
                        ? "Sistem üst üste 3 kez yanıt vermedi."
                        : $"Sistem üst üste 3 kez yanıt vermedi: {latestSnapshot.DependencyDetails}"
                    };

                    await _incidentRepository.AddAsync(newIncident);
                    await _notificationSender.SendDowntimeAlertAsync(newIncident, app);

                    // --- 2. KRİZ ANINDA YAPAY ZEKAYI TETİKLE (Event-Driven) ---
                    // Döngüyü kilitlememesi için Task'ı beklemeden arka planda fırlatıyoruz.
                    _ = Task.Run(() => TriggerRootCauseAnalysisAsync(app, recentSnapshots));
                }
            }
            else if (hasActiveIncident)
            {
                if (IncidentRules.ShouldResolveIncident(latestSnapshot, hasActiveIncident))
                {
                    activeIncident.ResolvedAt = DateTime.UtcNow;
                    await _incidentRepository.UpdateAsync(activeIncident);
                    await _notificationSender.SendRecoveryAlertAsync(activeIncident, app);
                }
            }
        }

        // --- Geliştirici 2'nin Kriz Yönetimi (Kök Neden Analizi) Metodu ---
        private async Task TriggerRootCauseAnalysisAsync(MonitoredApp app, List<HealthSnapshot> recentSnapshots)
        {
            // WDG049 KURALI: COOLDOWN (Soğuma) KONTROLÜ
            // Son 5 dakika içinde bu uygulama için bir analiz üretilmişse, LLM'i yorma, işlemi atla!
            var lastInsight = await _insightRepository.GetLatestInsightAsync(app.Id);
            if (lastInsight != null && lastInsight.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
            {
                return; // Cooldown aktif, bypass et.
            }

            try
            {
                // 1. Prompt'u Hazırla (Logları Özetle)
                var promptBuilder = new PromptBuilder();
                var prompt = promptBuilder.BuildRootCausePrompt(recentSnapshots, app.Name);

                // 2. Geliştirici 1'in Fabrikasından Aktif LLM'i İste
                // ÇÖZÜM 2: Arkadaşının yazdığı interface'deki doğru metot adı kullanıldı!
                var aiClient = await _aiClientFactory.CreateClientAsync();

                // 3. Analizi Yap
                var aiResponse = await aiClient.AnalyzeAsync(prompt);

                // 4. Sonucu Veritabanına Yaz
                var newInsight = new AiInsight
                {
                    AppId = app.Id,
                    InsightType = InsightType.CrashWarning, // Çöküş uyarısı olduğu için kırmızı ikonla çıkacak
                    Message = aiResponse,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _insightRepository.AddAsync(newInsight);
            }
            catch
            {
                // WDG054 KURALI: FALLBACK (Yedek Plan)
                // LLM sunucusu çökerse sistem patlamasın, statik bir uyarı oluştur.
                var fallbackInsight = new AiInsight
                {
                    AppId = app.Id,
                    Message = "Yapay zeka analiz motoruna ulaşılamadı. Sistemde anlık kesinti var, acil donanım kontrolü önerilir.",
                    InsightType = InsightType.CrashWarning,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _insightRepository.AddAsync(fallbackInsight);
            }
        }
    }
}