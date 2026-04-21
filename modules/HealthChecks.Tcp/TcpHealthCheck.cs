using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Tcp
{
    public class TcpHealthCheck : IHealthCheck
    {
        private readonly Func<string> _hostProvider;
        private readonly Func<int> _portProvider;
        private readonly string? _payload;
        private readonly string? _response;

        public string Name => "Tcp_Client_Check";

        public TcpHealthCheck(Func<string> hostProvider, Func<int> portProvider, string? payload = null, string? response = null)
        {
            _hostProvider = hostProvider;
            _portProvider = portProvider;
            _payload = payload;
            _response = response;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            // O anki güncel hedef IP ve Port'u okuyoruz
            string currentHost = _hostProvider();
            int currentPort = _portProvider();

            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(currentHost, currentPort, cancellationToken);

                if (string.IsNullOrEmpty(_payload))
                {
                    var result = HealthCheckResult.Healthy($"TCP bağlantısı başarılı. Hedef: {currentHost}:{currentPort}");
                    result.Duration = stopwatch.Elapsed;
                    return result;
                }

                using var stream = client.GetStream();
                byte[] dataToSend = Encoding.UTF8.GetBytes(_payload);

                await stream.WriteAsync(dataToSend.AsMemory(), cancellationToken);

                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
                string responseText = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (!string.IsNullOrEmpty(_response) &&
                    responseText.IndexOf(_response, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    var failResult = HealthCheckResult.Unhealthy($"Bağlantı başarılı ama beklenen cevap alınamadı. Gelen: {responseText}");
                    failResult.Duration = stopwatch.Elapsed;
                    return failResult;
                }

                var successResult = HealthCheckResult.Healthy($"TCP Client testi başarılı. Gelen Cevap: {responseText}");
                successResult.Duration = stopwatch.Elapsed;
                return successResult;
            }
            catch (Exception ex)
            {
                var unhealthyResult = HealthCheckResult.Unhealthy($"TCP hatası ({currentHost}:{currentPort}): {ex.Message}", ex);
                unhealthyResult.Duration = stopwatch.Elapsed;
                return unhealthyResult;
            }
            finally
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
        }
    }
}