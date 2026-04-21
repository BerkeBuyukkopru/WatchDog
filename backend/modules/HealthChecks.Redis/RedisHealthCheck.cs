using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using StackExchange.Redis;
using System.Diagnostics;

namespace HealthChecks.Redis;

public class RedisHealthCheck : IHealthCheck
{
    // Bağlantı dizesi yerine doğrudan multiplexer'ı alıyoruz
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public string Name => "Redis Monitor";

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            // Yeni bağlantı açmıyoruz, var olan singleton bağlantı üzerinden DB'yi alıyoruz
            var db = _connectionMultiplexer.GetDatabase();

            await db.PingAsync();

            watch.Stop();
            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Duration = watch.Elapsed,
                Description = "Redis ayakta."
            };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Redis Hatası: {ex.Message}",
                Duration = watch.Elapsed,
                Exception = ex
            };
        }
    }
}