using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    public class SnapshotRepository : ISnapshotRepository
    {
        private readonly WatchdogDbContext _context;

        public SnapshotRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(HealthSnapshot snapshot)
        {
            await _context.HealthSnapshots.AddAsync(snapshot);
            await _context.SaveChangesAsync();
        }

        public async Task<List<HealthSnapshot>> GetLatestSnapshotsAsync(Guid appId, int count)
        {
            return await _context.HealthSnapshots
                .AsNoTracking() // Performans: Sadece okuma yapıldığı için takip etme.
                .Where(s => s.AppId == appId)
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<HealthSnapshot>> GetLatestGlobalAsync(int count)
        {
            return await _context.HealthSnapshots
                .AsNoTracking()
                .Include(s => s.App)
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<HealthSnapshot>> GetSnapshotsSinceAsync(Guid appId, DateTime since)
        {
            return await _context.HealthSnapshots
                .AsNoTracking()
                .Where(s => s.AppId == appId && s.Timestamp >= since)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        // --- ARŞİVLEME: PERFORMANS OPTİMİZE EDİLDİ ---
        public async Task<IEnumerable<HealthSnapshot>> GetSnapshotsOlderThanAsync(DateTime cutoffDate)
        {
            return await _context.HealthSnapshots
                .AsNoTracking() // Bellek dostu: Binlerce satırı RAM'de takip etmez.
                .Where(s => s.Timestamp < cutoffDate)
                .ToListAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<HealthSnapshot> snapshots)
        {
            if (snapshots == null || !snapshots.Any()) return;

            // Toplu silme işlemi
            _context.HealthSnapshots.RemoveRange(snapshots);
            await _context.SaveChangesAsync();
        }
    }
}