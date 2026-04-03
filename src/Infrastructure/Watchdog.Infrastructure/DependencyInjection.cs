using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces;
using Watchdog.Infrastructure.Notifications;
using Watchdog.Infrastructure.Persistence;
using Watchdog.Infrastructure.Persistence.Repositories;
using Watchdog.Infrastructure.Probing;
using Microsoft.EntityFrameworkCore;
using HealthChecks.System;

namespace Watchdog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // === 1. Veritabanı (DbContext) Kaydı ===
            services.AddDbContext<WatchdogDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // === 2. Repository Kayıtları ===
            services.AddScoped<IMonitoredAppRepository, MonitoredAppRepository>();
            services.AddScoped<ISnapshotRepository, SnapshotRepository>();
            services.AddScoped<IIncidentRepository, IncidentRepository>();
            services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();

            // === 3. Bildirim (Mail) Servisleri ===
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<INotificationSender, MailSender>();
            services.AddHttpClient<IHealthProbeClient, HealthProbeHttpClient>();

            // 4. Sistem Sensörleri (API'den buraya taşındı - Encapsulation)
            services.AddSystemHealthChecks(
                serverCpuThreshold: 90.0,
                appCpuThreshold: 90.0,
                minServerAvailableMb: 512f,
                maxAppAllocatedMb: 1024f,
                minFreeSpaceGb: 5f
            );

            return services;
        }
    }
}
