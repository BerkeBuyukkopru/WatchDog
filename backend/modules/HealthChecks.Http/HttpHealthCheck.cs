using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Http
{
    public class HttpHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly Func<string> _urlProvider;

        // İsim sabit kalmalı ki WatchDog ararken kaybetmesin
        public string Name => "HTTP_Dynamic_Check";

        public HttpHealthCheck(HttpClient httpClient, Func<string> urlProvider)
        {
            _httpClient = httpClient;
            _urlProvider = urlProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            // KRİTİK: Her ping atıldığında güncel URL'i okuyoruz!
            string currentUrl = _urlProvider();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, currentUrl);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                stopwatch.Stop();

                var telemetryData = new Dictionary<string, object>
                {
                    { "StatusCode", (int)response.StatusCode },
                    { "Url", currentUrl }
                };

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var unhealthyResult = HealthCheckResult.Unhealthy($"HTTP isteği başarısız oldu. Sadece 404 Not Found alındı.", data: telemetryData);
                    unhealthyResult.Duration = stopwatch.Elapsed;
                    return unhealthyResult;
                }

                var result = HealthCheckResult.Healthy($"Uç nokta yanıt verdi. Status Code: {(int)response.StatusCode}", data: telemetryData);
                result.Duration = stopwatch.Elapsed;
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var exceptionResult = HealthCheckResult.Unhealthy($"HTTP isteği sırasında bağlantı hatası ({currentUrl}): {ex.Message}", ex);
                exceptionResult.Duration = stopwatch.Elapsed;
                return exceptionResult;
            }
        }
    }
}