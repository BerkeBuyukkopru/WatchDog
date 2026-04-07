using System;
using System.Collections.Generic;
using System.Threading.Tasks; // Task kullanımı için her iki taraftan da teyitli
using Watchdog.Application.DTOs.AI; // Berke'nin stratejik tahmin DTO'su için gerekli
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    // Sağlık Kayıtları (Snapshot) Depo Sözleşmesi. Sistemin "Kısa Süreli Hafızası" bu interface üzerinden yönetilir.
    public interface ISnapshotRepository
    {
        // Worker motorunun her ping sonrası sonucu kaydettiği metot.
        Task AddAsync(HealthSnapshot snapshot);

        // IncidentRules'un (3-Strike Kuralı) çalışabilmesi için veritabanından en taze "count" kadar kaydı getirir. 
        // (Hangi uygulama kontrol ediliyor? Son kaç kayda bakacağız?)
        Task<List<HealthSnapshot>> GetLatestSnapshotsAsync(Guid appId, int count);

        // Swagger ve React geçmişi için tüm sistemin son kayıtlarını getirir.
        Task<IEnumerable<HealthSnapshot>> GetLatestGlobalAsync(int count);

        // Rutin Yapay Zeka analizi için, belirli bir tarihten (örneğin son 1 saat) bu yana olan kayıtları getirir.
        Task<List<HealthSnapshot>> GetSnapshotsSinceAsync(Guid appId, DateTime since);

        // --- YENİ EKLENENLER (BERKE: AIOPS & STRATEJİK TAHMİN) ---

        // AIOps stratejik tahmini için zenginleştirilmiş günlük özet listesi döner.
        Task<List<DailyEnrichedSnapshotDto>> GetDailyEnrichedSnapshotsAsync(Guid appId, int days);

        // --- YENİ EKLENENLER (MAIN: UC-9: SICAK/SOĞUK VERİ ARŞİVLEME İÇİN) ---

        // Gece 03:00'te çalışan arşivleme motoru için, belirlenen günden (cutoffDate) daha eski olan "Soğuk" verileri getirir.
        Task<IEnumerable<HealthSnapshot>> GetSnapshotsOlderThanAsync(DateTime cutoffDate);

        // Diske başarıyla sıkıştırılıp kaydedilen bu eski verileri, veritabanından kalıcı olarak siler (Hard Delete).
        Task RemoveRangeAsync(IEnumerable<HealthSnapshot> snapshots);
    }
}