using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.MongoDb
{
    public class MongoDbHealthCheck : IHealthCheck
    {
        // MongoClient artık her ping'de yeniden yaratılmayacak, dışarıdan hazır gelecek
        private readonly IMongoClient _mongoClient;

        public string Name => "MongoDb_Check";

        // Constructor: Sadece connectionString yerine, bağlantı havuzunu yöneten Client'ı alıyoruz
        public MongoDbHealthCheck(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Varsayılan olarak admin veritabanına bağlanırız.
                var database = _mongoClient.GetDatabase("admin");

                // Ping komutunu hazırlıyoruz.
                var pingCommand = new BsonDocument("ping", 1);

                // Komutu sunucuya fırlatıyoruz.
                await database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);

                stopwatch.Stop();

                // AI için güvenli metrik verisi oluşturuyoruz (Şifre vb. sızdırmadan)
                var telemetryData = new Dictionary<string, object>
                {
                    { "ClusterStatus", "Connected" },
                    { "TargetDatabase", "admin" }
                };

                var result = HealthCheckResult.Healthy("MongoDB bağlantısı başarılı ve ping yanıtı alındı.", data: telemetryData);
                result.Duration = stopwatch.Elapsed;
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var unhealthyResult = HealthCheckResult.Unhealthy($"MongoDB bağlantı hatası: {ex.Message}", ex);
                unhealthyResult.Duration = stopwatch.Elapsed;
                return unhealthyResult;
            }
        }
    }
}