using HealthChecks.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Infrastructure.AiServices;
using Watchdog.Infrastructure.Auth;
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
            // Not: DbContext artık constructor'da ICurrentUserService bekliyor. 
            // Bu servis Program.cs içinde kaydedildiği için burada otomatik çözülecektir.
            services.AddDbContext<WatchdogDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // === 2. Repository Kayıtları ===
            services.AddScoped<IMonitoredAppRepository, MonitoredAppRepository>();
            services.AddScoped<ISnapshotRepository, SnapshotRepository>();
            services.AddScoped<IIncidentRepository, IncidentRepository>();
            services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();

            // Yapay Zeka Tavsiye Deposu Kaydı
            services.AddScoped<IAiInsightRepository, AiInsightRepository>();

            // === 3. Bildirim (Mail) Servisleri ===
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddScoped<INotificationSender, MailSender>();
            services.AddHttpClient<IHealthProbeClient, HealthProbeHttpClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });

            // Canlı yayın servisini tekilleştirilmiş (Singleton) olarak ekliyoruz
            services.AddSingleton<IStatusBroadcaster, SignalRStatusBroadcaster>();

            // === 4. Sistem Sensörleri ===
            services.AddSystemHealthChecks(
                serverCpuThreshold: 90.0,
                appCpuThreshold: 90.0,
                minServerAvailableMb: 512f,
                maxAppAllocatedMb: 1024f,
                minFreeSpaceGb: 5f
            );

            // === Yapay Zeka (AI) Servisleri ===
            // Fabrikamızı kaydediyoruz. UseCase IAiClientFactory istediğinde AiClientFactory verilecek.
            services.AddScoped<IAiClientFactory, AiClientFactory>();
            services.AddScoped<IAiProviderRepository, AiProviderRepository>();

            // Auth Servisleri ve Repository
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<DatabaseSeeder>();
            services.AddScoped<IAuthRepository, AuthRepository>();

            return services;
        }
    }
}