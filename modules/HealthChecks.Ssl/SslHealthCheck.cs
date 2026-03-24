using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums; // Önceki kodlarınızdan varsaydığım namespace
using System;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.Ssl
{
    public class SslHealthCheck : IHealthCheck
    {
        private readonly string _host;
        private readonly int _daysBeforeExpiration;

        public string Name => "Ssl_Certificate_Check";

        public SslHealthCheck(string host, int daysBeforeExpiration = 30)
        {
            _host = host;
            _daysBeforeExpiration = daysBeforeExpiration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var client = new TcpClient();

                // cancellationToken'ı ConnectAsync'e dahil ediyoruz (.NET 5 ve üzeri destekler)
                await client.ConnectAsync(_host, 443, cancellationToken);

                // Sertifika hatalarını yakalamak için callback'i güncelledik
                using var sslStream = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateCertificate));

                // cancellationToken destekleyen modern AuthenticateAsClientAsync kullanımı
                var authOptions = new SslClientAuthenticationOptions { TargetHost = _host };
                await sslStream.AuthenticateAsClientAsync(authOptions, cancellationToken);

                var serverCertificate = sslStream.RemoteCertificate;
                if (serverCertificate == null)
                {
                    return HealthCheckResult.Unhealthy($"{_host} adresinden SSL sertifikası alınamadı.");
                }

                // KRİTİK: X509Certificate2 IDisposable olduğu için 'using var' kullanıyoruz.
                using var cert2 = new X509Certificate2(serverCertificate);

                DateTime expirationDate = cert2.NotAfter;
                int remainingDays = (expirationDate - DateTime.Now).Days;

                if (remainingDays <= _daysBeforeExpiration)
                {
                    var warningResult = HealthCheckResult.Unhealthy(
                        $"Kritik Uyarı: {_host} SSL sertifikasının bitmesine sadece {remainingDays} gün kaldı! (Son Tarih: {expirationDate:dd.MM.yyyy})");
                    warningResult.Duration = stopwatch.Elapsed;
                    return warningResult;
                }

                var healthyResult = HealthCheckResult.Healthy($"{_host} SSL sertifikası geçerli. Kalan süre: {remainingDays} gün.");
                healthyResult.Duration = stopwatch.Elapsed;
                return healthyResult;
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"SSL kontrolü sırasında ağ hatası ({_host}): {ex.Message}", ex);
            }
            finally
            {
                // İşlem sonunda her halükarda sayacı durdur.
                if (stopwatch.IsRunning) stopwatch.Stop();
            }
        }

        // SSL hatalarını kontrol eden yardımcı metod
        private bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            // Eğer hiçbir SSL hatası yoksa true dön
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Eğer isterseniz burada sadece "Süresi Geçmiş" (RemoteCertificateChainErrors vs) hatalarını görmezden gelip, 
            // kalan gün sayısını kendi kodunuzun yakalamasını sağlayabilirsiniz. 
            // Ancak sahte sertifikalara karşı koruma sağlaması için varsayılan olarak false dönmesi daha iyidir.
            return false;
        }
    }
}