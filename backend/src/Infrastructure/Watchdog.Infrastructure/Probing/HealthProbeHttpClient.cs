using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;
using Watchdog.Domain.Enums;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Infrastructure.Probing
{
    public class HealthProbeHttpClient : IHealthProbeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ISystemConfigurationRepository _sysConfigRepository;

        // HttpClient DI (Dependency Injection) üzerinden gelecek
        public HealthProbeHttpClient(HttpClient httpClient, ISystemConfigurationRepository sysConfigRepository)
        {
            _httpClient = httpClient;
            _sysConfigRepository = sysConfigRepository;
        }

        public async Task<ProbeResult> CheckHealthAsync(string healthUrl, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ProbeResult();

            try
            {
                // Dinamik Sistem Ayarlarını Çek (TimeoutSeconds için)
                var sysConfig = await _sysConfigRepository.GetAsync();
                int timeoutSecs = sysConfig?.TimeoutSeconds ?? 20;

                // Dinamik timeout policy oluştur (Pessimistic: task'ı cidden iptal etmeye çalışır)
                var timeoutPolicy = Policy.TimeoutAsync(timeoutSecs, TimeoutStrategy.Pessimistic);

                // Polly kalkanı altında HTTP isteğini fırlatıyoruz.
                var response = await timeoutPolicy.ExecuteAsync(async () =>
                {
                    return await _httpClient.GetAsync(healthUrl, cancellationToken);
                });

                stopwatch.Stop();
                result.DurationMilliseconds = stopwatch.ElapsedMilliseconds;

                // JSON verisini HTTP durum kodundan bağımsız olarak KESİNLİKLE okuyoruz.
                result.JsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    result.Status = HealthStatus.Healthy;
                }
                else
                {
                    // Site ayakta ama 404 veya 500/503 gibi bir hata kodu dönüyor.
                    // 503 Service Unavailable dönerse direkt Unhealthy kabul ediyoruz.
                    result.Status = response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                        ? HealthStatus.Unhealthy
                        : HealthStatus.Degraded;
                }
            }
            catch (TimeoutRejectedException)
            {
                // Timeout (Zaman aşımı) durumunda sistem 'Unhealthy' (Kırmızı) olur.
                stopwatch.Stop();
                result.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
                result.JsonContent = "Timeout: Hedef uygulama belirlenen süre içinde yanıt vermedi.";
            }
            catch (Exception ex)
            {
                // DNS hatası, sunucu kapalı vb. durumlar
                stopwatch.Stop();
                result.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
                result.JsonContent = $"Connection Error: {ex.Message}";
            }

            return result;
        }
    }
}