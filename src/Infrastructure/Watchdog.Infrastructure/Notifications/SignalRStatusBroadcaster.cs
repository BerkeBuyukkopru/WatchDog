using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Notifications
{
    public class SignalRStatusBroadcaster : IStatusBroadcaster, IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly ILogger<SignalRStatusBroadcaster> _logger;

        public SignalRStatusBroadcaster(ILogger<SignalRStatusBroadcaster> logger)
        {
            _logger = logger;

            // Worker'dan aldığımız köprü inşası burada
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7054/statushub")
                .WithAutomaticReconnect()
                .Build();
        }

        private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StartAsync(cancellationToken);
                    _logger.LogInformation("SignalR tüneline başarıyla bağlanıldı.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("SignalR bağlantısı kurulamadı. API kapalı olabilir: {Message}", ex.Message);
                }
            }
        }

        public async Task BroadcastNewStatusAsync(HealthSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            // Sadece bağlıysak fırlat
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("BroadcastNewStatus", snapshot, cancellationToken);
            }
        }

        // Uygulama kapanırken bağlantıyı temizle
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}