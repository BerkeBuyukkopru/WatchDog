using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.DTOs;
using Watchdog.Application.DTOs.Monitoring;

namespace Watchdog.Application.Interfaces.ExternalClients
{
    public interface IHealthProbeClient
    {
        Task<ProbeResult> CheckHealthAsync(string healthUrl, CancellationToken cancellationToken = default);
    }
}