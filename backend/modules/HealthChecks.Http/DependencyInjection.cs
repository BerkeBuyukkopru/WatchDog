using System;
using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Http;

public static class DependencyInjection
{
    // string url yerine Func<string> urlProvider alıyoruz
    public static IServiceCollection AddHttpHealthCheck(this IServiceCollection services, Func<string> urlProvider)
    {
        services.AddHttpClient();

        services.AddTransient<IHealthCheck>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient();

            // urlProvider'ı içeri aktarıyoruz
            return new HttpHealthCheck(client, urlProvider);
        });

        return services;
    }
}