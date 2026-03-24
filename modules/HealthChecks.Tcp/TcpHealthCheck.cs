using HealthChecks.Abstractions;
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
        private readonly string _host;
        private readonly int _port;
        private readonly string? _payload;
        private readonly string? _response;

        public string Name => "Tcp_Client_Check";

        public TcpHealthCheck(string host, int port, string? payload = null, string? response = null)
        {
            _host = host;
            _port = port;
            _payload = payload;
            _response = response;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var client = new TcpClient();

                // 1. İyileştirme: Bağlantı aşamasına iptal jetonunu ekliyoruz
                await client.ConnectAsync(_host, _port, cancellationToken);

                if (string.IsNullOrEmpty(_payload))
                {
                    var result = HealthCheckResult.Healthy($"TCP bağlantısı başarılı. Hedef: {_host}:{_port}");
                    result.Duration = stopwatch.Elapsed;
                    return result;
                }

                using var stream = client.GetStream();

                byte[] dataToSend = Encoding.UTF8.GetBytes(_payload);

                // 2. İyileştirme: Modern AsMemory() kullanımı ile Write/Read işlemlerine jetonu ekliyoruz
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
                var unhealthyResult = HealthCheckResult.Unhealthy($"TCP hatası ({_host}:{_port}): {ex.Message}", ex);
                unhealthyResult.Duration = stopwatch.Elapsed;
                return unhealthyResult;
            }
            finally
            {
                // 3. İyileştirme: Süreyi durdurma işlemini tek bir merkeze topladık
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
        }
    }
}