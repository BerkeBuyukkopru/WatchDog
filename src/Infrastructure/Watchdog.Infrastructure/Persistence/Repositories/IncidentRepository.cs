using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    // Kesinti (Incident) Deposu. Karar motorunun (AnalyzeSystemHealthUseCase) hafıza merkezidir.
    public class IncidentRepository : IIncidentRepository
    {
        private readonly WatchdogDbContext _context;

        public IncidentRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<Incident?> GetActiveIncidentAsync(Guid appId)
        {
            // O uygulamaya ait, henüz kapatılmamış (ResolvedAt == null) en güncel olayı getir
            return await _context.Incidents
                .FirstOrDefaultAsync(i => i.AppId == appId && i.ResolvedAt == null);
        }

        public async Task AddAsync(Incident incident)
        {
            // Sistem 3 kez üst üste çöktüğünde yeni bir olay kaydı açılır.
            await _context.Incidents.AddAsync(incident);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Incident incident)
        {
            // Sistem düzeldiğinde veya AI (Ollama) analizini bitirdiğinde mevcut satır güncellenir.
            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();
        }
    }
}
