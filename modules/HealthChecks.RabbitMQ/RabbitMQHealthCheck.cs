using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using RabbitMQ.Client; // Kurye şirketinin özel maymuncuğu
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.RabbitMQ;

public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly ConnectionFactory _factory;

    // Kural 1: Yakasında Kurye Denetmeni yazar
    public string Name => "RabbitMQ Monitor";

    public RabbitMQHealthCheck(string connectionString)
    {
        // Kurye fabrikasına (ConnectionFactory) adresi bir kere veriyoruz (Performans)
        _factory = new ConnectionFactory { Uri = new Uri(connectionString) };
    }

    // Kural 2: Denetim Başlıyor
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            // Kapıyı açmayı (CreateConnection) BEKLEYEREK deniyoruz (await eklendi)
            using var connection = await _factory.CreateConnectionAsync(cancellationToken);

            // GEREKSİNİM WDG020: Sadece kapı yetmez, dağıtım bandı (Channel) da açık mı?
            using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            watch.Stop();

            // AI Motorumuz için telemetri verisi (Zero-Trust için şifre içermeyen bilgiler)
            var telemetryData = new Dictionary<string, object>
            {
                { "HostName", _factory.HostName },
                { "VirtualHost", _factory.VirtualHost },
                { "ChannelIsOpen", channel.IsOpen }
            };

            // Her şey sağlamsa kendi statik metodumuzla dönüyoruz
            var result = HealthCheckResult.Healthy("RabbitMQ bağlantısı (Connection) ve kanalı (Channel) sorunsuz çalışıyor.", data: telemetryData);
            result.Duration = watch.Elapsed;
            return result;
        }
        catch (Exception ex)
        {
            // Kapı kilitliyse veya kuryeler grevdeyse (çöktüyse) buraya düşer
            watch.Stop();
            var unhealthyResult = HealthCheckResult.Unhealthy($"RabbitMQ Hatası: {ex.Message}", ex);
            unhealthyResult.Duration = watch.Elapsed;
            return unhealthyResult;
        }
    }
}