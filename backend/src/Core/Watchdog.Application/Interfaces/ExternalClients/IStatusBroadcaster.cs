using System.Threading;
using System.Threading.Tasks;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.ExternalClients
{
    public interface IStatusBroadcaster
    {
        // Canlı yayın yapacak metot sözleşmemiz
        Task BroadcastNewStatusAsync(HealthSnapshot snapshot, CancellationToken cancellationToken = default);

        // Yapay zeka analizini canlı yayına sokacak metot sözleşmesi
        Task BroadcastNewInsightAsync(AiInsight insight, CancellationToken cancellationToken = default);
    }
}