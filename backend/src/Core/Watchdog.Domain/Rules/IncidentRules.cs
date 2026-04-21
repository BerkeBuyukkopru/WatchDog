using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Domain.Rules
{
    // Bu sınıf sistemin ne zaman "Eyvah!" diyeceğine karar verir.
    public static class IncidentRules
    {
        // Yeni bir kesinti (Incident) kaydı açılıp açılmayacağına karar verir. (3-Strike Kuralı)
        public static bool ShouldOpenIncident(List<HealthSnapshot> recentSnapshots, bool hasActiveIncident)
        {
            // Zaten açık bir olay varsa, yenisini açmaya gerek yok.
            if (hasActiveIncident) return false;

            // Eğer sistemde henüz yeterli veri yoksa (3 log bile birikmemişse), karar vermek için çok erken.
            if (recentSnapshots == null || recentSnapshots.Count < 3) return false;

            // Son 3 kaydın TAMAMI Unhealthy mi?
            bool isStrike3 = recentSnapshots.Take(3).All(s => s.Status == HealthStatus.Unhealthy);

            return isStrike3;
        }

        // Mevcut bir kesintinin (Incident) çözülüp çözülmediğine karar verir.
        public static bool ShouldResolveIncident(HealthSnapshot latestSnapshot, bool hasActiveIncident)
        {
            // Eğer açık bir olay yoksa, sistem zaten sağlıklı demektir.
            if (!hasActiveIncident) return false;

            //Tek bir 'Healthy' sinyali bile sistemin ayağa kalktığını müjdeler.
            return latestSnapshot.Status == HealthStatus.Healthy;
        }
    }
}
