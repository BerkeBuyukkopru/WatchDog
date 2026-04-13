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

        public async Task<Incident?> GetActiveIncidentAsync(Guid appId)
        {
            // Sadece çözülmemiş (ResolvedAt == null) olanları değil, aynı zamanda SİLİNMEMİŞ (!IsDeleted) olanları aramalıyız.
            return await _context.Incidents
                .FirstOrDefaultAsync(i => i.AppId == appId && i.ResolvedAt == null && !i.IsDeleted);
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
    }
}