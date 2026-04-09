using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities; 
using Microsoft.EntityFrameworkCore;

namespace Watchdog.Infrastructure.Persistence
{
    public class WatchdogDbContext : DbContext
    {
        public WatchdogDbContext(DbContextOptions<WatchdogDbContext> options) : base(options) { }

        public DbSet<MonitoredApp> MonitoredApps { get; set; }
        public DbSet<HealthSnapshot> HealthSnapshots { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<AiInsight> AiInsights { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<AiProvider> AiProviders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... Diğer entity ayarları (MonitoredApp vb.) aynı kalıyor ...

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
                        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new AiProvider
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Name = "OpenAI",
                        ModelName = "gpt-4o-mini",
                        IsActive = false,
                        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new AiProvider
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Name = "Groq",
                        ModelName = "llama-3.3-70b-versatile",
                        ApiUrl = "https://api.groq.com/openai/v1",
                        IsActive = false,
                        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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
                    LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            });
        }
    }
}
