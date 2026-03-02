using HealthChecks.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HealthChecks.MongoDb
{
    public class MongoDbHealthCheck: IHealthCheck
    {
        private readonly string _connectionString;

        public string Name => "MongoDb_Check";

        public MongoDbHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                //MongoDb yi bağlıyoruz.
                var mongoClient = new MongoClient(_connectionString);

                //varsayılan olarak admin veritabanına bağlanırız.
                var database = mongoClient.GetDatabase("admin");

                //ping komutunu hazırlıyoruz.
                var pingCommand = new BsonDocument ("ping", 1);

                //Komutu sunucuya fırlatıyoruz.
                await database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);

                stopwatch.Stop();

                var result = HealthCheckResult.Healthy("MongoDB bağlantısı başarılı ve ping yanıtı alındı.");
                result.Duration = stopwatch.Elapsed;
                return result;
            }
            catch (Exception ex) {
                stopwatch.Stop();
                var unhealthyResult = HealthCheckResult.Unhealthy($"MongoDB bağlantı hatası: {ex.Message}", ex);
                unhealthyResult.Duration = stopwatch.Elapsed;
                return unhealthyResult;
            }
        }
    }
}
