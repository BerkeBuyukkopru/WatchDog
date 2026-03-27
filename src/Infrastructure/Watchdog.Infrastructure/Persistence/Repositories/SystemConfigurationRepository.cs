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
    public class SystemConfigurationRepository : ISystemConfigurationRepository
    {
        private readonly WatchdogDbContext _context;

        public SystemConfigurationRepository(WatchdogDbContext context)
        {
            _context = context;
        }

        public async Task<SystemConfiguration?> GetAsync()
        {
            // FirstOrDefaultAsync artık Microsoft.EntityFrameworkCore sayesinde tanınacak
            return await _context.SystemConfigurations.FirstOrDefaultAsync(x => x.Id == 1);
        }

        public async Task<bool> UpdateAsync(SystemConfiguration config)
        {
            _context.SystemConfigurations.Update(config);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}