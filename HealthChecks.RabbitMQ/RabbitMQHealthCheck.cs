using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using RabbitMQ.Client; // Kurye şirketinin özel maymuncuğu
using System.Diagnostics;

namespace HealthChecks.RabbitMQ;

public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    // Kural 1: Yakasında Kurye Denetmeni yazar
    public string Name => "RabbitMQ Monitor";

    public RabbitMQHealthCheck(string connectionString)
    {
        _connectionString = connectionString; // Kurye odasının adresi
    }

    // Kural 2: Denetim Başlıyor
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            // Kurye fabrikasına (ConnectionFactory) adresi veriyoruz
            var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };

            // Kapıyı açmayı (CreateConnection) deniyoruz
            using var connection = factory.CreateConnection();

            // Eğer kod buraya ulaştıysa ve hata vermediyse kapı açıktır!
            watch.Stop();
            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Duration = watch.Elapsed,
                Description = "RabbitMQ kurye hattı sorunsuz çalışıyor."
            };
        }
        catch (Exception ex)
        {
            // Kapı kilitliyse veya kuryeler grevdeyse (çöktüyse) buraya düşer
            watch.Stop();
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"RabbitMQ Hatası: {ex.Message}",
                Duration = watch.Elapsed,
                Exception = ex
            };
        }
    }
}