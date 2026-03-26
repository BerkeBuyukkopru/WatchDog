using HealthChecks.System;
using Microsoft.EntityFrameworkCore;
using Watchdog.Infrastructure.Persistence;
using HealthChecks.System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// SENSÖRLERİ SİSTEME DAHİL EDİYORUZ (UC-3 Entegrasyonu)
builder.Services.AddSystemHealthChecks(
    serverCpuThreshold: 90.0,
    appCpuThreshold: 90.0,
    minServerAvailableMb: 512f,
    maxAppAllocatedMb: 1024f,
    minFreeSpaceGb: 5f
);

builder.Services.AddDbContext<WatchdogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddOpenApi();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();