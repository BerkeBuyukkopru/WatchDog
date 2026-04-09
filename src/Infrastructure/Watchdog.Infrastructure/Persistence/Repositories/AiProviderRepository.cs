using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    public class AiProviderRepository : IAiProviderRepository
    {
        private readonly WatchdogDbContext _context;

        public AiProviderRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AiProvider>> GetAllAsync()
        {
            // Performans için AsNoTracking eklenebilir ancak güncelleme yapılacağı için takipte kalması iyidir.
            return await _context.AiProviders.ToListAsync<AiProvider>();
        }

        public async Task<AiProvider?> GetActiveProviderAsync()
        {
            // IsActive bayrağı true olan ilk sağlayıcıyı döner.
            return await _context.AiProviders.FirstOrDefaultAsync<AiProvider>(p => p.IsActive);
        }

        public async Task<AiProvider?> GetByIdAsync(Guid id)
        {
            // GUID üzerinden tekil arama.
            return await _context.AiProviders.FindAsync(id);
        }

        public async Task<bool> SetActiveProviderAsync(Guid id)
        {
            var providers = await _context.AiProviders.ToListAsync();
            var targetProvider = providers.FirstOrDefault(p => p.Id == id);

            if (targetProvider == null) return false;

            // RADİKAL DEĞİŞİM (Switch): Tüm sistemi pasife alıp sadece hedefi açıyoruz.
            foreach (var provider in providers)
            {
                provider.IsActive = false;
            }

            targetProvider.IsActive = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(AiProvider provider)
        {
            // Entity Framework takip mekanizmasını kullanarak satırı günceller.
            _context.AiProviders.Update(provider);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
