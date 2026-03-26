using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    //Entity Framework Core (EF Core) kütüphanesini kullanarak veritabanına fiziksel emirler gönderiyoruz.
    public class MonitoredAppRepository : IMonitoredAppRepository
    {
        private readonly WatchdogDbContext _context;

        // Dependency Injection: Veritabanı bağlantı nesnesini (DbContext) içeri alıyoruz.
        public MonitoredAppRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MonitoredApp>> GetAllAsync()
        {
            return await _context.MonitoredApps.ToListAsync();
        }

        public async Task<MonitoredApp?> GetByIdAsync(Guid id)
        {
            return await _context.MonitoredApps.FindAsync(id);
        }

        public async Task<bool> AddAsync(MonitoredApp app)
        {
            await _context.MonitoredApps.AddAsync(app);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var app = await _context.MonitoredApps.FindAsync(id);
            if (app == null) return false;

            _context.MonitoredApps.Remove(app);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        // URL zaten var mı diye bakar.
        public async Task<bool> IsUrlExistAsync(string healthUrl)
        {
            return await _context.MonitoredApps.AnyAsync(a => a.HealthUrl == healthUrl);
        }
    }
}
