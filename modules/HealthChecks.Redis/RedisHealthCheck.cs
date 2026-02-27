using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using StackExchange.Redis;
using System.Diagnostics;

namespace HealthChecks.Redis;

public class RedisHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    // Kural 1: Sözleşmenin istediği 'Name' (İsim) özelliği
    public string Name => "Redis Monitor";

    public RedisHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Kural 2: Sözleşmenin istediği 'CheckHealthAsync' metodu
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            using var redisConnection = await ConnectionMultiplexer.ConnectAsync(_connectionString);
            var db = redisConnection.GetDatabase();

            await db.PingAsync();

            watch.Stop();
            return new HealthCheckResult { Status = HealthStatus.Healthy, Duration = watch.Elapsed, Description = "Redis ayakta." };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new HealthCheckResult { Status = HealthStatus.Unhealthy, Description = $"Redis Hatası: {ex.Message}", Duration = watch.Elapsed, Exception = ex };
        }
    }
}