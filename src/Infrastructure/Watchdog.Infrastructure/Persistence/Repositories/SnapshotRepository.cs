using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    // Sağlık Kayıtları Deposu. EF Core üzerinden SQL Server ile en performanslı şekilde konuşur.
    public class SnapshotRepository: ISnapshotRepository
    {
        private readonly WatchdogDbContext _context;

        public SnapshotRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(HealthSnapshot snapshot)
        {
            //Her ping sonucu buraya bir satır olarak mühürlenir.
            await _context.HealthSnapshots.AddAsync(snapshot);
            await _context.SaveChangesAsync();
        }

        // IncidentRules (3-Strike) için gerekli veriyi sağlar.
        public async Task<List<HealthSnapshot>> GetLatestSnapshotsAsync(Guid appId, int count)
        {
            // LINQ TO SQL: Bu sorgu SQL'de "SELECT TOP (X) ... ORDER BY Timestamp DESC" olur.
            return await _context.HealthSnapshots
                .Where(s => s.AppId == appId) // 1. Önce sadece hedef uygulamayı seç.
                .OrderByDescending(s => s.Timestamp) // 2. En yeni kaydı en başa al (WDG044 gereği).
                .Take(count) // 3. Sadece ihtiyacımız olan kadarını (Örn: 3) RAM'e çek.
                .ToListAsync();
        }

        public async Task<IEnumerable<HealthSnapshot>> GetLatestGlobalAsync(int count)
        {
            return await _context.HealthSnapshots
                .Include(s => s.App) // <--- KRİTİK: Uygulama bilgilerini JOIN ile getirir.
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
