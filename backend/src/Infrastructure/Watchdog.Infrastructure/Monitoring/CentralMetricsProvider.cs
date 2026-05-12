using Watchdog.Application.Interfaces.Monitoring;

namespace Watchdog.Infrastructure.Monitoring
{
    public class CentralMetricsProvider : ICentralMetricsProvider
    {
        private CentralSystemMetricsDto _latestMetrics = new CentralSystemMetricsDto
        {
            SystemCpu = 0,
            SystemRam = 0,
            FreeDiskGb = 0,
            TotalDiskGb = 0,
            LastUpdated = System.DateTime.MinValue
        };

        private readonly object _lock = new object();

        public CentralSystemMetricsDto GetLatestMetrics()
        {
            lock (_lock)
            {
                // Değer türleri veya değişmezler içeren yeni bir kopyasını dönüyoruz ki referans hatası olmasın
                return new CentralSystemMetricsDto
                {
                    SystemCpu = _latestMetrics.SystemCpu,
                    SystemRam = _latestMetrics.SystemRam,
                    FreeDiskGb = _latestMetrics.FreeDiskGb,
                    TotalDiskGb = _latestMetrics.TotalDiskGb,
                    LastUpdated = _latestMetrics.LastUpdated
                };
            }
        }

        public void UpdateMetrics(CentralSystemMetricsDto metrics)
        {
            lock (_lock)
            {
                _latestMetrics = metrics;
            }
        }
    }
}
