using System;
using System.Collections.Generic;
using System.Linq; // Where, OrderBy vb. LINQ metotları için gereklidir.
using System.Text;
using System.Threading.Tasks; // Task yapısı için gereklidir.
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.DTOs.AI; // Zenginleştirilmiş DTO için gerekli
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    // Sağlık Kayıtları Deposu. EF Core üzerinden SQL Server ile en performanslı şekilde konuşur.
    // Sistemin "Kısa Süreli Hafızası" bu sınıf üzerinden yönetilir.
    public class SnapshotRepository : ISnapshotRepository
    {
        private readonly WatchdogDbContext _context;

        public SnapshotRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(HealthSnapshot snapshot)
        {
            // Her ping sonucu buraya bir satır olarak mühürlenir.
            await _context.HealthSnapshots.AddAsync(snapshot);
            await _context.SaveChangesAsync();
        }

        // IncidentRules (3-Strike) için gerekli veriyi sağlar.
        public async Task<List<HealthSnapshot>> GetLatestSnapshotsAsync(Guid appId, int count)
        {
            // LINQ TO SQL: Bu sorgu SQL'de "SELECT TOP (X) ... ORDER BY Timestamp DESC" olur.
            return await _context.HealthSnapshots
                .AsNoTracking() // Performans: Sadece okuma yapıldığı için takip etme.
                .Where(s => s.AppId == appId) // Önce sadece hedef uygulamayı seç.
                .OrderByDescending(s => s.Timestamp) // En yeni kaydı en başa al (WDG044 gereği).
                .Take(count) // Sadece ihtiyacımız olan kadarını (Örn: 3) RAM'e çek.
                .ToListAsync();
        }

        public async Task<IEnumerable<HealthSnapshot>> GetLatestGlobalAsync(int count)
        {
            return await _context.HealthSnapshots
                .AsNoTracking()
                .Include(s => s.App) // <--- KRİTİK: Uygulama bilgilerini JOIN ile getirir.
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        // Worker'ın rutin yapay zeka analizi için geçmişe dönük 1 saatlik (veya istenen sürelik) veriyi getirir.
        public async Task<List<HealthSnapshot>> GetSnapshotsSinceAsync(Guid appId, DateTime since)
        {
            return await _context.HealthSnapshots
                .AsNoTracking()
                .Where(s => s.AppId == appId && s.Timestamp >= since)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        // --- BERKE: AIOPS STRATEJİK TAHMİN METODU ---

        // Milyonlarca logu RAM'e almadan, SQL seviyesinde filtreleyip C# tarafında gün gün zenginleştirerek özetler.
        public async Task<List<DailyEnrichedSnapshotDto>> GetDailyEnrichedSnapshotsAsync(Guid appId, int days)
        {
            var sinceTime = DateTime.UtcNow.AddDays(-days);

            // 1. ADIM: Sadece ihtiyacımız olan kolonları çekiyoruz (Select). Tüm tabloyu RAM'e almaktan kurtarır.
            var rawData = await _context.HealthSnapshots
                .AsNoTracking()
                .Where(s => s.AppId == appId && s.Timestamp >= sinceTime)
                .Select(s => new
                {
                    s.Timestamp,
                    s.AppCpuUsage,
                    s.AppRamUsage,
                    s.TotalDuration,
                    s.Status,
                    s.DependencyDetails
                })
                .ToListAsync();

            if (!rawData.Any()) return new List<DailyEnrichedSnapshotDto>();

            // --- KURUMSAL STANDART: Sunucu saatine güvenme, zaman dilimini açıkça belirt ---
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

            // C# tarafında (Memory'de) günlere göre grupla ve zenginleştir (Enriched DTO).
            var enrichedList = rawData
                // KRİTİK: UTC tarihi Türkiye saatine çevirip öyle grupluyoruz. Docker'da bile şaşmaz.
                .GroupBy(s => TimeZoneInfo.ConvertTimeFromUtc(s.Timestamp, turkeyTimeZone).Date)
                .Select(g =>
                {
                    var dailyRecords = g.ToList();

                    // O gün içindeki en yüksek CPU kullanımının olduğu anı (Zirve - Peak) bul
                    var peakRecord = dailyRecords.OrderByDescending(x => x.AppCpuUsage).First();

                    // O günkü hataları topla, adetlerine göre say ve en çok tekrar eden ilk 3'ünü al
                    var topErrors = dailyRecords
                        .Where(x => x.Status == Watchdog.Domain.Enums.HealthStatus.Unhealthy && !string.IsNullOrWhiteSpace(x.DependencyDetails))
                        .GroupBy(x => x.DependencyDetails)
                        .OrderByDescending(errGroup => errGroup.Count())
                        .Take(3)
                        .Select(errGroup => $"{errGroup.Key} (x{errGroup.Count()})")
                        .ToList();

                    return new DailyEnrichedSnapshotDto
                    {
                        Date = g.Key,
                        AvgCpu = Math.Round((double)dailyRecords.Average(x => x.AppCpuUsage), 2),
                        AvgRam = Math.Round((double)dailyRecords.Average(x => x.AppRamUsage), 2),
                        AvgLatency = Math.Round((double)dailyRecords.Average(x => x.TotalDuration), 2),
                        MaxCpu = Math.Round((double)peakRecord.AppCpuUsage, 2),
                        MaxRam = Math.Round((double)peakRecord.AppRamUsage, 2),
                        // Zirve saatini de rapor için TR saatine çeviriyoruz
                        PeakHour = TimeZoneInfo.ConvertTimeFromUtc(peakRecord.Timestamp, turkeyTimeZone).ToString("HH:mm"),
                        TopErrors = topErrors
                    };
                })
                .OrderByDescending(d => d.Date) // En yeni gün en üstte olsun
                .ToList();

            return enrichedList;
        }

        // --- MAIN: UC-9 ARŞİVLEME VE TEMİZLİK METOTLARI ---

        // (YENİ) Aylık Arşivleme Motoru: Belirli tarih aralığındaki verileri getirir.
        public async Task<List<HealthSnapshot>> GetSnapshotsByDateRangeAsync(DateTime startDate, DateTime endDate, int batchSize)
        {
            // DİKKAT: IsDeleted olanları getirme ki sonsuz döngüye girmeyelim!
            return await _context.HealthSnapshots
                .Where(s => s.Timestamp >= startDate && s.Timestamp <= endDate && !s.IsDeleted)
                .OrderBy(s => s.Timestamp)
                .Take(batchSize)
                .ToListAsync();
        }

        // Arşivlenen verileri HARD DELETE (Kalıcı Silme) ile temizler.
        public async Task RemoveRangeAsync(IEnumerable<HealthSnapshot> snapshots)
        {
            if (snapshots == null || !snapshots.Any()) return;

            var ids = snapshots.Select(s => s.Id).ToList();
            
            // EF Core 7+ ExecuteDelete: SaveChangesAsync'teki Soft Delete mantığını bypass eder 
            // ve veriyi fiziksel olarak SQL'den kazır. Arşivleme için gereken budur.
            await _context.HealthSnapshots
                .Where(s => ids.Contains(s.Id))
                .ExecuteDeleteAsync();

            // Hafızayı temizle
            _context.ChangeTracker.Clear();
        }
    }
}