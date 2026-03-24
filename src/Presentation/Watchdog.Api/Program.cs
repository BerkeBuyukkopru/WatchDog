using HealthChecks.System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// WatchDog Sistem (CPU, RAM, Storage) Sağlık Kontrollerini Enjekte Ediyoruz
builder.Services.AddSystemHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();