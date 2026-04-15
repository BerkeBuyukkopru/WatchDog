using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.DTOs.Auth;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.DTOs.SystemConfig;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.UseCases.AI;
using Watchdog.Application.UseCases.Apps;
using Watchdog.Application.UseCases.Auth;
using Watchdog.Application.UseCases.HealthMonitoring;
using Watchdog.Application.UseCases.SystemConfig;
using Watchdog.Domain.Entities;

namespace Watchdog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // === Use Case Kayıtları ===
        services.AddScoped<IUseCaseAsync<GetSystemConfigRequest, SystemConfigDto?>, GetSystemConfigUseCase>();
        services.AddScoped<IUseCaseAsync<SystemConfigDto, bool>, UpdateSystemConfigUseCase>();

        // === apay Zeka Sağlayıcı Yönetimi Use Case'leri ===
        services.AddScoped<IUseCaseAsync<GetAllAiProvidersRequest, IEnumerable<AiProviderDto>>, GetAllAiProvidersUseCase>();
        services.AddScoped<SetActiveAiProviderUseCase>(); 
        services.AddScoped<UpdateAiProviderUseCase>();    

        services.AddScoped<IUseCaseAsync<GetAllAppsRequest, IEnumerable<AppDto>>, GetAllAppsUseCase>();
        services.AddScoped<IUseCaseAsync<DeleteAppRequest, bool>, DeleteAppUseCase>();

        services.AddScoped<IUseCaseAsync<CreateMonitoredAppRequest, CreateMonitoredAppResponse>, CreateMonitoredAppUseCase>();
        services.AddScoped<IUseCaseAsync<UpdateAppEmailsRequest, (bool IsSuccess, string ErrorMessage)>, UpdateAppEmailsUseCase>();

        services.AddTransient<IUseCaseAsync<HealthSnapshot>, AnalyzeSystemHealthUseCase>();
        services.AddScoped<IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?>, PollSingleAppUseCase>();
        services.AddScoped<IUseCaseAsync<GetLatestStatusesRequest, IEnumerable<LatestStatusDto>>, GetLatestStatusesUseCase>();
        services.AddScoped<PollAllAppsUseCase>();

        // === Yapay Zeka Use Case'leri ===
        services.AddScoped<GenerateRoutineInsightUseCase>();
        services.AddScoped<GetAiInsightsUseCase>();
        services.AddScoped<GenerateStrategicInsightUseCase>();
        services.AddScoped<IPromptBuilder, PromptBuilder>();

        // Okundu/Çözüldü İşaretleme Senaryosu 
        services.AddScoped<IUseCaseAsync<Guid, bool>, ResolveInsightUseCase>();
        services.AddScoped<ArchiveSnapshotsUseCase>();

        // Auth Use Case
        services.AddScoped<IUseCaseAsync<LoginRequest, LoginResponse>, LoginUseCase>();
        services.AddScoped<IUseCaseAsync<RegisterRequest, RegisterResponse>, RegisterUseCase>();
        services.AddScoped<IUseCaseAsync<Guid, bool>, DeleteAdminUseCase>();
        services.AddScoped<IUseCaseAsync<UpdateAdminRequest, bool>, UpdateAdminUseCase>();
        services.AddScoped<IUseCaseAsync<Guid, bool>, RestoreAdminUseCase>();

        return services;
    }
}