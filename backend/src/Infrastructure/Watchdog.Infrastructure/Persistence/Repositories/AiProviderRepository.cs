using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq; // Where için gerekli
using System.Text;
using System.Threading.Tasks;
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
            // Sadece SİLİNMEMİŞ sağlayıcıları getir.
            return await _context.AiProviders
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<AiProvider?> GetActiveProviderAsync()
        {
            // Hem aktif (IsActive) hem de silinmemiş olmalı.
            return await _context.AiProviders
                .OrderBy(p => p.Name)
                .FirstOrDefaultAsync(p => p.IsActive && !p.IsDeleted);
        }

        public async Task<AiProvider?> GetByIdAsync(Guid id)
        {
            // Silinmiş bir sağlayıcı ID ile çağırılamasın.
            return await _context.AiProviders
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<AiProvider?> GetByIdIncludingDeletedAsync(Guid id)
        {
            return await _context.AiProviders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> SetActiveProviderAsync(Guid id)
        {
            var targetProvider = await _context.AiProviders
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (targetProvider == null) return false;

            // Kullanıcının istediğini bağımsız olarak açıp kapatabilmesi için durumu tersine çevir (Toggle)
            targetProvider.IsActive = !targetProvider.IsActive;

            // --- YENİ MANTIK: Eğer sağlayıcı "Aktif" hale getirildiyse, uygulamaları ona bağla ---
            // Eğer "İnaktif" ediliyorsa diğer aktif sağlayıcılar kullanılmaya devam edebilir
            if (targetProvider.IsActive)
            {
                var apps = await _context.MonitoredApps
                    .Where(a => !a.IsDeleted)
                    .ToListAsync();

                foreach (var app in apps)
                {
                    if (app.ActiveAiProviderId != id)
                    {
                        app.ActiveAiProviderId = id;
                    }
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(AiProvider provider)
        {
            _context.AiProviders.Update(provider);
            // DbContext otomatik olarak ModifiedBy ve ModifiedAt alanlarını dolduracak.
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddAsync(AiProvider provider)
        {
            await _context.AiProviders.AddAsync(provider);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var provider = await _context.AiProviders.FindAsync(id);
            if (provider == null || provider.IsDeleted) return false;

            // Infrastructure katmanındaki Interceptor yapımız sayesinde 
            // Remove çağrısı otomatik olarak IsDeleted = true yapacaktır.
            _context.AiProviders.Remove(provider);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AiProvider?> GetBestFallbackProviderAsync()
        {
            // Öncelik 1: Aktif olan Ollama
            var ollama = await _context.AiProviders
                .Where(p => !p.IsDeleted && p.IsActive && p.Name.Contains("Ollama"))
                .OrderBy(p => p.Name)
                .FirstOrDefaultAsync();

            if (ollama != null) return ollama;

            // Öncelik 2: Herhangi bir Aktif motor
            return await _context.AiProviders
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AiProvider>> GetDeletedProvidersAsync()
        {
            return await _context.AiProviders
                .Where(p => p.IsDeleted)
                .OrderByDescending(p => p.DeletedAt)
                .ToListAsync();
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            var provider = await _context.AiProviders
                .IgnoreQueryFilters() // Silinmişleri de bulabilmek için
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted);

            if (provider == null) return false;

            provider.IsDeleted = false;
            provider.DeletedAt = null;
            provider.DeletedBy = null;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
