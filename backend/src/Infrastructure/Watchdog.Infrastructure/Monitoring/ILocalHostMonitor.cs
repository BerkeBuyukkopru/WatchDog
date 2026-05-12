using Watchdog.Application.Interfaces.Monitoring;

namespace Watchdog.Infrastructure.Monitoring
{
    public interface ILocalHostMonitor
    {
        CentralSystemMetricsDto GetCurrentHostMetrics();
    }
}
