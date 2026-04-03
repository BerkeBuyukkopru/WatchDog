using Microsoft.Extensions.DependencyInjection;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces;
using Watchdog.Application.Services;
using Watchdog.Application.UseCases;
using Watchdog.Domain.Entities;

namespace Watchdog.Application;

// BU SINIFIN BAŞKA BİR SINIFIN İÇİNDE OLMADIĞINDAN EMİN OL!
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // === Servis Kayıtları ===
        services.AddScoped<IAppService, AppService>();
        services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();

        // === Use Case Kayıtları ===
        services.AddScoped<IUseCaseAsync<CreateMonitoredAppRequest, CreateMonitoredAppResponse>, CreateMonitoredAppUseCase>();
        services.AddScoped<IUseCaseAsync<UpdateAppEmailsRequest, (bool IsSuccess, string ErrorMessage)>, UpdateAppEmailsUseCase>();
        services.AddScoped<IUseCaseAsync<HealthSnapshot>, AnalyzeSystemHealthUseCase>();
        services.AddScoped<IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?>, PollSingleAppUseCase>();
        services.AddScoped<IUseCaseAsync<GetLatestStatusesRequest, IEnumerable<LatestStatusDto>>, GetLatestStatusesUseCase>();

        return services;
    }
}