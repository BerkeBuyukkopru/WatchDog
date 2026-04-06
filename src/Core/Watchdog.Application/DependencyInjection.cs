using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.DTOs.SystemConfig;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.UseCases.AI;
using Watchdog.Application.UseCases.Apps;
using Watchdog.Application.UseCases.HealthMonitoring;
using Watchdog.Application.UseCases.SystemConfig;
using Watchdog.Domain.Entities;

namespace Watchdog.Application;

// BU SINIFIN BAŞKA BİR SINIFIN İÇİNDE OLMADIĞINDAN EMİN OL!
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // === Use Case Kayıtları ===

        services.AddScoped<IUseCaseAsync<GetSystemConfigRequest, SystemConfigDto?>, GetSystemConfigUseCase>();
        services.AddScoped<IUseCaseAsync<SystemConfigDto, bool>, UpdateSystemConfigUseCase>();

        services.AddScoped<IUseCaseAsync<GetAllAppsRequest, IEnumerable<AppDto>>, GetAllAppsUseCase>();
        services.AddScoped<IUseCaseAsync<DeleteAppRequest, bool>, DeleteAppUseCase>();

        services.AddScoped<IUseCaseAsync<CreateMonitoredAppRequest, CreateMonitoredAppResponse>, CreateMonitoredAppUseCase>();
        services.AddScoped<IUseCaseAsync<UpdateAppEmailsRequest, (bool IsSuccess, string ErrorMessage)>, UpdateAppEmailsUseCase>();
        services.AddScoped<IUseCaseAsync<HealthSnapshot>, AnalyzeSystemHealthUseCase>();
        services.AddScoped<IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?>, PollSingleAppUseCase>();
        services.AddScoped<IUseCaseAsync<GetLatestStatusesRequest, IEnumerable<LatestStatusDto>>, GetLatestStatusesUseCase>();

        // === YENİ: Yapay Zeka Use Case'i ===
        // Worker doğrudan sınıfı talep ettiği için doğrudan (concrete) sınıf olarak kaydediyoruz.
        services.AddScoped<GenerateRoutineInsightUseCase>();
        services.AddScoped<GetAiInsightsUseCase>();

        return services;
    }
}