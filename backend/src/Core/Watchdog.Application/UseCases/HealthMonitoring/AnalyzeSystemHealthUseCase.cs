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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IStatusBroadcaster _statusBroadcaster;

        // YENİ EKLENEN: Sorumlu Adminleri bulmak için AuthRepository'i ekliyoruz.
        private readonly IAuthRepository _authRepository;

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository,
            IServiceScopeFactory scopeFactory,
            IStatusBroadcaster statusBroadcaster,
            IAuthRepository authRepository) // Constructora Eklendi
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
            _scopeFactory = scopeFactory;
            _statusBroadcaster = statusBroadcaster;
            _authRepository = authRepository;
        }

        public async Task ExecuteAsync(HealthSnapshot latestSnapshot)
        {
            await _statusBroadcaster.BroadcastNewStatusAsync(latestSnapshot);

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

                    // --- YENİ MAİL MANTIĞI: SADECE SORUMLU ADMİNLERİ BUL VE MAİL AT ---
                    await SendAlertToResponsibleAdminsAsync(app, "🚨 KRİTİK KESİNTİ", $"{app.Name} uygulaması an itibarıyla çöktü. Hata: {newIncident.ErrorMessage}");

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

                    // --- YENİ MAİL MANTIĞI: İYİLEŞMEYİ ADMİNLERE BİLDİR ---
                    await SendAlertToResponsibleAdminsAsync(app, "✅ SİSTEM DÜZELDİ", $"{app.Name} uygulaması tekrar sağlıklı duruma döndü.");
                }
            }
        }

        // Kodu kirletmemek için mail gönderme işini küçük bir metoda aldık:
        private async Task SendAlertToResponsibleAdminsAsync(MonitoredApp app, string subject, string message)
        {
            var responsibleAdmins = await _authRepository.GetAdminsByAppIdAsync(app.Id);

            var adminEmails = responsibleAdmins
                .Where(a => !string.IsNullOrEmpty(a.Email))
                .Select(a => a.Email)
                .ToList();

            if (adminEmails.Any())
            {
                string toEmails = string.Join(",", adminEmails);
                await _notificationSender.SendEmailAsync(toEmails, subject, message);
                Console.WriteLine($">>>> [MAIL] Uyarı {adminEmails.Count} sorumlu admine gönderildi.");
            }
        }

        private async Task TriggerRootCauseAnalysisAsync(MonitoredApp app, List<HealthSnapshot> recentSnapshots)
        {
            // (Burası aynı kaldı, değiştirmedim)
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var insightRepository = scope.ServiceProvider.GetRequiredService<IAiInsightRepository>();
                var aiProviderRepository = scope.ServiceProvider.GetRequiredService<IAiProviderRepository>();
                var promptBuilder = scope.ServiceProvider.GetRequiredService<IPromptBuilder>();
                var aiClientFactory = scope.ServiceProvider.GetRequiredService<IAiClientFactory>();

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
                var aiClient = await aiClientFactory.CreateClientAsync(app.ActiveAiProviderId);
                var aiResponse = await aiClient.AnalyzeAsync(prompt);

                Console.WriteLine($">>>> [RCA-REPORT] {app.Name} Kriz Analiz Raporu:\n{aiResponse}\n--------------------------------------------------");

                var newInsight = new AiInsight
                {
                    AppId = app.Id,
                    AiProviderId = activeProviderEntity?.Id,
                    InsightType = InsightType.CrashWarning,
                    Message = aiResponse,
                    IsResolved = false
                };

                await insightRepository.AddAsync(newInsight);

                var statusBroadcaster = scope.ServiceProvider.GetRequiredService<IStatusBroadcaster>();
                var newInsightDto = new Watchdog.Application.DTOs.AI.AiInsightDto
                {
                    Id = newInsight.Id,
                    AppName = app.Name,
                    Message = newInsight.Message,
                    InsightType = newInsight.InsightType.ToString(),
                    IsResolved = newInsight.IsResolved,
                    CreatedAt = newInsight.CreatedAt
                };

                await statusBroadcaster.BroadcastNewInsightAsync(newInsightDto);

                Console.WriteLine($">>>> [RCA-SUCCESS] Analiz tamamlandı ve veritabanına kaydedildi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>>> [RCA-FATAL-ERROR] Analiz tamamen patladı: {ex.ToString()}");
            }
        }
    }
}