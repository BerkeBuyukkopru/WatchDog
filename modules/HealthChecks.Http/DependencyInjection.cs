using System;
using System.Collections.Generic;
using System.Text;
using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Http;

public static class DependencyInjection
{
    public static IServiceCollection AddHttpHealthCheck(this IServiceCollection services, string url)
    {
        // 1. HttpClient fabrikasını sisteme dahil et
        services.AddHttpClient();

        // 2. Kendi modülümüzü nasıl üreteceğini sisteme öğret
        services.AddTransient<IHealthCheck>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient();

            return new HttpHealthCheck(client, url);
        });

        // 3. Zincirleme kullanım için servis koleksiyonunu geri dön
        return services;
    }
}