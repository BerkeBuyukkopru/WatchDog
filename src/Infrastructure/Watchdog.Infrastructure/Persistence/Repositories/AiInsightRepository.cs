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

        // Veritabanındaki tavsiyeleri listelemek için (Arayüze de eklemeyi unutma!)
        public async Task<IEnumerable<AiInsight>> GetByAppIdAsync(Guid? appId)
        {
            var query = _context.AiInsights.Include(i => i.App).AsQueryable();

            if (appId.HasValue)
                query = query.Where(i => i.AppId == appId.Value);

            return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }
    }
}
