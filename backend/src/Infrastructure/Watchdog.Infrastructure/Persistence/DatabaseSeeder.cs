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
                    CriticalCpuThreshold = 90.0,
                    CriticalRamThreshold = 90.0,
                    CriticalLatencyThreshold = 1000.0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedBy = "System Seeder",
                    IsDeleted = false
                });

                // KRİTİK EKLEME: Kaydı hemen veritabanına mühürlüyoruz. 
                // Böylece diğer projeler (Worker vb.) AnyAsync() kontrolü yaptığında kaydı hemen görür 
                // ve duplikasyon (çift kayıt) oluşmaz.
                await _context.SaveChangesAsync();
            }

            // 3. AI PROVIDERS TOHUMLAMA (Dinamik ID ve Çift Kontrol)

            // OLLAMA
            var existingOllama = await _context.AiProviders.FirstOrDefaultAsync(p => p.Name == "Ollama");
            if (existingOllama == null)
            {
                await _context.AiProviders.AddAsync(new AiProvider
                {
                    Id = Guid.NewGuid(),
                    Name = "Ollama",
                    ModelName = "phi3:mini",
                    ApiUrl = "http://host.docker.internal:11434",
                    IsActive = false
                });
            }
            else if (existingOllama.ModelName != "phi3:mini")
            {
                // Mevcut kaydı "mini"ye güncelle (Eski "medium" kalmasın)
                existingOllama.ModelName = "phi3:mini";
                _context.AiProviders.Update(existingOllama);
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

            // 4. MEVCUT KAYITLARI GÜNCELLE (Akıllı Ortam Tespiti)
            // Docker'da mıyız? Kontrol et.
            bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            string targetHost = isDocker ? "host.docker.internal" : "localhost";
            string sourceHost = isDocker ? "localhost" : "host.docker.internal";

            var allApps = await _context.MonitoredApps.ToListAsync();
            foreach (var app in allApps)
            {
                if (app.HealthUrl.Contains(sourceHost))
                {
                    app.HealthUrl = app.HealthUrl.Replace(sourceHost, targetHost);
                }
            }

            var allProviders = await _context.AiProviders.ToListAsync();
            foreach (var provider in allProviders)
            {
                if (provider.ApiUrl != null && provider.ApiUrl.Contains(sourceHost))
                {
                    provider.ApiUrl = provider.ApiUrl.Replace(sourceHost, targetHost);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}