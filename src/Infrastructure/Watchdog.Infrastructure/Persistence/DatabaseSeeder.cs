using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Watchdog.Infrastructure.Persistence
{
    // Uygulama ayağa kalkarken çalışan ve kritik verileri veritabanına eken servis.
    public class DatabaseSeeder
    {
        private readonly WatchdogDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public DatabaseSeeder(WatchdogDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync()
        {
            // EĞER tabloda hiç kullanıcı yoksa (İlk kurulum anıysa)
            if (!await _context.AdminUsers.AnyAsync())
            {
                var defaultAdmin = new AdminUser
                {
                    Id = Guid.NewGuid(), // Artık dinamik kimlik verebiliriz!
                    Username = "admin",
                    PasswordHash = _passwordHasher.HashPassword("admin123"), // Merkezi şifreleyiciyi kullandık
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.AdminUsers.AddAsync(defaultAdmin);
                await _context.SaveChangesAsync();
            }
            // Tabloda biri varsa bu metot sessizce kapanır, kimsenin şifresini ezmez!
        }
    }
}
