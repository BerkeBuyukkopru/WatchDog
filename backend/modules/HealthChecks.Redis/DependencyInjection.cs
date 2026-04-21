using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace HealthChecks.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddRedisHealthCheck(this IServiceCollection services, string connectionString)
    {
        // Redis bağlantısını uygulamanın geneli için Singleton olarak kaydediyoruz (eğer daha önce kaydedilmediyse)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connectionString));

        // Health check sınıfımızı kaydediyoruz. IConnectionMultiplexer DI konteynerinden otomatik çözülecektir.
        services.AddTransient<IHealthCheck, RedisHealthCheck>();

        return services;
    }
}