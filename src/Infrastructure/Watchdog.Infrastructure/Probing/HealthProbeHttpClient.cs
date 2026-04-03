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

namespace Watchdog.Infrastructure.Probing
{
    public class HealthProbeHttpClient : IHealthProbeClient
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncTimeoutPolicy _timeoutPolicy;

        // HttpClient DI (Dependency Injection) üzerinden gelecek
        public HealthProbeHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // POLLY KURALI: Bir siteye ping attığımızda 5 saniye içinde cevap gelmezse bekleme, fişini çek!
            _timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Pessimistic);
        }

        public async Task<ProbeResult> CheckHealthAsync(string healthUrl, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ProbeResult();

            try
            {
                // Polly kalkanı altında HTTP isteğini fırlatıyoruz.
                var response = await _timeoutPolicy.ExecuteAsync(async () =>
                {
                    return await _httpClient.GetAsync(healthUrl, cancellationToken);
                });

                stopwatch.Stop();
                result.DurationMilliseconds = stopwatch.ElapsedMilliseconds;

                if (response.IsSuccessStatusCode)
                {
                    result.Status = HealthStatus.Healthy;
                    result.JsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                else
                {
                    // Site ayakta ama 404 veya 500 gibi bir hata kodu dönüyor.
                    result.Status = HealthStatus.Degraded;
                }
            }
            catch (TimeoutRejectedException)
            {
                // Timeout (Zaman aşımı) durumunda sistem 'Unhealthy' (Kırmızı) olur.
                stopwatch.Stop();
                result.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
            }
            catch (Exception)
            {
                // DNS hatası, sunucu kapalı vb. durumlar
                stopwatch.Stop();
                result.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
                result.Status = HealthStatus.Unhealthy;
            }

            return result;
        }
    }
}