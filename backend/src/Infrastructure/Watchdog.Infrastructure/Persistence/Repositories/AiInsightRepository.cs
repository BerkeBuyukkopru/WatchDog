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
                .AsNoTracking()
                .Where(i => i.AppId == appId && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<AiInsight?> GetLatestInsightByTypeAsync(Guid appId, Watchdog.Domain.Enums.InsightType type)
        {
            return await _context.AiInsights
                .AsNoTracking()
                .Where(i => i.AppId == appId && i.InsightType == type && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();
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

        public async Task ResolveAllActiveInsightsForAppAsync(Guid appId)
        {
            var activeInsights = await _context.AiInsights
                .Where(i => i.AppId == appId && !i.IsResolved && !i.IsDeleted)
                .ToListAsync();

            if (activeInsights.Any())
            {
                foreach (var insight in activeInsights)
                {
                    insight.IsResolved = true;
                    insight.ModifiedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
