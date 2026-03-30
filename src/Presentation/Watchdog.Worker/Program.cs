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

// 3. 7/24 Çalışacak Motorumuzu (Worker) Sisteme Kaydediyoruz
builder.Services.AddHostedService<Worker>();

// 4. Sistemi İnşa Et ve Çalıştır
var host = builder.Build();
host.Run();