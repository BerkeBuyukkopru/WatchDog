using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace HealthChecks.MongoDb;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoDbHealthCheck(this IServiceCollection services, string connectionString)
    {
        // 1. MongoClient'ı SADECE BİR KERE yaratıyoruz (Bağlantı Havuzu - Connection Pool burada oluşur)
        // Lazy bir şekilde veya doğrudan Singleton olarak kaydedebiliriz.
        var mongoClient = new MongoClient(connectionString);

        // 2. Modülümüzü üretirken, her seferinde bu hazır ve tek olan MongoClient'ı veriyoruz
        services.AddTransient<IHealthCheck>(serviceProvider =>
        {
            return new MongoDbHealthCheck(mongoClient);
        });

        return services;
    }
}