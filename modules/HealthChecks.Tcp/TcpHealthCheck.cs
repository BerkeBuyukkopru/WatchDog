using HealthChecks.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace HealthChecks.Tcp
{
    public class TcpHealthCheck: IHealthCheck
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
            using var client = new TcpClient();

            try
            {
                await client.ConnectAsync(_host, _port);

                // Eğer içeriye gönderilecek bir mesaj (payload) verilmediyse, sadece kapının açık olması yeterlidir.
                if (string.IsNullOrEmpty(_payload))
                {
                    stopwatch.Stop();
                    var result = HealthCheckResult.Healthy($"TCP bağlantısı başarılı. Hedef: {_host}:{_port}");
                    result.Duration = stopwatch.Elapsed;
                    return result;
                }

                //Sunucu ile veri akışını sağlayacak dijital bir boru (stream) açar.
                using var stream = client.GetStream();

                // Mesajımızı bilgisayarın anladığı byte (0 ve 1) diline çeviriyoruz.
                byte[] dataToSend = Encoding.UTF8.GetBytes(_payload);
                await stream.WriteAsync(dataToSend, 0, dataToSend.Length);

                byte[] buffer = new byte[1024]; // Gelecek cevabı tutacağımız sepet
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                stopwatch.Stop();
                
                if (!string.IsNullOrEmpty(_response) &&
                response.IndexOf(_response, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    var failResult = HealthCheckResult.Unhealthy($"Bağlantı başarılı ama beklenen cevap alınamadı. Gelen: {response}");
                    failResult.Duration = stopwatch.Elapsed;
                    return failResult;
                }

                var successResult = HealthCheckResult.Healthy($"TCP Client testi başarılı. Gelen Cevap: {response}");
                successResult.Duration = stopwatch.Elapsed;
                return successResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var unhealthyResult = HealthCheckResult.Unhealthy($"TCP hatası ({_host}:{_port}): {ex.Message}", ex);
                unhealthyResult.Duration = stopwatch.Elapsed;
                return unhealthyResult;
            }
        }
    }
}
