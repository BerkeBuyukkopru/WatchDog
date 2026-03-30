using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;
using Watchdog.Infrastructure.Persistence;

namespace Watchdog.Infrastructure.Persistence.Repositories
{
    // EF Core kullanarak SQL Server ile fiziksel iletişimi kurar.
    public class SystemConfigurationRepository : ISystemConfigurationRepository
    {
        private readonly WatchdogDbContext _context;

        // Dependency Injection (DI) ile DbContext içeri alınır.
        public SystemConfigurationRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<SystemConfiguration?> GetAsync()
        {
            // FirstOrDefaultAsync: SQL'e "SELECT TOP 1 ... WHERE Id = 1" sorgusu fırlatır.
            return await _context.SystemConfigurations.FirstOrDefaultAsync(x => x.Id == 1);
        }

        public async Task<bool> UpdateAsync(SystemConfiguration config)
        {
            // Gelen nesneyi EF Core takip listesinde "Güncellenecek" olarak işaretle.
            _context.SystemConfigurations.Update(config);

            // Değişiklikleri SQL Server'a "COMMIT" et.
            var result = await _context.SaveChangesAsync();

            // Eğer 1 satır bile etkilendiyse işlem başarılıdır.
            return result > 0;
        }
    }
}