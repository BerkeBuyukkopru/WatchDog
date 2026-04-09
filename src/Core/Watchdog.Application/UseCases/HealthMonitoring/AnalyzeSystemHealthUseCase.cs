using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.UseCases.AI;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;
using Watchdog.Domain.Rules;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    public class AnalyzeSystemHealthUseCase : IUseCaseAsync<HealthSnapshot>
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly INotificationSender _notificationSender;
        private readonly IMonitoredAppRepository _appRepository;

        // KRİTİK EKLENTİ: Arka planda (Task.Run) yeni veritabanı bağlantıları açabilmek için fabrika
        private readonly IServiceScopeFactory _scopeFactory;

        // Not: AI ve Prompt servislerini artık kurucuda değil, TriggerRootCauseAnalysisAsync içinde yeni Scope üzerinden çağıracağız. Aksi takdirde ObjectDisposedException alırız.

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository,
            IServiceScopeFactory scopeFactory)
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
            _scopeFactory = scopeFactory;
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
                    // --- KURUMSAL LOG GÜNCELLEMESİ ---
                    Console.WriteLine($">>>> [INCIDENT-TRIGGER] [CRITICAL] {app.Name} ÜST ÜSTE 3 KEZ YANIT VERMEDİ! Olay başlatılıyor...");

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

                    // Arka plana atıyoruz ama artık kendi scope'unu yaratacak
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
            // HER ŞEY TRY-CATCH İÇİNDE! KAÇIŞ YOK.
            try
            {
                // SCOPE YARATIMI: Ana thread burayı terk etse bile bu using bloğu sayesinde 
                // veritabanı bağlantımız arka planda güvende ve açık kalır.
                using var scope = _scopeFactory.CreateScope();

                var insightRepository = scope.ServiceProvider.GetRequiredService<IAiInsightRepository>();
                var aiProviderRepository = scope.ServiceProvider.GetRequiredService<IAiProviderRepository>();
                var promptBuilder = scope.ServiceProvider.GetRequiredService<IPromptBuilder>();
                var aiClientFactory = scope.ServiceProvider.GetRequiredService<IAiClientFactory>();

                // --- KURUMSAL LOG GÜNCELLEMESİ ---
                Console.WriteLine($">>>> [RCA-START] {app.Name} için Kök Neden Analizi süreci başladı...");

                var lastInsight = await insightRepository.GetLatestInsightAsync(app.Id);
                if (lastInsight != null && lastInsight.CreatedAt > DateTime.UtcNow.AddMinutes(-5))
                {
                    Console.WriteLine($">>>> [RCA-SKIP] Cooldown aktif. Son 5 dakika içinde analiz yapılmış.");
                    return;
                }

                var activeProviderEntity = await aiProviderRepository.GetActiveProviderAsync();
                string activeProvider = activeProviderEntity?.Name ?? "Ollama";

                var prompt = promptBuilder.BuildRootCausePrompt(recentSnapshots, app.Name);

                Console.WriteLine($">>>> [RCA-REQUEST] Yapay Zeka motoruna istek atılıyor...");
                var aiClient = await aiClientFactory.CreateClientAsync();
                var aiResponse = await aiClient.AnalyzeAsync(prompt);

                // --- KURUMSAL LOG GÜNCELLEMESİ ---
                Console.WriteLine($">>>> [RCA-REPORT] {app.Name} Kriz Analiz Raporu:\n{aiResponse}\n--------------------------------------------------");

                var newInsight = new AiInsight
                {
                    AppId = app.Id,
                    AiProviderId = activeProviderEntity?.Id,
                    InsightType = InsightType.CrashWarning,
                    Message = aiResponse,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow
                };

                await insightRepository.AddAsync(newInsight);
                Console.WriteLine($">>>> [RCA-SUCCESS] Analiz tamamlandı ve veritabanına kaydedildi!");
            }
            catch (Exception ex)
            {
                // YAKALANDIN! Hatayı yutmayıp konsola basıyoruz.
                Console.WriteLine($">>>> [RCA-FATAL-ERROR] Analiz tamamen patladı: {ex.ToString()}");
            }
        }
    }
}