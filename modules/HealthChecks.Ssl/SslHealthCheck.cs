using HealthChecks.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.Ssl
{
    public class SslHealthCheck : IHealthCheck
    {
        private readonly string _host;

        private readonly int _daysBeforeExpiration;

        public string Name => $"Ssl_Certificate_Check";

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
                //Hedefin 443(Güvenli) portuna normal bir TCP bağlantısı açıyoruz.
                using var client = new TcpClient();
                await client.ConnectAsync(_host, 443);

                using var sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true));

                await sslStream.AuthenticateAsClientAsync(_host);

                var serverCertificate = sslStream.RemoteCertificate;

                if (serverCertificate == null)
                {
                    stopwatch.Stop();
                    return HealthCheckResult.Unhealthy($"{_host} adresinden SSL sertifikası alınamadı (Site HTTPS desteklemiyor olabilir).");
                }

                //Sertifikayı detaylı okuyabilmek için "X509Certificate2" formatına çeviriyoruz.
                var cert2 = new X509Certificate2(serverCertificate);

                //Bitiş tarihini (NotAfter) çekip alıyoruz.
                DateTime expirationDate = cert2.NotAfter;

                // Bugünden bitiş tarihine kadar kaç gün kaldığını hesaplıyoruz.
                int remainingDays = (expirationDate - DateTime.Now).Days;

                // İşlem bitti, kronometreyi durdur.
                stopwatch.Stop();

                // Kalan gün sayısı bizim güvenlik sınırımızdan küçük mü?
                if (remainingDays <= _daysBeforeExpiration)
                {
                    // Belirlediğimiz sınırın altına düştüyse sistem alarm vermeli!
                    var warningResult = HealthCheckResult.Unhealthy(
                        $"Kritik Uyarı: {_host} SSL sertifikasının bitmesine sadece {remainingDays} gün kaldı! (Son Tarih: {expirationDate:dd.MM.yyyy})");
                    warningResult.Duration = stopwatch.Elapsed;
                    return warningResult;
                }

                // Sınırın üstündeyse her şey yolunda.
                var healthyResult = HealthCheckResult.Healthy($"{_host} SSL sertifikası geçerli. Kalan süre: {remainingDays} gün.");
                healthyResult.Duration = stopwatch.Elapsed;
                return healthyResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return HealthCheckResult.Unhealthy($"SSL kontrolü sırasında ağ hatası ({_host}): {ex.Message}", ex);
            }

        }
    }
}
