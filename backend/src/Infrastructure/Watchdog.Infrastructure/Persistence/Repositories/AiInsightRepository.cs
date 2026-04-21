using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    // Sınıfımız IAiInsightRepository sözleşmesini imzalıyor
    public class AiInsightRepository : IAiInsightRepository
    {
        private readonly WatchdogDbContext _context;

        public AiInsightRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AiInsight insight)
        {
            await _context.Set<AiInsight>().AddAsync(insight);
            await _context.SaveChangesAsync();
        }

        // Veritabanındaki tavsiyeleri listelemek için
        public async Task<IEnumerable<AiInsight>> GetByAppIdAsync(Guid? appId)
        {
            var query = _context.AiInsights
                .Include(i => i.App)
                .Where(i => !i.IsDeleted)
                .AsQueryable();

            if (appId.HasValue)
                query = query.Where(i => i.AppId == appId.Value);

            return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }

        public async Task<AiInsight?> GetLatestInsightAsync(Guid appId)
        {
            return await _context.AiInsights
                .AsNoTracking() // Sadece okuma yapacağımız için belleği yormuyoruz (Performans)
                .Where(i => i.AppId == appId) // Sadece kriz yaşayan uygulamaya ait analizler
                .OrderByDescending(i => i.CreatedAt) // En yenisi en üstte
                .FirstOrDefaultAsync(); // İlkini (en tazesini) al veya yoksa null dön
        }

        public async Task<AiInsight?> GetByIdAsync(Guid id)
        {
            return await _context.AiInsights.FindAsync(id);
        }

        public async Task UpdateAsync(AiInsight insight)
        {
            _context.AiInsights.Update(insight);
            await _context.SaveChangesAsync();
        }
    }
}
