using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.MongoDb;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoDbHealthCheck(this IServiceCollection services, string connectionString)
    {
        services.AddTransient<IHealthCheck>(serviceProvider =>
        {
            return new MongoDbHealthCheck(connectionString);
        });

        return services;
    }
}
