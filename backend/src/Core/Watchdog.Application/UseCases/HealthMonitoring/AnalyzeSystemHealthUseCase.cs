using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
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

            // 1. Bileşen bazlı analiz için verileri hazırla
            var componentsStatus = ParseComponentStatuses(latestSnapshot.DependencyDetails);
            
            // Eğer JSON parse edilemediyse veya boşsa (örn: Network Error), "System" olarak ele al
            if (!componentsStatus.Any())
            {
                componentsStatus["System"] = latestSnapshot.Status;
            }

            // 2. Her bir bileşen için durumu kontrol et
            foreach (var component in componentsStatus)
            {
                string componentName = component.Key;
                HealthStatus currentStatus = component.Value;

                var activeIncident = await _incidentRepository.GetActiveIncidentAsync(app.Id, componentName);
                bool hasActiveIncident = activeIncident != null;

                if (!hasActiveIncident && currentStatus == HealthStatus.Unhealthy)
                {
                    // 3-Strike Kontrolü: Bu bileşen son 3 snapshot'ta da mı Unhealthy?
                    var recentSnapshots = await _snapshotRepository.GetLatestSnapshotsAsync(app.Id, 3);
                    
                    if (ShouldTriggerIncidentForComponent(recentSnapshots, componentName))
                    {
                        Console.WriteLine($">>>> [INCIDENT-TRIGGER] {app.Name} - {componentName} ÜST ÜSTE 3 KEZ HATA VERDİ!");

                        var newIncident = new Incident
                        {
                            AppId = app.Id,
                            FailedComponent = componentName,
                            StartedAt = DateTime.UtcNow,
                            ErrorMessage = latestSnapshot.DependencyDetails // Tüm JSON veya hata mesajını sakla
                        };

                        await _incidentRepository.AddAsync(newIncident);

                        await SendAlertToResponsibleAdminsAsync(app, $"🚨 KESİNTİ: {componentName}", 
                            $"{app.Name} uygulamasında {componentName} bileşeni çöktü.\nDetay: {latestSnapshot.DependencyDetails}");

                        // Sadece ilk kritik hatada RCA tetikleyelim (Opsiyonel)
                        if (componentName == "System" || componentsStatus.Count == 1)
                        {
                             _ = Task.Run(async () => await TriggerRootCauseAnalysisAsync(app, recentSnapshots));
                        }
                    }
                }
                else if (hasActiveIncident && currentStatus == HealthStatus.Healthy)
                {
                    // İyileşme Kontrolü
                    Console.WriteLine($">>>> [RECOVERY] {app.Name} - {componentName} düzeldi.");
                    activeIncident.ResolvedAt = DateTime.UtcNow;
                    await _incidentRepository.UpdateAsync(activeIncident);

                    await SendAlertToResponsibleAdminsAsync(app, $"✅ DÜZELDİ: {componentName}", 
                        $"{app.Name} uygulamasında {componentName} bileşeni tekrar sağlıklı.");
                }
            }
        }

        private Dictionary<string, HealthStatus> ParseComponentStatuses(string dependencyDetails)
        {
            var results = new Dictionary<string, HealthStatus>();
            if (string.IsNullOrEmpty(dependencyDetails)) return results;

            try
            {
                using var doc = JsonDocument.Parse(dependencyDetails);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        string statusStr = "";
                        if (prop.Value.ValueKind == JsonValueKind.Object && prop.Value.TryGetProperty("status", out var statusProp))
                        {
                            statusStr = statusProp.ToString();
                        }
                        else
                        {
                            statusStr = prop.Value.ToString();
                        }

                        if (statusStr.Contains("Unhealthy", StringComparison.OrdinalIgnoreCase) || statusStr == "3")
                            results[prop.Name] = HealthStatus.Unhealthy;
                        else if (statusStr.Contains("Degraded", StringComparison.OrdinalIgnoreCase) || statusStr == "2")
                            results[prop.Name] = HealthStatus.Degraded;
                        else
                            results[prop.Name] = HealthStatus.Healthy;
                    }
                }
            }
            catch { /* Not a JSON or invalid format */ }

            return results;
        }

        private bool ShouldTriggerIncidentForComponent(List<HealthSnapshot> snapshots, string componentName)
        {
            if (snapshots == null || snapshots.Count < 3) return false;

            // Son 3 kaydın tamamında bu bileşen Unhealthy mi?
            foreach (var snapshot in snapshots.Take(3))
            {
                var statusMap = ParseComponentStatuses(snapshot.DependencyDetails);
                
                // Eğer bileşen JSON'da yoksa ve biz "System" arıyorsak, snapshot statüsüne bak
                if (!statusMap.ContainsKey(componentName))
                {
                    if (componentName == "System" && snapshot.Status == HealthStatus.Unhealthy) continue;
                    return false;
                }

                if (statusMap[componentName] != HealthStatus.Unhealthy) return false;
            }

            return true;
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
                var aiClient = await aiClientFactory.CreateClientAsync();
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
                await statusBroadcaster.BroadcastNewInsightAsync(newInsight);

                Console.WriteLine($">>>> [RCA-SUCCESS] Analiz tamamlandı ve veritabanına kaydedildi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>>> [RCA-FATAL-ERROR] Analiz tamamen patladı: {ex.ToString()}");
            }
        }
    }
}