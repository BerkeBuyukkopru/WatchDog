using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    //Entity Framework Core (EF Core) kütüphanesini kullanarak veritabanına fiziksel emirler gönderiyoruz.
    public class MonitoredAppRepository : IMonitoredAppRepository
    {
        private readonly WatchdogDbContext _context;

        public MonitoredAppRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MonitoredApp>> GetAllAsync()
        {
            return await _context.MonitoredApps
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<MonitoredApp?> GetByIdAsync(Guid id)
        {
            // Silinmiş bir veri sistemde "yok" hükmündedir.
            return await _context.MonitoredApps
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<bool> AddAsync(MonitoredApp app)
        {
            await _context.MonitoredApps.AddAsync(app);
            // DbContext içindeki SaveChangesAsync, parmak izini (CreatedBy) otomatik basacak.
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var app = await _context.MonitoredApps.FindAsync(id);
            if (app == null || app.IsDeleted) return false;

            _context.MonitoredApps.Remove(app);

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateAsync(MonitoredApp app)
        {
            _context.MonitoredApps.Update(app);
            // DbContext, ModifiedBy ve ModifiedAt alanlarını otomatik dolduracak.
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsUrlExistAsync(string healthUrl)
        {
            // Yeni bir uygulama eklerken, sadece SİLİNMEMİŞ uygulamalar arasında bu URL var mı diye bakıyoruz.
            return await _context.MonitoredApps
                .AnyAsync(a => a.HealthUrl == healthUrl && !a.IsDeleted);
        }
    }
}
