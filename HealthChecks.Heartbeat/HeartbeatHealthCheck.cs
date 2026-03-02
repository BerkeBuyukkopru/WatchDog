using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System.Diagnostics;

namespace HealthChecks.Heartbeat;

public class HeartbeatHealthCheck : IHealthCheck
{
    // İşçinin son imza attığı saati getirecek olan temsilci (Fonksiyon)
    private readonly Func<Task<DateTime?>> _getLastPulseAsync;

    // İşçinin ne kadar süredir kayıp olursa alarm verileceği (Örn: 15 dakika)
    private readonly TimeSpan _tolerance;

    public string Name => "Background Worker Monitor";

    // İnşaatçı: Bekçi bu uzmanı işe alırken ona "Defterin yerini" ve "Tolerans süresini" verir
    public HeartbeatHealthCheck(Func<Task<DateTime?>> getLastPulseAsync, TimeSpan tolerance)
    {
        _getLastPulseAsync = getLastPulseAsync;
        _tolerance = tolerance;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            // Defterdeki son imza saatini okuyoruz
            var lastPulse = await _getLastPulseAsync();

            watch.Stop();

            // Eğer defter boşsa (hiç imza atılmamışsa)
            if (!lastPulse.HasValue)
            {
                return new HealthCheckResult { Status = HealthStatus.Unhealthy, Duration = watch.Elapsed, Description = "İşçiden henüz hiç nabız alınamadı!" };
            }

            // Şu anki saatten, son imza saatini çıkarıp ne kadar zaman geçtiğini buluyoruz
            var timeSinceLastPulse = DateTime.UtcNow - lastPulse.Value;

            // Eğer geçen zaman, bizim toleransımızdan (örn 15 dk) fazlaysa alarm ver!
            if (timeSinceLastPulse > _tolerance)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Duration = watch.Elapsed,
                    Description = $"Kritik: Arka plan işçisi en son {timeSinceLastPulse.TotalMinutes} dakika önce çalıştı. Sistem durmuş olabilir!"
                };
            }

            // Her şey yolundaysa
            return new HealthCheckResult { Status = HealthStatus.Healthy, Duration = watch.Elapsed, Description = "Arka plan işçisi düzenli çalışıyor." };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new HealthCheckResult { Status = HealthStatus.Unhealthy, Description = $"Nabız kontrol hatası: {ex.Message}", Duration = watch.Elapsed, Exception = ex };
        }
    }
}