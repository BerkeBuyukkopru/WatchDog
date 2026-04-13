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

        // Dependency Injection: Veritabanı bağlantı nesnesini (DbContext) içeri alıyoruz.
        public MonitoredAppRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MonitoredApp>> GetAllAsync()
        {
            // SADECE AKTİF (Silinmemiş) uygulamaları getir
            return await _context.MonitoredApps.Where(a => a.IsActive).ToListAsync();
        }

        public async Task<MonitoredApp?> GetByIdAsync(Guid id)
        {
            // Silinmiş bir uygulamayı getirmemesi için IsActive kontrolü
            return await _context.MonitoredApps.FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
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
            if (app == null || !app.IsActive) return false;

            // HARD DELETE YERİNE SOFT DELETE YAPIYORUZ
            app.IsActive = false;
            _context.MonitoredApps.Update(app);

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateAsync(MonitoredApp app)
        {
            _context.MonitoredApps.Update(app);
            return await _context.SaveChangesAsync() > 0;
        }

        // URL zaten var mı diye bakar.
        public async Task<bool> IsUrlExistAsync(string healthUrl)
        {
            // Sadece aktif uygulamalar arasında bu URL var mı diye bak
            return await _context.MonitoredApps.AnyAsync(a => a.HealthUrl == healthUrl && a.IsActive);
        }
    }
}
