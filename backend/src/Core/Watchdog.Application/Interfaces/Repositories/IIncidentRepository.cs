using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    // Kesinti (Incident) Depo Sözleşmesi. Karar motorunun veritabanı ile arasındaki 'Hafıza' katmanıdır.
    public interface IIncidentRepository
    {
        // Belirli bir uygulama ve bileşen için henüz kapatılmamış (ResolvedAt == null) olayı getirir
        Task<Incident?> GetActiveIncidentAsync(Guid appId, string componentName);

        // Belirli bir uygulama için tüm aktif (çözülmemiş) olayları getirir
        Task<IEnumerable<Incident>> GetActiveIncidentsAsync(Guid appId);

        // Tüm olayları getirir (opsiyonel olarak AppId ile filtreler)
        Task<IEnumerable<Incident>> GetAllAsync(Guid? appId = null);

        // Id'ye göre tekil olayı getirir
        Task<Incident?> GetByIdAsync(Guid id);

        // Sisteme yeni bir çöküş (Incident) kaydeder
        Task AddAsync(Incident incident);

        // Sistem düzeldiğinde veya AI (Ollama) bir analiz ürettiğinde, mevcut olayı bu metotla güncelleriz.
        Task UpdateAsync(Incident incident);

        // Belirtilen uygulama ID listesine ait tüm olayları getirir.
        Task<IEnumerable<Incident>> GetAllByAppIdsAsync(List<Guid> appIds);
    }
}
