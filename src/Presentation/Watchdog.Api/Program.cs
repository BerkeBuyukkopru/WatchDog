using HealthChecks.System;
using Watchdog.Api;
using Watchdog.Application;
using Watchdog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// === 1. Standart API Servisleri ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// === 2. Katman Kayıtları (Yeni Mimari) ===
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// === 3. Real-time İletişim ===
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// SignalR Hub Kaydı
app.MapHub<Watchdog.Api.Hubs.StatusHub>("/statushub");

app.Run();