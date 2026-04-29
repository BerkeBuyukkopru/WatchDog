using Microsoft.AspNetCore.SignalR;
using Watchdog.Domain.Entities;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.DTOs.Monitoring;

namespace Watchdog.Api.Hubs
{
    // Hub'dan türetiyoruz ki SignalR'ın tüm "Canlı Yayın" yeteneklerini kazansın.
    public class StatusHub : Hub
    {
        // 1. Worker motoru, veritabanına kaydı bitirince BU metodu çağıracak.
        public async Task BroadcastNewStatus(HealthSnapshot newSnapshot)
        {
            // 2. Clients.All: "Şu an bu tünele bağlı olan tüm tarayıcılara (React) seslen" demektir.
            // 3. "ReceiveStatusUpdate": React'in JavaScript tarafında dinlediği radyo frekansının adıdır.
            // 4. newSnapshot: O frekanstan göndereceğimiz taze veri paketi.
            await Clients.All.SendAsync("ReceiveStatusUpdate", newSnapshot);
        }

        // Yapay zeka analiz raporlarını (Insights) React'e fırlatır
        public async Task BroadcastNewInsight(AiInsightDto newInsight)
        {
            // Frontend tarafı bu veriyi "ReceiveNewInsight" olayını (event) dinleyerek yakalayacak
            await Clients.All.SendAsync("ReceiveNewInsight", newInsight);
        }

        public async Task BroadcastAllInsightsResolved(Guid appId)
        {
            await Clients.All.SendAsync("ReceiveAllInsightsResolved", appId);
        }

        public async Task BroadcastNewIncident(IncidentDto newIncident)
        {
            await Clients.All.SendAsync("ReceiveNewIncident", newIncident);
        }

        public async Task BroadcastResolvedIncident(IncidentDto resolvedIncident)
        {
            await Clients.All.SendAsync("ReceiveResolvedIncident", resolvedIncident);
        }
    }
}