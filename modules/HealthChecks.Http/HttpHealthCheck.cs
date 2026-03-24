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
        private readonly string _url;

        public string Name => $"HTTP_Check_{_url}";

        public HttpHealthCheck(HttpClient httpClient, string url)
        {
            _httpClient = httpClient;
            _url = url;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Sadece Header'ları okuyarak gövdeyi (Body) indirmekten kurtuluyoruz (Performans Artışı)
                using var request = new HttpRequestMessage(HttpMethod.Get, _url);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                stopwatch.Stop();

                // Telemetri verilerini hazırlıyoruz
                var telemetryData = new Dictionary<string, object>
                {
                    { "StatusCode", (int)response.StatusCode },
                    { "Url", _url }
                };

                // Mentör Özel Revizyonu: SADECE 404 ise Unhealthy say!
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var unhealthyResult = HealthCheckResult.Unhealthy($"HTTP isteği başarısız oldu. Sadece 404 Not Found alındı.", data: telemetryData);
                    unhealthyResult.Duration = stopwatch.Elapsed;
                    return unhealthyResult;
                }

                // 404 harici tüm yanıtlarda (500 dahil) uç noktanın var olduğu kabul edilir
                var result = HealthCheckResult.Healthy($"Uç nokta yanıt verdi. Status Code: {(int)response.StatusCode}", data: telemetryData);
                result.Duration = stopwatch.Elapsed;
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var exceptionResult = HealthCheckResult.Unhealthy($"HTTP isteği sırasında bağlantı hatası: {ex.Message}", ex);
                exceptionResult.Duration = stopwatch.Elapsed;
                return exceptionResult;
            }
        }
    }
}