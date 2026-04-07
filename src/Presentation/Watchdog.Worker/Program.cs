using Watchdog.Application;
using Watchdog.Infrastructure;
using Watchdog.Worker;
using Watchdog.Worker.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

// === 1. Katman Kayıtları (Yeni Mimari) ===
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// === 2. Worker Servis Kayıtları ===
builder.Services.AddHostedService<Worker>();             // Temel Sağlık Taraması (Health Polling)
builder.Services.AddHostedService<AiAnalyzerWorker>();   // Geliştirici 1: Yapay Zeka Kapasite İşçisi

// --- YENİ EKLENEN (GELİŞTİRİCİ 2) ---
builder.Services.AddHostedService<DataArchiverWorker>(); // Geliştirici 2: Sıcak/Soğuk Veri Arşiv İşçisi (UC-9)



var host = builder.Build();
host.Run();