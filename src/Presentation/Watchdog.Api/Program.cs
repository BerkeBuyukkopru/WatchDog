using HealthChecks.System;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.Interfaces;
using Watchdog.Infrastructure.Persistence;
using HealthChecks.System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

<<<<<<< HEAD
// === 1. SWAGGER ARAYÜZÜ İÇİN GEREKLİ SERVİSLER ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// === 1. SWAGGER ARAYÜZÜ İÇİN GEREKLİ SERVİSLER ===

// Scoped: Her bir HTTP isteği için yeni bir örnek oluşturur.
builder.Services.AddScoped<IAppService, Watchdog.Application.Services.AppService>();
builder.Services.AddScoped<IMonitoredAppRepository, Watchdog.Infrastructure.Persistence.Repositories.MonitoredAppRepository>();

// SQL Server Bağlantısı (ConnectionString appsettings'ten geliyor).
=======
// SENSÖRLERİ SİSTEME DAHİL EDİYORUZ (UC-3 Entegrasyonu)
builder.Services.AddSystemHealthChecks(
    serverCpuThreshold: 90.0,
    appCpuThreshold: 90.0,
    minServerAvailableMb: 512f,
    maxAppAllocatedMb: 1024f,
    minFreeSpaceGb: 5f
);

>>>>>>> furkan
builder.Services.AddDbContext<WatchdogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


<<<<<<< HEAD
builder.Services.AddOpenApi();
builder.Services.AddSystemHealthChecks();
=======

>>>>>>> furkan

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // "/swagger" adresini tarayıcıda aktif eder.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers(); // Controller'ları rotaya bağlar.

app.Run();