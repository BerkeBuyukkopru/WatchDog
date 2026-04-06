using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    //Sağlık Kayıtları (Snapshot) Depo Sözleşmesi. Sistemin "Kısa Süreli Hafızası" bu interface üzerinden yönetilir.
    public interface ISnapshotRepository
    {
        // Worker motorunun her ping sonrası sonucu kaydettiği metot.
        Task AddAsync(HealthSnapshot snapshot);

        // IncidentRules'un (3-Strike Kuralı) çalışabilmesi için veritabanından en taze "count" kadar kaydı getirir. (Hangi uygulama kontrol ediliyor? Son kaç kayda bakacağız?)
        Task<List<HealthSnapshot>> GetLatestSnapshotsAsync(Guid appId, int count);

        // Swagger ve React geçmişi için tüm sistemin son kayıtlarını getirir.
        Task<IEnumerable<HealthSnapshot>> GetLatestGlobalAsync(int count);

        // Rutin Yapay Zeka analizi için, belirli bir tarihten (örneğin son 1 saat) bu yana olan kayıtları getirir.
        Task<List<HealthSnapshot>> GetSnapshotsSinceAsync(Guid appId, DateTime since);
    }
}
