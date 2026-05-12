namespace Watchdog.Application.Interfaces.Monitoring
{
    public interface ICentralMetricsProvider
    {
        CentralSystemMetricsDto GetLatestMetrics();
        void UpdateMetrics(CentralSystemMetricsDto metrics);
    }
}
