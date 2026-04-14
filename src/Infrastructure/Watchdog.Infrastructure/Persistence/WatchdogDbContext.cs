using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Common;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Infrastructure.Persistence
{
    public class WatchdogDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var user = _currentUserService.Username ?? "System";

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity<Guid> or BaseEntity<int>)
                {
                    dynamic entity = entry.Entity;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            // EĞER ID boşsa (Örn: Seeder'dan geliyorsa) sistem otomatik GUID atar!
                            if (entity is BaseEntity<Guid> && entity.Id == Guid.Empty)
                                entity.Id = Guid.NewGuid();

                            entity.CreatedAt = DateTime.UtcNow;
                            entity.CreatedBy = user;
                            break;

                        case EntityState.Modified:
                            entity.ModifiedAt = DateTime.UtcNow;
                            entity.ModifiedBy = user;
                            break;

                        case EntityState.Deleted:
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

            // AI Provider tablo kuralları
            modelBuilder.Entity<AiProvider>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                // DİKKAT: HasData blokları DatabaseSeeder'a taşındı.
            });

            // SystemConfiguration tablo kuralları
            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                // DİKKAT: HasData bloğu DatabaseSeeder'a taşındı.
            });
        }
    }
}