using System;
using System.Text.Json;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    // Sözleşmemizi imzaladık. Dışarıdan AppId alır, geriye HealthSnapshot döner.
    public class PollSingleAppUseCase : IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?>
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly IHealthProbeClient _probeClient;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IUseCaseAsync<HealthSnapshot> _analyzeUseCase;

        public PollSingleAppUseCase(
            IMonitoredAppRepository appRepository,
            IHealthProbeClient probeClient,
            ISnapshotRepository snapshotRepository,
            IUseCaseAsync<HealthSnapshot> analyzeUseCase) // UC-5 Kural Motoru
        {
            _appRepository = appRepository;
            _probeClient = probeClient;
            _snapshotRepository = snapshotRepository;
            _analyzeUseCase = analyzeUseCase;
        }

        public async Task<HealthSnapshot?> ExecuteAsync(PollSingleAppRequest request)
        {
            // 1. Uygulamayı veritabanından bul
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            HealthStatus finalStatus = HealthStatus.Unhealthy;
            long finalDuration = 0;
            string errorOrJson = "";
            double appCpu = 0, sysCpu = 0, appRam = 0, sysRam = 0, realDisk = 0;

            try
            {
                // 2. Altyapı Elçimize (Infrastructure) ping attır
                var probeResult = await _probeClient.CheckHealthAsync(app.HealthUrl, request.CancellationToken);

                finalStatus = probeResult.Status;
                finalDuration = probeResult.DurationMilliseconds;
                errorOrJson = probeResult.JsonContent ?? "";

                // 3. Başarılıysa veya JSON verisi geldiyse ayrıştır (Parse)
                if (!string.IsNullOrEmpty(errorOrJson) && errorOrJson.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using var jsonDoc = JsonDocument.Parse(errorOrJson);
                        var root = jsonDoc.RootElement;

                        // HTTP 200 gelse bile JSON içindeki gerçek statüyü okuyarak False-Positive engelliyoruz
                        if (root.TryGetProperty("status", out var statusProp))
                        {
                            string statusStr = statusProp.GetString() ?? "";
                            if (statusStr.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase))
                                finalStatus = HealthStatus.Unhealthy;
                            else if (statusStr.Equals("Degraded", StringComparison.OrdinalIgnoreCase))
                                // Kullanıcı isteği: Degraded durumlarını da kritik (Unhealthy) kabul et ki AI tetiklensin.
                                finalStatus = HealthStatus.Unhealthy;
                            else if (statusStr.Equals("Healthy", StringComparison.OrdinalIgnoreCase))
                                finalStatus = HealthStatus.Healthy;
                        }

                        // Metrikleri JSON root dizininden doğru isimlerle çekiyoruz
                        if (root.TryGetProperty("metrics", out var metricsProp))
                        {
                            if (metricsProp.TryGetProperty("process_cpu_percent", out var aC)) appCpu = aC.GetDouble();
                            if (metricsProp.TryGetProperty("system_cpu_percent", out var sC)) sysCpu = sC.GetDouble();
                            if (metricsProp.TryGetProperty("process_ram_mb", out var aR)) appRam = aR.GetDouble();
                            if (metricsProp.TryGetProperty("system_ram_percent", out var sR)) sysRam = sR.GetDouble();
                            if (metricsProp.TryGetProperty("free_disk_gb", out var d)) realDisk = d.GetDouble();
                        }

                        // Sadece alt detayları (SQL vb.) AI analizi için sakla
                        if (root.TryGetProperty("checks", out var checksProp))
                        {
                            errorOrJson = checksProp.ToString();

                            // BAĞIMLILIK KONTROLÜ (OVERRIDE): Eğer herhangi bir dependency "Unhealthy" ise genel durumu EZ.
                            foreach (var prop in checksProp.EnumerateObject())
                            {
                                // Değer "Unhealthy" veya obje içinde { status: "Unhealthy" } olabilir.
                                string propString = prop.Value.ToString();
                                if (propString.Contains("Unhealthy", StringComparison.OrdinalIgnoreCase) || 
                                    propString.Contains("\"status\":3", StringComparison.OrdinalIgnoreCase) ||
                                    propString.Contains("\"status\":\"Unhealthy\"", StringComparison.OrdinalIgnoreCase))
                                {
                                    finalStatus = HealthStatus.Unhealthy;
                                    break;
                                }
                            }
                        }
                        else if (root.TryGetProperty("dependencyDetails", out var depProp))
                        {
                            errorOrJson = depProp.GetString() ?? errorOrJson;
                        }
                    }
                    catch { /* JSON okunamasa bile uygulamanın ayakta olduğunu biliyoruz. */ }
                }
            }
            catch (Exception ex)
            {
                // İŞTE KRİTİK NOKTA! Ağ hatası alırsak program çökmeyecek. 
                // Durumu Unhealthy yapıp, hatayı AI'a göndermek üzere kaydedeceğiz.
                finalStatus = HealthStatus.Unhealthy;
                errorOrJson = $"Kritik Ağ Hatası (DNS/Bağlantı): {ex.Message}";
                Console.WriteLine($">>>> [NETWORK FAIL] {app.Name} pinglenemedi! Hata yutuldu ve AI'a paslanacak.");
            }

            // 4. Veritabanına kaydedilecek Snapshot nesnesini hazırla
            var snapshot = new HealthSnapshot
            {
                AppId = app.Id,
                Timestamp = DateTime.UtcNow,
                Status = finalStatus,
                TotalDuration = finalDuration,
                AppCpuUsage = appCpu,           // Ayrıştırıldı!
                SystemCpuUsage = sysCpu,        // Ayrıştırıldı!
                AppRamUsage = appRam,           // Ayrıştırıldı!
                SystemRamUsage = sysRam,        // Ayrıştırıldı!
                FreeDiskGb = realDisk,
                DependencyDetails = errorOrJson // Yapay Zeka buradaki hatayı okuyup yorumlayacak!
            };

            // 5. Veritabanına kaydet 
            await _snapshotRepository.AddAsync(snapshot);

            // 6. Kesinti var mı diye AI / Kural Motorunu tetikle
            await _analyzeUseCase.ExecuteAsync(snapshot);

            // 7. React'a yayınlaması için sonucu Worker'a geri dön
            return snapshot;
        }
    }
}