using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Watchdog.Api;
using Watchdog.Application;
using Watchdog.Infrastructure;
using Watchdog.Infrastructure.Persistence;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Api.Services; 

var builder = WebApplication.CreateBuilder(args);

// === 1. Standart API Servisleri ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// === Kimlik Tespit Servisleri (Audit & Soft Delete İçin) ===
builder.Services.AddHttpContextAccessor(); // HttpContext'e erişim sağlar
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); // Kimliği bilet üzerinden okur

// CORS POLİTİKASI (React Bağlantısı İçin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // React/Vite portları
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR için şart
    });
});

// --- Swagger Konfigürasyonu ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WatchDog API", Version = "v1" });

    // Güvenlik Tanımı
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Kopyaladığınız Token'ı direkt buraya yapıştırın. (Başına Bearer yazmanıza gerek yok!)",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// === 2. AUTHENTICATION & AUTHORIZATION ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "Fallback_Secure_Key_32_Characters_Long");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };

    // SignalR İçin Token Okuma Kuralı
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // İstek statushub'a geliyorsa URL'deki token'ı al
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/statushub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// === 3. Katman Kayıtları ===
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// === 4. Real-time İletişim (SignalR) ===
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10485760;
    options.EnableDetailedErrors = true;
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.PayloadSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();
app.UseMiddleware<Watchdog.Api.Middlewares.ExceptionMiddleware>();

// === 5. Middleware Pipeline ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WatchDog API v1");
    });
}

// app.UseHttpsRedirection();

// Routing'den sonra, Auth'tan önce CORS çalışmalı
app.UseRouting();
app.UseCors("ReactAppPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Watchdog.Api.Hubs.StatusHub>("/statushub");

// Başlangıçta Data Seeder'ı Çalıştır
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();