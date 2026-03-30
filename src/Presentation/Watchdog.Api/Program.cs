using HealthChecks.System;
using Microsoft.EntityFrameworkCore;
using Watchdog.Application.Interfaces;
using Watchdog.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// === 1. SWAGGER ARAYÜZÜ İÇİN GEREKLİ SERVİSLER ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// --- Servis Kayıtları (Dependency Injection) ---

// Uygulama ve Repository Servisleri
builder.Services.AddScoped<IAppService, Watchdog.Application.Services.AppService>();
builder.Services.AddScoped<IMonitoredAppRepository, Watchdog.Infrastructure.Persistence.Repositories.MonitoredAppRepository>();

// Sistem Konfigürasyon Servisleri (Gelen Değişiklikler)
builder.Services.AddScoped<ISystemConfigurationRepository, Watchdog.Infrastructure.Persistence.Repositories.SystemConfigurationRepository>();
builder.Services.AddScoped<ISystemConfigurationService, Watchdog.Application.Services.SystemConfigurationService>();

// SENSÖRLERİ SİSTEME DAHİL EDİYORUZ (UC-3 Entegrasyonu - Senin Yazdığın)
builder.Services.AddSystemHealthChecks(
    serverCpuThreshold: 90.0,
    appCpuThreshold: 90.0,
    minServerAvailableMb: 512f,
    maxAppAllocatedMb: 1024f,
    minFreeSpaceGb: 5f
);

// SQL Server Bağlantısı
builder.Services.AddDbContext<WatchdogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ⚡ === ADIM 2.1: SIGNALR YETENEĞİNİ SİSTEME EKLİYORUZ ===
builder.Services.AddSignalR();

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

// 🚪 === ADIM 2.2: CANLI YAYIN ODASININ KAPISINI DIŞ DÜNYAYA AÇIYORUZ ===
app.MapHub<Watchdog.Api.Hubs.StatusHub>("/statushub");

app.Run();