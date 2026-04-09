using HealthChecks.Abstractions;
using HealthChecks.Abstractions.Enums;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Heartbeat;

public class HeartbeatHealthCheck : IHealthCheck
{
    // İşçinin son imza attığı saati getirecek olan temsilci (İptal token'ı alacak şekilde güncellendi)
    private readonly Func<CancellationToken, Task<DateTime?>> _getLastPulseAsync;

    private readonly TimeSpan _tolerance;

    public string Name => "Background_Worker_Pulse";

    public HeartbeatHealthCheck(Func<CancellationToken, Task<DateTime?>> getLastPulseAsync, TimeSpan tolerance)
    {
        _getLastPulseAsync = getLastPulseAsync;
        _tolerance = tolerance;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            // Defterdeki son imza saatini okuyoruz (Token'ı içeri aktararak)
            var lastPulse = await _getLastPulseAsync(cancellationToken);
            watch.Stop();

            if (!lastPulse.HasValue)
            {
                var result = HealthCheckResult.Unhealthy("İşçiden henüz hiç nabız alınamadı!");
                result.Duration = watch.Elapsed;
                return result;
            }

            var timeSinceLastPulse = DateTime.UtcNow - lastPulse.Value;

            // Metrik verisini Data Dictionary içine koyuyoruz
            var telemetryData = new Dictionary<string, object>
            {
                { "MinutesSinceLastPulse", Math.Round(timeSinceLastPulse.TotalMinutes, 2) },
                { "ToleranceMinutes", _tolerance.TotalMinutes }
            };

            // Eğer geçen zaman, toleransımızdan fazlaysa
            if (timeSinceLastPulse > _tolerance)
            {
                var result = HealthCheckResult.Unhealthy(
                    $"Kritik: Arka plan işçisi en son {timeSinceLastPulse.TotalMinutes:F1} dakika önce çalıştı. Sistem durmuş olabilir!",
                    data: telemetryData); // Data'yı gönderiyoruz
                result.Duration = watch.Elapsed;
                return result;
            }

            // Her şey yolundaysa
            var healthyResult = HealthCheckResult.Healthy("Arka plan işçisi düzenli çalışıyor.", data: telemetryData);
            healthyResult.Duration = watch.Elapsed;
            return healthyResult;
        }
        catch (Exception ex)
        {
            watch.Stop();
            var result = HealthCheckResult.Unhealthy($"Nabız kontrol hatası: {ex.Message}", ex);
            result.Duration = watch.Elapsed;
            return result;
        }
    }
}