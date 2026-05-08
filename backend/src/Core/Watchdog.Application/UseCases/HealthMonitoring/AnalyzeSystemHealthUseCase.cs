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
        private readonly ISystemConfigurationRepository _sysConfigRepository;

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository,
            IServiceScopeFactory scopeFactory,
            IStatusBroadcaster statusBroadcaster,
            IAuthRepository authRepository,
            IConfiguration configuration,
            ISystemConfigurationRepository sysConfigRepository) // Constructora Eklendi
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
            _sysConfigRepository = sysConfigRepository;
            _scopeFactory = scopeFactory;
            _statusBroadcaster = statusBroadcaster;
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task ExecuteAsync(HealthSnapshot latestSnapshot)
        {
            var app = await _appRepository.GetByIdAsync(latestSnapshot.AppId);
            if (app == null) return;
            
            // Dinamik Sistem Ayarlarını Çek (RetryCount için)
            var sysConfig = await _sysConfigRepository.GetAsync();
            int retryCount = sysConfig?.RetryCount ?? 3; // Fallback to 3 if null

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

            // --- YENİ EKLENEN: GLOBAL EŞİK (THRESHOLD) KONTROLLERİ ---
            if (sysConfig != null)
            {
                // CPU Kontrolü (Sadece Sistem Geneli)
                if (latestSnapshot.SystemCpuUsage >= sysConfig.CriticalCpuThreshold)
                {
                    componentsStatus["System.CPU"] = HealthStatus.Unhealthy;
                }

                // RAM Kontrolü
                if (latestSnapshot.SystemRamUsage >= sysConfig.CriticalRamThreshold)
                {
                    componentsStatus["System.RAM"] = HealthStatus.Unhealthy;
                }

                // Latency (Gecikme) Kontrolü
                if (latestSnapshot.TotalDuration >= sysConfig.CriticalLatencyThreshold)
                {
                    componentsStatus["System.Latency"] = HealthStatus.Degraded;
                }
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
                    // Dinamik Retry Kontrolü: Bu bileşen son 'retryCount' snapshot'ta da mı Unhealthy?
                    var recentSnapshots = await _snapshotRepository.GetLatestSnapshotsAsync(app.Id, retryCount);
                    
                    if (ShouldTriggerIncidentForComponent(recentSnapshots, componentName, retryCount))
                    {
                        // EĞER AĞ HATASIYSA (Uygulama çalışmıyorsa), İNSİDENT OLUŞTURMA!
                        // Sadece bileşen bazlı (DB, Redis vb.) gerçek hataları insident olarak kaydet.
                        if (latestSnapshot.DependencyDetails != null && 
                            latestSnapshot.DependencyDetails.StartsWith("Kritik Ağ Hatası"))
                        {
                            Console.WriteLine($">>>> [INFO] {app.Name} - Ağ Hatası tespit edildi. İnsident oluşturulmuyor (UI uyarısı verilecek).");
                            continue;
                        }

                        Console.WriteLine($">>>> [INCIDENT-TRIGGER] {app.Name} - {componentName} ÜST ÜSTE {retryCount} KEZ HATA VERDİ! Analiz başlıyor...");

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

                        await SendAlertToResponsibleAdminsAsync(app, newIncident);

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

                        Console.WriteLine($">>>> [RECOVERY] {app.Name} için AI önerileri artık manuel kapatılmak üzere korundu.");

                        await SendAlertToResponsibleAdminsAsync(app, incidentToResolve, isRecovery: true);
                    }
                }
            }
        }

        private Dictionary<string, HealthStatus> ParseComponentStatuses(string? dependencyDetails)
        {
            var result = new Dictionary<string, HealthStatus>();
            if (string.IsNullOrWhiteSpace(dependencyDetails)) return result;
            
            dependencyDetails = dependencyDetails.Trim();
            
            // Eğer JSON formatında değilse (Örn: "Connection Error: ...") boş dön. 
            // Yukarıdaki kod parçası zaten bunu "System" olarak ele alacaktır.
            if (!dependencyDetails.StartsWith("{") && !dependencyDetails.StartsWith("["))
            {
                return result;
            }
            
            try 
            {
                using var doc = JsonDocument.Parse(dependencyDetails);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in root.EnumerateObject())
                    {
                        var value = property.Value;
                        HealthStatus status = HealthStatus.Healthy;

                        if (value.ValueKind == JsonValueKind.Number)
                        {
                            // Basit format: {"Redis": 3}
                            status = (HealthStatus)value.GetInt32();
                        }
                        else if (value.ValueKind == JsonValueKind.String)
                        {
                            // Metin formatı: {"Redis": "Unhealthy"}
                            Enum.TryParse<HealthStatus>(value.GetString(), true, out status);
                        }
                        else if (value.ValueKind == JsonValueKind.Object)
                        {
                            // Karmaşık format: {"Redis": {"status": "Unhealthy", ...}}
                            if (value.TryGetProperty("status", out var statusProp))
                            {
                                if (statusProp.ValueKind == JsonValueKind.Number)
                                    status = (HealthStatus)statusProp.GetInt32();
                                else
                                    Enum.TryParse<HealthStatus>(statusProp.GetString(), true, out status);
                            }
                        }
                        
                        result[property.Name] = status;
                    }
                }
            }
            catch (Exception)
            {
                // Parse hatası durumunda sessizce boş dön.
                // Log kalabalığını önlemek için hata mesajını siliyoruz.
            }

            return result;
        }

        private bool ShouldTriggerIncidentForComponent(List<HealthSnapshot> snapshots, string componentName, int retryCount)
        {
            // Eğer yeterli snapshot henüz birikmemişse tetikleme yapma
            if (snapshots == null || snapshots.Count < retryCount) return false;

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
        private async Task SendAlertToResponsibleAdminsAsync(MonitoredApp app, Incident incident, bool isRecovery = false)
        {
            var responsibleAdmins = await _authRepository.GetAdminsByAppIdAsync(app.Id);

            var adminEmails = responsibleAdmins
                .Where(a => !string.IsNullOrWhiteSpace(a.Email) && a.Email.Contains("@"))
                .Select(a => a.Email.Trim())
                .ToList();

            if (adminEmails.Any())
            {
                foreach (var email in adminEmails)
                {
                    try {
                        if (isRecovery)
                            await _notificationSender.SendRecoveryAlertAsync(email, incident, app);
                        else
                            await _notificationSender.SendDowntimeAlertAsync(email, incident, app);
                    } catch (Exception ex) {
                        Console.WriteLine($">>>> [MAIL-ERROR] {email} adresine mail gönderilemedi: {ex.Message}");
                    }
                }
            }
        }

        private async Task TriggerRootCauseAnalysisAsync(MonitoredApp app, List<HealthSnapshot> recentSnapshots)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var insightRepository = scope.ServiceProvider.GetRequiredService<IAiInsightRepository>();
                var incidentRepository = scope.ServiceProvider.GetRequiredService<IIncidentRepository>(); // Local repo
                var aiProviderRepository = scope.ServiceProvider.GetRequiredService<IAiProviderRepository>();
                var promptBuilder = scope.ServiceProvider.GetRequiredService<IPromptBuilder>();
                var aiClientFactory = scope.ServiceProvider.GetRequiredService<IAiClientFactory>();
                var statusBroadcaster = scope.ServiceProvider.GetRequiredService<IStatusBroadcaster>();
                var appRepository = scope.ServiceProvider.GetRequiredService<IMonitoredAppRepository>();

                Console.WriteLine($">>>> [RCA-START] {app.Name} için Kök Neden Analizi süreci başladı...");

                // ⏱ AKILLI COOLDOWN: 
                // Eğer son 10 dakika içinde bir analiz yapılmışsa VE yeni bir bileşen hatası eklenmemişse bekle.
                var lastRcaInsight = await insightRepository.GetLatestInsightByTypeAsync(app.Id, InsightType.CrashWarning);
                var activeIncidents = await incidentRepository.GetActiveIncidentsAsync(app.Id); // Local repo kullanımı

                if (lastRcaInsight != null && (DateTime.UtcNow - lastRcaInsight.CreatedAt).TotalMinutes < 10)
                {
                    // Son analizden sonra yeni bir bileşen eklenmiş mi kontrol et
                    bool hasNewFailure = activeIncidents.Any(i => i.StartedAt > lastRcaInsight.CreatedAt);
                    
                    if (!hasNewFailure)
                    {
                        Console.WriteLine($">>>> [RCA-SKIP] Cooldown aktif ve yeni bir hata bileşeni tespit edilmedi.");
                        return;
                    }
                    Console.WriteLine($">>>> [RCA-BYPASS] Yeni bir hata bileşeni eklendiği için cooldown deliniyor!");
                }

                // 🧠 AKILLI MOTOR SEÇİMİ VE FALLBACK MEKANİZMASI
                AiProvider? providerToUse = null;
                
                // Arka plan görevinde güvenli çalışmak için uygulamayı bu scope içinde tekrar çekiyoruz
                var localApp = await appRepository.GetByIdAsync(app.Id);
                if (localApp == null) return;

                // 1. Uygulama için özel bir motor seçilmiş mi?
                if (localApp.ActiveAiProviderId.HasValue)
                {
                    providerToUse = await aiProviderRepository.GetByIdAsync(localApp.ActiveAiProviderId.Value);

                    // Seçili motor silinmiş veya pasifse
                    if (providerToUse == null || !providerToUse.IsActive)
                    {
                        Console.WriteLine($">>>> [RCA-FALLBACK] {localApp.Name} için seçili motor ({localApp.ActiveAiProviderId}) pasif veya silinmiş! Kurtarma moduna geçiliyor...");
                        providerToUse = null; // Aşağıdaki fallback mantığını tetikle
                    }
                }

                // 2. Eğer özel seçim yoksa veya geçersizse Fallback motoru bul
                if (providerToUse == null)
                {
                    providerToUse = await aiProviderRepository.GetBestFallbackProviderAsync();

                    if (providerToUse != null)
                    {
                        // 🛡️ RACE-CONDITION KONTROLÜ: Güncellemeden önce son bir kez daha DB'ye bakıyoruz.
                        // Eğer kullanıcı biz analiz yaparken Dashboard'dan manuel bir seçim yaptıysa, onun seçimini EZMİYORUZ.
                        var latestAppState = await appRepository.GetByIdAsync(localApp.Id);
                        if (latestAppState != null && latestAppState.ActiveAiProviderId == localApp.ActiveAiProviderId)
                        {
                            latestAppState.ActiveAiProviderId = providerToUse.Id;
                            await appRepository.UpdateAsync(latestAppState);
                            Console.WriteLine($">>>> [RCA-AUTO-UPDATE] {localApp.Name} motoru otomatik olarak {providerToUse.Name} ile güncellendi.");
                            
                            // Analizin geri kalanında güncel nesneyi kullanalım
                            localApp = latestAppState;
                        }
                    }
                }

                if (providerToUse == null)
                {
                    Console.WriteLine($">>>> [RCA-ERROR] Hata: Sistemde hiçbir aktif AI sağlayıcısı bulunamadı!");
                    return;
                }

                Console.WriteLine($">>>> [RCA-REQUEST] {providerToUse.Name} motoruna istek atılıyor...");
                var aiClient = await aiClientFactory.CreateClientAsync(providerToUse.Id);

                // Yerel model kontrolü ve dil optimizasyonu
                bool isLocal = aiClient.GetType().Name.Contains("LocalOllamaClient");
                var prompt = promptBuilder.BuildRootCausePrompt(recentSnapshots, localApp.Name, isLocal);

                var aiResponse = await aiClient.AnalyzeAsync(prompt);

                if (string.IsNullOrEmpty(aiResponse))
                {
                    Console.WriteLine($">>>> [RCA-ERROR] Yapay zeka boş cevap döndü!");
                    return;
                }

                Console.WriteLine($">>>> [RCA-REPORT] {localApp.Name} Kriz Analiz Raporu Tamamlandı.");

                var newInsight = new AiInsight
                {
                    AppId = localApp.Id,
                    AiProviderId = providerToUse.Id,
                    InsightType = InsightType.CrashWarning,
                    Message = aiResponse,
                    ProviderName = providerToUse.Name,
                    ModelName = providerToUse.ModelName,
                    IsResolved = false
                };

                await insightRepository.AddAsync(newInsight);

                var newInsightDto = new Watchdog.Application.DTOs.AI.AiInsightDto
                {
                    Id = newInsight.Id,
                    AppId = newInsight.AppId,
                    AppName = localApp.Name,
                    Message = newInsight.Message,
                    InsightType = newInsight.InsightType.ToString(),
                    IsResolved = newInsight.IsResolved,
                    ProviderName = newInsight.ProviderName,
                    ModelName = newInsight.ModelName,
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