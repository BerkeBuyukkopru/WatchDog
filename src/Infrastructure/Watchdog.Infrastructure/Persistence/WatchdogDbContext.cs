using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; // CancellationToken ve Task için
using System.Threading;      // CancellationToken için
using Microsoft.EntityFrameworkCore;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Common; // BaseEntity'ye erişim için
using Watchdog.Application.Interfaces.Common; // ICurrentUserService için

namespace Watchdog.Infrastructure.Persistence
{
    public class WatchdogDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        // ICurrentUserService sayesinde her kayıt işleminde "kim yaptı" sorusuna cevap bulacağız.
        public WatchdogDbContext(
            DbContextOptions<WatchdogDbContext> options,
            ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<MonitoredApp> MonitoredApps { get; set; }
        public DbSet<HealthSnapshot> HealthSnapshots { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<AiInsight> AiInsights { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<AiProvider> AiProviders { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

        // Mühendislik Kararı: Veritabanına her kayıt atıldığında araya giriyoruz.
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // O anki login olan kullanıcının kullanıcı adını alıyoruz (Token'dan gelir).
            var user = _currentUserService.Username ?? "System";

            // Bellekteki (ChangeTracker) tüm değişiklikleri tek tek inceliyoruz.
            foreach (var entry in ChangeTracker.Entries())
            {
                // Eğer değişen nesne bizim BaseEntity'mizden türediyse (Guid veya int fark etmeksizin)
                if (entry.Entity is BaseEntity<Guid> or BaseEntity<int>)
                {
                    // Ortak property'lere erişebilmek için dynamic cast yapıyoruz.
                    dynamic entity = entry.Entity;

                    switch (entry.State)
                    {
                        // Yeni bir kayıt ekleniyorsa
                        case EntityState.Added:
                            // Eğer ID atanmamışsa burada atıyoruz (Dinamik hata önlemi)
                            if (entity is BaseEntity && entity.Id == Guid.Empty)
                                entity.Id = Guid.NewGuid();

                            entity.CreatedAt = DateTime.UtcNow;
                            entity.CreatedBy = user;
                            break;

                        // Mevcut bir kayıt güncelleniyorsa
                        case EntityState.Modified:
                            entity.ModifiedAt = DateTime.UtcNow;
                            entity.ModifiedBy = user;
                            break;

                        // Bir kayıt silinmek isteniyorsa (Soft Delete Devreye Girer)
                        case EntityState.Deleted:
                            // EF Core'a "bunu gerçekten silme, sadece durumunu güncelle" diyoruz.
                            entry.State = EntityState.Modified;
                            entity.IsDeleted = true;
                            entity.DeletedAt = DateTime.UtcNow;
                            entity.DeletedBy = user;
                            break;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DİKKAT: AdminUser statik tohumlama kodları kurumsal "Data Seeder" mantığına 
            // geçiş yaptığımız için buradan tamamen temizlendi.

            // AI Provider Seed Data - Sabit GUID'ler ile kurumsal yapılandırma
            modelBuilder.Entity<AiProvider>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                entity.HasData(
                    new AiProvider
                    {
                        // Sabit GUID'ler veriyoruz ki her migration'da ID değişmesin.
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "Ollama",
                        ModelName = "phi3:medium",
                        ApiUrl = "http://localhost:11434",
                        IsActive = true,
                        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        CreatedBy = "System", // Statik seed için eklendi
                        IsDeleted = false     // Statik seed için eklendi
                    },
                    new AiProvider
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Name = "OpenAI",
                        ModelName = "gpt-4o-mini",
                        IsActive = false,
                        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        CreatedBy = "System",
                        IsDeleted = false
                    },
                    new AiProvider
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Name = "Groq",
                        ModelName = "llama-3.3-70b-versatile",
                        ApiUrl = "https://api.groq.com/openai/v1",
                        IsActive = false,
                        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        CreatedBy = "System",
                        IsDeleted = false
                    }
                );
            });

            // SystemConfiguration Seed Data
            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasData(new SystemConfiguration
                {
                    Id = 1,
                    CriticalCpuThreshold = 90.0,
                    CriticalRamThreshold = 90.0,
                    CriticalLatencyThreshold = 1000.0,
                    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System",
                    IsDeleted = false
                });
            });
        }
    }
}