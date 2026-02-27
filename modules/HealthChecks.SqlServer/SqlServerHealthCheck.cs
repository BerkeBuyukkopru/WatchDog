using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace HealthChecks.SqlServer;

public class SqlServerHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    // Kural 1: Sözleşmenin istediği 'Name' (İsim) özelliği
    public string Name => "SQL Server Monitor";

    public SqlServerHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Kural 2: Sözleşmenin istediği 'CheckHealthAsync' metodu
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            watch.Stop();
            return new HealthCheckResult { Status = HealthStatus.Healthy, Duration = watch.Elapsed, Description = "SQL Server ayakta." };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new HealthCheckResult { Status = HealthStatus.Unhealthy, Description = $"SQL Hatası: {ex.Message}", Duration = watch.Elapsed, Exception = ex };
        }
    }
}