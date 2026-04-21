using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    // Kesinti (Incident) Depo Sözleşmesi. Karar motorunun veritabanı ile arasındaki 'Hafıza' katmanıdır.
    public interface IIncidentRepository
    {
        // Belirli bir uygulama için henüz kapatılmamış (ResolvedAt == null) olayı getirir
        Task<Incident?> GetActiveIncidentAsync(Guid appId);

        // Sisteme yeni bir çöküş (Incident) kaydeder
        Task AddAsync(Incident incident);

        // Sistem düzeldiğinde veya AI (Ollama) bir analiz ürettiğinde, mevcut olayı bu metotla güncelleriz.
        Task UpdateAsync(Incident incident);
    }
}
