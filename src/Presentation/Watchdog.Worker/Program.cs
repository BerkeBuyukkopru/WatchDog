using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Watchdog.Infrastructure.Persistence;
using Watchdog.Worker;

// 1. Usta İnşaatçıyı (Builder) Başlat
var builder = Host.CreateApplicationBuilder(args);

// 2. EKSİK OLAN PARÇA: Resepsiyoniste Veritabanını (DbContext) Tanıtıyoruz!
builder.Services.AddDbContext<WatchdogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. 7/24 Çalışacak Motorumuzu (Worker) Sisteme Kaydediyoruz
builder.Services.AddHostedService<Worker>();

// 4. Sistemi İnşa Et ve Çalıştır
var host = builder.Build();
host.Run();