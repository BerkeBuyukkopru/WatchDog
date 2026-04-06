using HealthChecks.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Infrastructure.AiServices;
using Watchdog.Infrastructure.Notifications;
using Watchdog.Infrastructure.Persistence;
using Watchdog.Infrastructure.Persistence.Repositories;
using Watchdog.Infrastructure.Probing;

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

            // YENİ: Yapay Zeka Tavsiye Deposu Kaydı
            services.AddScoped<IAiInsightRepository, AiInsightRepository>();

            // === 3. Bildirim (Mail) Servisleri ===
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<INotificationSender, MailSender>();
            services.AddHttpClient<IHealthProbeClient, HealthProbeHttpClient>();

            // === 4. Sistem Sensörleri ===
            services.AddSystemHealthChecks(
                serverCpuThreshold: 90.0,
                appCpuThreshold: 90.0,
                minServerAvailableMb: 512f,
                maxAppAllocatedMb: 1024f,
                minFreeSpaceGb: 5f
            );

            // === 5. YENİ: Yapay Zeka (AI) Servisleri ===
            // Fabrikamızı kaydediyoruz. UseCase IAiClientFactory istediğinde AiClientFactory verilecek.
            services.AddScoped<IAiClientFactory, AiClientFactory>();

            return services;
        }
    }
}
