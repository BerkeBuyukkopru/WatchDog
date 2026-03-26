using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities; 
using Microsoft.EntityFrameworkCore;

namespace Watchdog.Infrastructure.Persistence
{
    public class WatchdogDbContext: DbContext
    {
        public WatchdogDbContext(DbContextOptions<WatchdogDbContext> options) : base(options)
        {
        }

        // SQL Server'da tabloya dönüşecek olan sınıflarımız
        public DbSet<MonitoredApp> MonitoredApps { get; set; }
        public DbSet<HealthSnapshot> HealthSnapshots { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<AiInsight> AiInsights { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MonitoredApp Kuralları
            modelBuilder.Entity<MonitoredApp>(entity =>
            {
                entity.HasKey(e => e.Id); // Birincil Anahtarı
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200); // İsim boş geçilemez (NOT NULL) ve maksimum 200 karakter olabilir (VARCHAR(200)).
                entity.Property(e => e.HealthUrl).IsRequired().HasMaxLength(500); // URL boş geçilemez ve maksimum 500 karakter olabilir.
            });

            // HealthSnapshot Kuralları
            modelBuilder.Entity<HealthSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Alt detayların JSON olarak saklanması için nvarchar(max) tipi zorunludur.
                entity.Property(e => e.DependencyDetails).HasColumnType("nvarchar(max)");

                // HealthSnapshot (ve Incident, AiInsight) içindeki ilişki ayarı:
                entity.HasOne(e => e.App) // "Her bir Snapshot'ın BİR TANE App'i (Uygulaması) vardır"
                      .WithMany()         // "Bir App'in BİRDEN FAZLA Snapshot'ı olabilir"
                      .HasForeignKey(e => e.AppId) // "Aralarındaki bağ AppId kolonu üzerinden kurulur"
                      .OnDelete(DeleteBehavior.Cascade); // KURAL: Ana uygulama silinirse, bu logları da sil!
            });

            // 3. Incident Kuralları ve İlişkisi
            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.App)
                      .WithMany()
                      .HasForeignKey(e => e.AppId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. AiInsight Kuralları ve İlişkisi
            modelBuilder.Entity<AiInsight>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.App)
                      .WithMany()
                      .HasForeignKey(e => e.AppId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 5. SystemConfiguration Kuralları ve Başlangıç Verisi
            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Sistem ilk ayağa kalktığında tabloya varsayılan değerleri yerleştir (Seed Data)
                entity.HasData(new SystemConfiguration
                {
                    Id = 1,
                    ActiveAiProvider = "Ollama", // Varsayılan yapay zeka
                    CriticalCpuThreshold = 90.0, // %90 CPU sınırı
                    CriticalRamThreshold = 90.0  // 2GB RAM sınırı
                });
            });
        }
    }
}
