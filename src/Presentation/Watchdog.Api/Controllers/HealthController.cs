using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")] // Dışarıdan "https://localhost:xxxx/health" yazılarak ulaşılacak
    public class HealthController : ControllerBase
    {
        // Sistemdeki tüm IHealthCheck sensörlerini (CPU, RAM, Disk) bir liste olarak buraya alıyoruz
        private readonly IEnumerable<IHealthCheck> _healthChecks;

        public HealthController(IEnumerable<IHealthCheck> healthChecks)
        {
            _healthChecks = healthChecks;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var checkResults = new Dictionary<string, string>();
            var metrics = new Dictionary<string, object>();
            bool isHealthy = true;

            // 1. SİSTEMDEKİ TÜM SENSÖRLERİ ÇALIŞTIR!
            foreach (var check in _healthChecks)
            {
                var result = await check.CheckHealthAsync();

                // Bir sensör bile hata verirse genel sistem "Degraded/Unhealthy" olur
                if (result.Status != HealthStatus.Healthy) isHealthy = false;

                checkResults[check.Name] = result.Status.ToString();

                // Sensörün ölçtüğü sayısal verileri torbaya (metrics) doldur
                if (result.Data != null)
                {
                    foreach (var item in result.Data) metrics[item.Key] = item.Value;
                }
            }

            // 2. RAPORU JSON OLARAK DIŞARIYA SUN! (GTD WDG014 Kuralı)
            var response = new
            {
                status = isHealthy ? "Healthy" : "Degraded",
                checks = checkResults,
                metrics = metrics
            };

            return Ok(response);
        }
    }
}