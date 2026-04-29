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
                .WithUrl("http://localhost:5226/statushub")
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

        // Yapay zeka verisini API'ye fırlatıyoruz
        public async Task BroadcastNewInsightAsync(Watchdog.Application.DTOs.AI.AiInsightDto insight, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                // API'deki "BroadcastNewInsight" isimli Hub metodunu tetikler
                await _hubConnection.InvokeAsync("BroadcastNewInsight", insight, cancellationToken);
            }
        }

        public async Task BroadcastAllInsightsResolvedAsync(Guid appId)
        {
            await EnsureConnectedAsync(default);
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("BroadcastAllInsightsResolved", appId);
            }
        }

        public async Task BroadcastNewIncidentAsync(Watchdog.Application.DTOs.Monitoring.IncidentDto incident, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("BroadcastNewIncident", incident, cancellationToken);
            }
        }

        public async Task BroadcastResolvedIncidentAsync(Watchdog.Application.DTOs.Monitoring.IncidentDto incident, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("BroadcastResolvedIncident", incident, cancellationToken);
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