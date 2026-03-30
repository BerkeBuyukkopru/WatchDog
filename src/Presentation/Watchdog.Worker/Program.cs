using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Watchdog.Infrastructure.Persistence;
using Watchdog.Worker;

// 1. Usta İnşaatçıyı (Builder) Başlat. Worker Service Builder'ı başlatıyoruz. WebApplication.CreateBuilder'dan farkı; içinde Controller veya Swagger gibi ağır web yükleri taşımaz, sadece saf performans odaklı bir "Host" oluşturur.
var builder = Host.CreateApplicationBuilder(args);

// 2. EKSİK OLAN PARÇA: Resepsiyoniste Veritabanını (DbContext) Tanıtıyoruz! API projesiyle aynı veritabanına bağlanıyoruz. Üretilen sağlık Snapshot'larını bu kanal üzerinden kaydedeceğiz.
builder.Services.AddDbContext<WatchdogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// (UC-5 Kural Motoru ve Bildirimler)
builder.Services.AddScoped<Watchdog.Application.Interfaces.ISnapshotRepository, Watchdog.Infrastructure.Persistence.Repositories.SnapshotRepository>();
builder.Services.AddScoped<Watchdog.Application.Interfaces.IIncidentRepository, Watchdog.Infrastructure.Persistence.Repositories.IncidentRepository>();
builder.Services.AddScoped<Watchdog.Application.Interfaces.INotificationSender, Watchdog.Infrastructure.Notifications.MailSender>();
builder.Services.AddScoped<Watchdog.Application.Interfaces.IMonitoredAppRepository, Watchdog.Infrastructure.Persistence.Repositories.MonitoredAppRepository>();
builder.Services.AddScoped<Watchdog.Application.UseCases.AnalyzeSystemHealthUseCase>();

// 3. 7/24 Çalışacak Motorumuzu (Worker) Sisteme Kaydediyoruz
builder.Services.AddHostedService<Worker>();

// 4. Sistemi İnşa Et ve Çalıştır
var host = builder.Build();
host.Run();