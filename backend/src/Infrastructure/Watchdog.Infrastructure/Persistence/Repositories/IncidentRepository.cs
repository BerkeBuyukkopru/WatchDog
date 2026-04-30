using System;
using System.Collections.Generic;
using System.Linq; // Where/Any için şart
using System.Text;
using System.Threading.Tasks;
using Watchdog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly WatchdogDbContext _context;

        public IncidentRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<Incident?> GetActiveIncidentAsync(Guid appId, string componentName)
        {
            return await _context.Incidents
                .FirstOrDefaultAsync(i => i.AppId == appId && 
                                         i.FailedComponent == componentName && 
                                         i.ResolvedAt == null && 
                                         !i.IsDeleted);
        }

        public async Task<IEnumerable<Incident>> GetActiveIncidentsAsync(Guid appId)
        {
            return await _context.Incidents
                .Where(i => i.AppId == appId && i.ResolvedAt == null && !i.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Incident>> GetAllAsync(Guid? appId = null)
        {
            var query = _context.Incidents
                .Include(i => i.App) // Uygulama bilgilerini de joinleyelim
                .Where(i => !i.IsDeleted);

            if (appId.HasValue)
            {
                query = query.Where(i => i.AppId == appId.Value);
            }

            return await query
                .OrderByDescending(i => i.StartedAt)
                .ToListAsync();
        }

        public async Task<Incident?> GetByIdAsync(Guid id)
        {
            return await _context.Incidents
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task AddAsync(Incident incident)
        {
            await _context.Incidents.AddAsync(incident);
            // DbContext, CreatedAt ve CreatedBy alanlarını otomatik basacaktır.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Incident incident)
        {
            _context.Incidents.Update(incident);
            // DbContext, ModifiedAt ve ModifiedBy alanlarını otomatik güncelleyecektir.
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Incident>> GetAllByAppIdsAsync(List<Guid> appIds)
        {
            return await _context.Incidents
                .Include(i => i.App)
                .Where(i => appIds.Contains(i.AppId) && !i.IsDeleted)
                .OrderByDescending(i => i.StartedAt)
                .ToListAsync();
        }
    }
}