using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Watchdog.Infrastructure.Persistence;
using Watchdog.Worker;

// 1. Usta İnşaatçıyı (Builder) Başlat.
var builder = Host.CreateApplicationBuilder(args);

// 2. Resepsiyoniste Veritabanını (DbContext) Tanıtıyoruz!
builder.Services.AddDbContext<WatchdogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// (UC-5 Kural Motoru ve Bildirimler)
builder.Services.AddScoped<Watchdog.Application.Interfaces.ISnapshotRepository, Watchdog.Infrastructure.Persistence.Repositories.SnapshotRepository>();
builder.Services.AddScoped<Watchdog.Application.Interfaces.IIncidentRepository, Watchdog.Infrastructure.Persistence.Repositories.IncidentRepository>();
builder.Services.AddScoped<Watchdog.Application.Interfaces.INotificationSender, Watchdog.Infrastructure.Notifications.MailSender>();
builder.Services.AddScoped<Watchdog.Application.Interfaces.IMonitoredAppRepository, Watchdog.Infrastructure.Persistence.Repositories.MonitoredAppRepository>();

// Use Case Kayıtları
builder.Services.AddScoped<Watchdog.Application.Interfaces.IUseCaseAsync<Watchdog.Domain.Entities.HealthSnapshot>, Watchdog.Application.UseCases.AnalyzeSystemHealthUseCase>();

// YENİ EKLENEN KAYITLAR: Ping Elçisi ve Orkestra Şefi
builder.Services.AddHttpClient<Watchdog.Application.Interfaces.IHealthProbeClient, Watchdog.Infrastructure.Probing.HealthProbeHttpClient>();
builder.Services.AddScoped<Watchdog.Application.Interfaces.IUseCaseAsync<Watchdog.Application.DTOs.PollSingleAppRequest, Watchdog.Domain.Entities.HealthSnapshot?>, Watchdog.Application.UseCases.PollSingleAppUseCase>();

// 3. 7/24 Çalışacak Motorumuzu (Worker) Sisteme Kaydediyoruz
builder.Services.AddHostedService<Worker>();

// 4. Sistemi İnşa Et ve Çalıştır
var host = builder.Build();
host.Run();