using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.DTOs;

namespace Watchdog.Application.Interfaces
{
    public interface IHealthProbeClient
    {
        Task<ProbeResult> CheckHealthAsync(string healthUrl, CancellationToken cancellationToken = default);
    }
}