using HealthChecks.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HealthChecks.Http
{
    public class HttpHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;

        // Modülün adı, dışarıdan verilen URL'e göre dinamik olacak
        public string Name => $"HTTP_Check_{_url}";

        // Constructor: İşi yapacak olan HttpClient ve gidilecek URL'i dışarıdan alıyoruz
        public HttpHealthCheck(HttpClient httpClient, string url)
        {
            _httpClient = httpClient;
            _url = url;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            // Kronometreyi başlatıyoruz (Sistemin ne kadar hızlı yanıt verdiğini ölçeceğiz)
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // URL'e GET isteği atıyoruz
                var response = await _httpClient.GetAsync(_url, cancellationToken);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    var result = HealthCheckResult.Healthy($"Status Code: {(int)response.StatusCode}");
                    result.Duration = stopwatch.Elapsed;
                    return result;
                }

                // Eğer sunucu hata döndüyse
                var unhealthyResult = HealthCheckResult.Unhealthy($"HTTP isteği başarısız oldu. Status Code: {(int)response.StatusCode}");
                unhealthyResult.Duration = stopwatch.Elapsed;
                return unhealthyResult;
            }
            catch (Exception ex)
            {
                // Site hiç yoksa, internet koptuysa veya timeout olduysa buraya düşer
                stopwatch.Stop();
                var exceptionResult = HealthCheckResult.Unhealthy($"HTTP isteği sırasında hata oluştu: {ex.Message}", ex);
                exceptionResult.Duration = stopwatch.Elapsed;
                return exceptionResult;
            }
        }
    }
}
