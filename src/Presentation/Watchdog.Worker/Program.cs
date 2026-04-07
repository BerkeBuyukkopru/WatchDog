using Watchdog.Application;
using Watchdog.Infrastructure;
using Watchdog.Worker;
using Watchdog.Worker.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

// === 1. Katman Kayıtları (Yeni Mimari) ===
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// === 2. Worker Servis Kaydı ===
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<AiAnalyzerWorker>(); // Yapay Zeka İşçisi

// AIOps Stratejik Tahmin İşçisi (Gece çalışan karşılaştırmalı analiz motoru)
builder.Services.AddHostedService<StrategicAnalyzerWorker>();

var host = builder.Build();
host.Run();