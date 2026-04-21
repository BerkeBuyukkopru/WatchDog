using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Ssl
{
    public class SslHealthCheck : IHealthCheck
    {
        private readonly Func<string> _hostProvider;
        private readonly int _daysBeforeExpiration;

        public string Name => "Ssl_Certificate_Check";

        public SslHealthCheck(Func<string> hostProvider, int daysBeforeExpiration = 30)
        {
            _hostProvider = hostProvider;
            _daysBeforeExpiration = daysBeforeExpiration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            string currentHost = _hostProvider(); // O anki güncel host adresi

            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(currentHost, 443, cancellationToken);

                using var sslStream = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateCertificate));

                var authOptions = new SslClientAuthenticationOptions { TargetHost = currentHost };
                await sslStream.AuthenticateAsClientAsync(authOptions, cancellationToken);

                var serverCertificate = sslStream.RemoteCertificate;
                if (serverCertificate == null)
                {
                    return HealthCheckResult.Unhealthy($"{currentHost} adresinden SSL sertifikası alınamadı.");
                }

                using var cert2 = new X509Certificate2(serverCertificate);
                DateTime expirationDate = cert2.NotAfter;
                int remainingDays = (expirationDate - DateTime.Now).Days;

                if (remainingDays <= _daysBeforeExpiration)
                {
                    var warningResult = HealthCheckResult.Unhealthy(
                        $"Kritik Uyarı: {currentHost} SSL sertifikasının bitmesine sadece {remainingDays} gün kaldı! (Son Tarih: {expirationDate:dd.MM.yyyy})");
                    warningResult.Duration = stopwatch.Elapsed;
                    return warningResult;
                }

                var healthyResult = HealthCheckResult.Healthy($"{currentHost} SSL sertifikası geçerli. Kalan süre: {remainingDays} gün.");
                healthyResult.Duration = stopwatch.Elapsed;
                return healthyResult;
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"SSL kontrolü sırasında ağ hatası ({currentHost}): {ex.Message}", ex);
            }
            finally
            {
                if (stopwatch.IsRunning) stopwatch.Stop();
            }
        }

        private bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            return false;
        }
    }
}