using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchdog.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Watchdog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class HealthController : ControllerBase
    {
        private readonly IEnumerable<IHealthCheck> _healthChecks;
        private readonly ISystemConfigurationRepository _configRepository;

        public HealthController(
            IEnumerable<IHealthCheck> healthChecks,
            ISystemConfigurationRepository configRepository)
        {
            _healthChecks = healthChecks;
            _configRepository = configRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var checkResults = new Dictionary<string, string>();
            var metrics = new Dictionary<string, object>();
            bool isHealthy = true;

            // Veritabanındaki güncel eşik değerlerini al
            var config = await _configRepository.GetAsync();
            var cpuThreshold = config?.CriticalCpuThreshold ?? 90.0;
            var ramThreshold = config?.CriticalRamThreshold ?? 90.0;

            foreach (var check in _healthChecks)
            {
                var result = await check.CheckHealthAsync();

                // Sensörün kendi içindeki statik eşiği yerine DB'deki dinamik eşiğe göre değerlendir
                var currentStatus = result.Status;

                if (result.Data != null)
                {
                    foreach (var item in result.Data) metrics[item.Key] = item.Value;

                    // Dinamik CPU Kontrolü
                    if (result.Data.TryGetValue("system_cpu_percent", out var cpuVal))
                    {
                        var cpu = Convert.ToDouble(cpuVal);
                        if (cpu >= cpuThreshold) currentStatus = HealthStatus.Degraded;
                    }
                    
                    // Dinamik RAM Kontrolü
                    if (result.Data.TryGetValue("system_ram_percent", out var ramVal))
                    {
                        var ram = Convert.ToDouble(ramVal);
                        if (ram >= ramThreshold) currentStatus = HealthStatus.Degraded;
                    }
                }

                if (currentStatus != HealthStatus.Healthy) isHealthy = false;
                checkResults[check.Name] = currentStatus.ToString();
            }

            var response = new
            {
                status = isHealthy ? "Healthy" : "Degraded",
                checks = checkResults,
                metrics = metrics,
                thresholds = new { cpu = cpuThreshold, ram = ramThreshold } // Bilgi amaçlı ekledik
            };

            return Ok(response);
        }
    }
}