using HealthChecks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.SqlServer;

public static class DependencyInjection
{
    // Bu metod (extension) ana sistemin IServiceCollection yapısına eklenir
    public static IServiceCollection AddSqlServerHealthCheck(this IServiceCollection services, string connectionString)
    {
        // Ne zaman biri benden IHealthCheck isterse, ona bu SQL uzmanını ver diyoruz
        services.AddTransient<IHealthCheck>(provider => new SqlServerHealthCheck(connectionString));
        return services;
    }
}