using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace HealthChecks.SqlServer;

public class SqlServerHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public string Name => "SQL Server Monitor";

    public SqlServerHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";

            // Sağlık kontrolü sorgusunun asılı kalmasını önlemek için 5 saniyelik limit (opsiyonel)
            command.CommandTimeout = 5;

            await command.ExecuteScalarAsync(cancellationToken);

            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Duration = watch.Elapsed,
                Description = "SQL Server ayakta."
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"SQL Hatası: {ex.Message}",
                Duration = watch.Elapsed,
                Exception = ex
            };
        }
        finally
        {
            // İşlem başarılı da olsa hata da verse süreyi her halükarda durduruyoruz.
            if (watch.IsRunning)
            {
                watch.Stop();
            }
        }
    }
}