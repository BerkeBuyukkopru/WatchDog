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

        public async Task<bool> SetActiveProviderAsync(Guid id)
        {
            // Sadece silinmemiş sağlayıcılar arasında işlem yap.
            var providers = await _context.AiProviders
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var targetProvider = providers.FirstOrDefault(p => p.Id == id);

            if (targetProvider == null) return false;

            // Hedef sağlayıcıyı aktif (kullanılabilir) yap (Zaten aktifse bir şey değişmez)
            targetProvider.IsActive = true;

            // --- YENİ MANTIK: Tüm uygulamaları bu yeni sağlayıcıya bağla ---
            var apps = await _context.MonitoredApps
                .Where(a => !a.IsDeleted)
                .ToListAsync();

            foreach (var app in apps)
            {
                app.ActiveAiProviderId = id;
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
    }
}