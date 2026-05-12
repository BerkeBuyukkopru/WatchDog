using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Monitoring;
using Watchdog.Infrastructure.Monitoring;

namespace Watchdog.Worker.BackgroundServices
{
    public class CentralMetricsCollectorWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CentralMetricsCollectorWorker> _logger;

        public CentralMetricsCollectorWorker(IServiceProvider serviceProvider, ILogger<CentralMetricsCollectorWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CentralMetricsCollectorWorker started. Collecting host metrics every 5 seconds.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Her döngüde scope oluşturarak servisleri alıyoruz
                    using var scope = _serviceProvider.CreateScope();
                    var hostMonitor = scope.ServiceProvider.GetRequiredService<ILocalHostMonitor>();
                    var metricsProvider = scope.ServiceProvider.GetRequiredService<ICentralMetricsProvider>();

                    // 1. Yeni ve Bağımsız Sensörümüzden ölçümleri al
                    var currentMetrics = hostMonitor.GetCurrentHostMetrics();

                    // 2. Kasayı (Singleton) güncelliyoruz
                    metricsProvider.UpdateMetrics(currentMetrics);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting central system metrics.");
                }

                // 5 saniyede bir ölçüm yap
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
