using Watchdog.Application;
using Watchdog.Infrastructure;
using Watchdog.Worker.BackgroundServices;
using Watchdog.Application.Interfaces.Common;           

var builder = Host.CreateApplicationBuilder(args);

// === 1. Katman Kayıtları (Yeni Mimari) ===
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// === KRİTİK: Worker Kimlik Kaydı ===
// DbContext'in istediği ICurrentUserService'i burada "WorkerService" olarak çözüyoruz.
builder.Services.AddSingleton<ICurrentUserService, WorkerCurrentUserService>();

// === 2. Worker Servis Kayıtları ===
builder.Services.AddHostedService<HealthPollingWorker>();             // Temel Sağlık Taraması (Health Polling)
builder.Services.AddHostedService<AiAnalyzerWorker>();   // Geliştirici 1: Yapay Zeka Kapasite İşçisi

builder.Services.AddHostedService<DataArchiverWorker>(); // Geliştirici 2: Sıcak/Soğuk Veri Arşiv İşçisi (UC-9)

// AIOps Stratejik Tahmin İşçisi (Gece çalışan karşılaştırmalı analiz motoru)
builder.Services.AddHostedService<StrategicAnalyzerWorker>();

var host = builder.Build();
host.Run();