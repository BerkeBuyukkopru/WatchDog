using Microsoft.AspNetCore.SignalR;
using Watchdog.Domain.Entities;

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
        public async Task BroadcastNewInsight(Watchdog.Application.DTOs.AI.AiInsightDto newInsight)
        {
            // Frontend tarafı bu veriyi "ReceiveNewInsight" olayını (event) dinleyerek yakalayacak
            await Clients.All.SendAsync("ReceiveNewInsight", newInsight);
        }
    }
}