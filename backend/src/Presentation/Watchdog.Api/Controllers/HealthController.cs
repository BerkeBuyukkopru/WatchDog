using Watchdog.Infrastructure.Monitoring;
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
        private readonly ILocalHostMonitor _hostMonitor;
        private readonly ISystemConfigurationRepository _configRepository;

        public HealthController(
            ILocalHostMonitor hostMonitor,
            ISystemConfigurationRepository configRepository)
        {
            _hostMonitor = hostMonitor;
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

            // Sensörden güncel donanım verisini çek (Backend'in yeni bağımsız servisi)
            var currentMetrics = _hostMonitor.GetCurrentHostMetrics();

            metrics["system_cpu_percent"] = currentMetrics.SystemCpu;
            metrics["system_ram_percent"] = currentMetrics.SystemRam;
            metrics["free_disk_gb"] = currentMetrics.FreeDiskGb;
            metrics["total_disk_gb"] = currentMetrics.TotalDiskGb;

            // Dinamik Eşik Kontrolleri (DB'den gelen)
            if (currentMetrics.SystemCpu >= cpuThreshold) isHealthy = false;
            if (currentMetrics.SystemRam >= ramThreshold) isHealthy = false;

            checkResults["System.CPU"] = currentMetrics.SystemCpu >= cpuThreshold ? "Degraded" : "Healthy";
            checkResults["System.RAM"] = currentMetrics.SystemRam >= ramThreshold ? "Degraded" : "Healthy";
            checkResults["System.Storage"] = currentMetrics.FreeDiskGb <= 5.0 ? "Degraded" : "Healthy";

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