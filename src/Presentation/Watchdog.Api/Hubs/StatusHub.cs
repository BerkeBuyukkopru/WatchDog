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
            // 3. "ReceiveStatusUpdate": React'in JavaScript tarafında dinlediği radyo frekansının adıdır[cite: 466, 467].
            // 4. newSnapshot: O frekanstan göndereceğimiz taze veri paketi.
            await Clients.All.SendAsync("ReceiveStatusUpdate", newSnapshot);
        }
    }
}