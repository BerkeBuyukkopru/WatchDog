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
            // 0. OTOMATİK MİGRASYON
            // Uygulama ayağa kalkarken veritabanı şemasını en güncel hale getirir.
            // Docker ortamında manuel 'update-database' komutu ihtiyacını ortadan kaldırır.
            await _context.Database.MigrateAsync();

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

            // 3. AI PROVIDERS TOHUMLAMA (Minimalist ve Pasif Yapı)
            // Eğer sistemde en az bir tane (silinmemiş) sağlayıcı varsa, seeder hiçbir şey yapmaz.
            // Bu sayede kullanıcı kendi yapılandırmasını (örn: Mixtral) yaptığında sistem varsayılanları dayatmaz.
            if (!await _context.AiProviders.AnyAsync(p => !p.IsDeleted))
            {
                var ollamaId = Guid.Parse("89ac6b3d-6efa-4a33-8916-5f8a3ebf020a");
                await _context.AiProviders.AddAsync(new AiProvider
                {
                    Id = ollamaId,
                    Name = "Ollama",
                    ModelName = "phi3",
                    ApiUrl = "http://host.docker.internal:11434",
                    IsActive = true, // İlk kurulumda tek motor olduğu için varsayılan aktif
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System Seeder"
                });

                await _context.SaveChangesAsync();
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