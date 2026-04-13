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
builder.Services.AddSignalR(options =>
{
    // Gelen mesaj limitini 32 KB'den 10 MB'a çıkartıyoruz. (1024 * 1024 * 10)
    options.MaximumReceiveMessageSize = 10485760;
    options.EnableDetailedErrors = true; // SignalR detaylı hatalarını açıyoruz
})
    .AddJsonProtocol(options =>
    {
        // Döngüsel referansları görmezden gelmeye devam et
        options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

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