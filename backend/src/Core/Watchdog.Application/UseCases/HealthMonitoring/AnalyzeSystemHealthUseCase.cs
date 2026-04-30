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
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;
using Watchdog.Domain.Rules;
using Microsoft.Extensions.Configuration;

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
        private readonly IConfiguration _configuration;

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository,
            IServiceScopeFactory scopeFactory,
            IStatusBroadcaster statusBroadcaster,
            IAuthRepository authRepository,
            IConfiguration configuration) // Constructora Eklendi
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
            _scopeFactory = scopeFactory;
            _statusBroadcaster = statusBroadcaster;
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task ExecuteAsync(HealthSnapshot latestSnapshot)
        {
            var app = await _appRepository.GetByIdAsync(latestSnapshot.AppId);
            if (app == null) return;

            // 🚨 CANLI YAYIN: Veriyi DTO'ya çevirip fırlat (React'in beklediği format)
            var dto = new LatestStatusDto
            {
                Id = latestSnapshot.Id,
                AppId = latestSnapshot.AppId,
                AppName = app.Name, // Uygulama ismini ekledik
                Status = latestSnapshot.Status.ToString(),
                TotalDuration = latestSnapshot.TotalDuration,
                Timestamp = latestSnapshot.Timestamp,
                AppCpuUsage = latestSnapshot.AppCpuUsage,
                SystemCpuUsage = latestSnapshot.SystemCpuUsage,
                AppRamUsage = latestSnapshot.AppRamUsage,
                SystemRamUsage = latestSnapshot.SystemRamUsage,
                FreeDiskGb = latestSnapshot.FreeDiskGb,
                DependencyDetails = latestSnapshot.DependencyDetails,
                TotalRamMb = Convert.ToDouble(_configuration["SystemMetrics:TotalRamMb"] ?? "16384"),
                TotalCpuPercentage = Convert.ToDouble(_configuration["SystemMetrics:TotalCpuPercentage"] ?? "100"),
                TotalDiskGb = Convert.ToDouble(_configuration["SystemMetrics:TotalDiskGb"] ?? "500"),
                TotalCpuCores = Convert.ToInt32(_configuration["SystemMetrics:TotalCpuCores"] ?? "16")
            };

            await _statusBroadcaster.BroadcastNewStatusAsync(dto);

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
                        // EĞER AĞ HATASIYSA (Uygulama çalışmıyorsa), İNSİDENT OLUŞTURMA!
                        // Sadece bileşen bazlı (DB, Redis vb.) gerçek hataları insident olarak kaydet.
                        if (latestSnapshot.DependencyDetails != null && 
                            latestSnapshot.DependencyDetails.StartsWith("Kritik Ağ Hatası"))
                        {
                            Console.WriteLine($">>>> [INFO] {app.Name} - Ağ Hatası tespit edildi. İnsident oluşturulmuyor (UI uyarısı verilecek).");
                            continue;
                        }

                        Console.WriteLine($">>>> [INCIDENT-TRIGGER] {app.Name} - {componentName} ÜST ÜSTE 3 KEZ HATA VERDİ! Analiz başlıyor...");

                        var newIncident = new Incident
                        {
                            AppId = app.Id,
                            FailedComponent = componentName,
                            ErrorMessage = latestSnapshot.DependencyDetails ?? "Bilinmeyen bileşen hatası.",
                            StartedAt = DateTime.UtcNow
                        };

                        await _incidentRepository.AddAsync(newIncident);

                        // 🚨 CANLI BİLDİRİM: Yeni olayı tüm adminlere fırlat
                        await _statusBroadcaster.BroadcastNewIncidentAsync(new IncidentDto
                        {
                            Id = newIncident.Id,
                            AppId = newIncident.AppId,
                            AppName = app.Name,
                            FailedComponent = newIncident.FailedComponent,
                            ErrorMessage = newIncident.ErrorMessage,
                            StartedAt = newIncident.StartedAt
                        });

                        await SendAlertToResponsibleAdminsAsync(app, $"🚨 KESİNTİ: {componentName}", 
                            $"{app.Name} uygulamasında {componentName} bileşeni çöktü.\nDetay: {latestSnapshot.DependencyDetails}");

                        // Yapay Zeka Analizini (RCA) tetikle
                        _ = Task.Run(async () => await TriggerRootCauseAnalysisAsync(app, recentSnapshots));
                    }
                    else
                    {
                        Console.WriteLine($">>>> [INFO] {app.Name} - {componentName} Unhealthy ama henüz 3 hata birikmedi ({recentSnapshots.Count}/3).");
                    }
                }
                else if (currentStatus == HealthStatus.Healthy)
                {
                    // 🟢 OTOMATİK İYİLEŞME KONTROLÜ: 
                    // İsim uyuşmazlıklarını önlemek için (Örn: MongoDb vs MongoDB_Check) daha esnek arama yapıyoruz
                    var activeIncidents = await _incidentRepository.GetActiveIncidentsAsync(app.Id);
                    var incidentToResolve = activeIncidents.FirstOrDefault(i => 
                        i.FailedComponent.Contains(componentName, StringComparison.OrdinalIgnoreCase) || 
                        componentName.Contains(i.FailedComponent, StringComparison.OrdinalIgnoreCase));

                    if (incidentToResolve != null)
                    {
                        Console.WriteLine($">>>> [RECOVERY] {app.Name} - {componentName} düzeldi. Olay kapatılıyor.");
                        incidentToResolve.ResolvedAt = DateTime.UtcNow;
                        await _incidentRepository.UpdateAsync(incidentToResolve);

                        // 🚨 CANLI BİLDİRİM: Olayın çözüldüğünü tüm adminlere fırlat
                        await _statusBroadcaster.BroadcastResolvedIncidentAsync(new IncidentDto
                        {
                            Id = incidentToResolve.Id,
                            AppId = incidentToResolve.AppId,
                            AppName = app.Name,
                            FailedComponent = incidentToResolve.FailedComponent,
                            ErrorMessage = incidentToResolve.ErrorMessage,
                            StartedAt = incidentToResolve.StartedAt,
                            ResolvedAt = incidentToResolve.ResolvedAt
                        });

                        // 🧠 AI AUTO-RESOLVE: Hata düzeldiğine göre bu hataya dair AI önerilerini de kapat
                        using var scope = _scopeFactory.CreateScope();
                        var insightRepository = scope.ServiceProvider.GetRequiredService<IAiInsightRepository>();
                        await insightRepository.ResolveAllActiveInsightsForAppAsync(app.Id);
                        
                        // 🚨 CANLI BİLDİRİM: AI önerilerinin kapandığını frontend'e bildir
                        await _statusBroadcaster.BroadcastAllInsightsResolvedAsync(app.Id);

                        Console.WriteLine($">>>> [AI-AUTO-RESOLVE] {app.Name} için tüm aktif yapay zeka önerileri kapatıldı ve bildirildi.");

                        await SendAlertToResponsibleAdminsAsync(app, $"✅ DÜZELDİ: {componentName}", 
                            $"{app.Name} uygulamasında {componentName} bileşeni tekrar sağlıklı.");
                    }
                }
            }
        }

        private Dictionary<string, HealthStatus> ParseComponentStatuses(string? dependencyDetails)
        {
            if (string.IsNullOrEmpty(dependencyDetails)) return new Dictionary<string, HealthStatus>();
            
            try 
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Dictionary<string, HealthStatus>>(dependencyDetails, options) 
                       ?? new Dictionary<string, HealthStatus>();
            }
            catch 
            {
                return new Dictionary<string, HealthStatus>();
            }
        }

        private bool ShouldTriggerIncidentForComponent(List<HealthSnapshot> snapshots, string componentName)
        {
            // Eğer 3 snapshot henüz birikmemişse tetikleme yapma
            if (snapshots == null || snapshots.Count < 3) return false;

            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

            // Son 3 kaydın tamamında bu bileşen Unhealthy mi? 
            // VE bu kayıtlar taze mi? (Son 5 dakika içinde mi?)
            foreach (var snapshot in snapshots)
            {
                if (snapshot.Timestamp < fiveMinutesAgo) 
                {
                    Console.WriteLine($">>>> [INFO] {componentName} için bulunan kayıt çok eski ({snapshot.Timestamp}). Analiz tetiklenmiyor.");
                    return false;
                }

                var statusMap = ParseComponentStatuses(snapshot.DependencyDetails);
                
                if (!statusMap.ContainsKey(componentName))
                {
                    // Eğer bileşen listede yoksa ama snapshot genel olarak Unhealthy ise ve "System" bakıyorsak
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
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var insightRepository = scope.ServiceProvider.GetRequiredService<IAiInsightRepository>();
                var aiProviderRepository = scope.ServiceProvider.GetRequiredService<IAiProviderRepository>();
                var promptBuilder = scope.ServiceProvider.GetRequiredService<IPromptBuilder>();
                var aiClientFactory = scope.ServiceProvider.GetRequiredService<IAiClientFactory>();
                var statusBroadcaster = scope.ServiceProvider.GetRequiredService<IStatusBroadcaster>();

                Console.WriteLine($">>>> [RCA-START] {app.Name} için Kök Neden Analizi süreci başladı...");

                var lastInsight = await insightRepository.GetLatestInsightAsync(app.Id);
                // ⏱ COOLDOWN: 10 dakika (Aynı hata için sürekli analiz yapıp motoru yormayalım)
                if (lastInsight != null && lastInsight.CreatedAt > DateTime.UtcNow.AddMinutes(-10))
                {
                    Console.WriteLine($">>>> [RCA-SKIP] Cooldown aktif. Son 10 dakika içinde analiz yapılmış.");
                    return;
                }

                var activeProviderEntity = await aiProviderRepository.GetActiveProviderAsync();
                
                // 🔍 KRİTİK DÜZELTME: Eğer uygulamaya özel bir motor seçilmediyse, sistemin aktif motorunu kullan
                var providerIdToUse = app.ActiveAiProviderId ?? activeProviderEntity?.Id;

                if (providerIdToUse == null)
                {
                    Console.WriteLine($">>>> [RCA-ERROR] Hata: Hiçbir AI sağlayıcısı aktif değil!");
                    return;
                }

                var prompt = promptBuilder.BuildRootCausePrompt(recentSnapshots, app.Name);

                Console.WriteLine($">>>> [RCA-REQUEST] {activeProviderEntity?.Name ?? "Yapay Zeka"} motoruna istek atılıyor...");
                var aiClient = await aiClientFactory.CreateClientAsync(providerIdToUse.Value);
                var aiResponse = await aiClient.AnalyzeAsync(prompt); 

                if (string.IsNullOrEmpty(aiResponse))
                {
                    Console.WriteLine($">>>> [RCA-ERROR] Yapay zeka boş cevap döndü!");
                    return;
                }

                Console.WriteLine($">>>> [RCA-REPORT] {app.Name} Kriz Analiz Raporu Tamamlandı.");

                var newInsight = new AiInsight
                {
                    AppId = app.Id,
                    AiProviderId = providerIdToUse,
                    InsightType = InsightType.CrashWarning,
                    Message = aiResponse,
                    IsResolved = false
                };

                await insightRepository.AddAsync(newInsight);

                var newInsightDto = new Watchdog.Application.DTOs.AI.AiInsightDto
                {
                    Id = newInsight.Id,
                    AppId = newInsight.AppId,
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