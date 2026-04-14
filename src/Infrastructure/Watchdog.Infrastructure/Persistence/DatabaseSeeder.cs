using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Watchdog.Domain.Constants;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Domain.Entities;

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
            // 1. ADMİN TOHUMLAMA
            if (!await _context.AdminUsers.AnyAsync())
            {
                await _context.AdminUsers.AddAsync(new AdminUser
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    PasswordHash = _passwordHasher.HashPassword("Admin123!"),
                    Role = RoleConstants.SuperAdmin,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 2. SYSTEM CONFIGURATION TOHUMLAMA
            if (!await _context.SystemConfigurations.AnyAsync())
            {
                await _context.SystemConfigurations.AddAsync(new SystemConfiguration
                {
                    // Configuration tablomuzun BaseEntity'si int ID alıyor varsayarak
                    CriticalCpuThreshold = 90.0,
                    CriticalRamThreshold = 90.0,
                    CriticalLatencyThreshold = 1000.0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedBy = "System Seeder",
                    IsDeleted = false
                });
            }

            // 3. AI PROVIDERS TOHUMLAMA (Dinamik ID ve Çift Kontrol)

            // OLLAMA
            if (!await _context.AiProviders.AnyAsync(p => p.Name == "Ollama" && p.ModelName == "phi3:medium"))
            {
                await _context.AiProviders.AddAsync(new AiProvider
                {
                    Id = Guid.NewGuid(), // Tamamen rastgele!
                    Name = "Ollama",
                    ModelName = "phi3:medium",
                    ApiUrl = "http://localhost:11434",
                    IsActive = false
                });
            }

            // OPENAI
            if (!await _context.AiProviders.AnyAsync(p => p.Name == "OpenAI" && p.ModelName == "gpt-4o-mini"))
            {
                await _context.AiProviders.AddAsync(new AiProvider
                {
                    Id = Guid.NewGuid(), // Tamamen rastgele!
                    Name = "OpenAI",
                    ModelName = "gpt-4o-mini",
                    ApiUrl = "https://api.openai.com/v1",
                    IsActive = false
                });
            }

            // GROQ
            if (!await _context.AiProviders.AnyAsync(p => p.Name == "Groq" && p.ModelName == "llama-3.3-70b-versatile"))
            {
                await _context.AiProviders.AddAsync(new AiProvider
                {
                    Id = Guid.NewGuid(), // Tamamen rastgele!
                    Name = "Groq",
                    ModelName = "llama-3.3-70b-versatile",
                    ApiUrl = "https://api.groq.com/openai/v1",
                    IsActive = true // Varsayılan Aktif
                });
            }

            // Tüm tohumları tek seferde veritabanına mühürle
            await _context.SaveChangesAsync();
        }
    }
}