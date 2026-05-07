using System.Text.Json;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Watchdog.Application.UseCases.AI
{
    public class PromptBuilder : IPromptBuilder
    {
        // --- 1. EVENT-DRIVEN RCA PROMPT ---
        public string BuildRootCausePrompt(List<HealthSnapshot> recentSnapshots, string appName)
        {
            var summary = AggregateSnapshots(recentSnapshots);
            var jsonContext = JsonSerializer.Serialize(summary);

            return $@"SİSTEM ROLÜ: Sen uzman bir SRE (Site Reliability Engineer) ve Olay Müdahale Uzmanısın.
GÖREV: '{appName}' uygulaması için oluşan kesintiyi analiz et. 
KURALLAR:
- Cevabına tam olarak şu önekle başla: 🚨 KRİTİK OLAY VE KÖK NEDEN ANALİZİ
- KESİNLİKLE TÜRKÇE CEVAP VERMELİSİN (İstanbul Türkçesi).
- Başlıkları KESİNLİKLE kalınlaştırma (bold) veya yıldız (**) ile işaretleme. Sadece düz metin kullan.
- Aşağıdaki 3 bölümlü yapıyı eksiksiz uygula.

[VERİ BAĞLAMI]
Son Telemetri Verileri: {jsonContext}

KÖK NEDEN ANALİZİ:
(Verilere dayanarak kesintinin neden kaynaklandığını teknik detaylarıyla açıkla)

KAPASİTE DURUMU:
(Mevcut kaynakların bu yükü kaldırmak için yeterli olup olmadığını değerlendir)

STRATEJİK TAVSİYE:
(Sistemi ayağa kaldırmak için 1-2 acil teknik adım öner)";
        }

        // --- 2. HOURLY ROUTINE PROMPT ---
        public string BuildRoutinePrompt(
            MonitoredApp app,
            double cpuLimit, double ramLimit, double latencyLimit,
            double avgCpu24h, double avgRam24h, double avgLatency24h,
            double avgCpu2h, double avgRam2h, double avgLatency2h,
            double maxCpu2h, double maxRam2h, double maxLatency2h,
            string peakCpuTime, string dependencyContext,
            int outageCount)
        {
            return $@"SİSTEM ROLÜ: Sen bir AIOps Kapasite Analiz Uzmanısın.
GÖREV: '{app.Name}' uygulaması için son 24 saatlik performans trendlerini incele.
KURALLAR:
- Cevabına tam olarak şu önekle başla: 📊 GÜNLÜK SİSTEM PERFORMANS VE DURUM ANALİZİ
- KESİNLİKLE PROFESYONEL TÜRKÇE KULLAN.
- Başlıkları KESİNLİKLE kalınlaştırma (bold) veya yıldız (**) ile işaretleme. Sadece düz metin kullan.
- Aşağıdaki 3 bölümlü yapıyı eksiksiz uygula.

[GÜNLÜK TELEMETRİ ÖZETİ]
Kesinti Sayısı: {outageCount} | Ort. Gecikme: {avgLatency24h}ms
İşlemci (CPU) Ort/Maks: %{avgCpu24h} / %{maxCpu2h}
Bellek (RAM) Ort/Maks: %{avgRam24h} / %{maxRam2h}
Bağımlılıklar: {dependencyContext}

KÖK NEDEN ANALİZİ:
(Son 24 saatteki performans değişimlerini ve varsa kesintileri teknik olarak açıkla)

KAPASİTE DURUMU:
(Mevcut yükü ve kaynak tüketimini tanımlanan eşiklerle karşılaştırarak analiz et)

STRATEJİK TAVSİYE:
(Önümüzdeki birkaç saat içinde sistem stabilitesini artıracak teknik adımlar öner)";
        }

        // --- 3. WEEKLY STRATEGIC PROMPT ---
        public string BuildStrategicPrompt(
            MonitoredApp app,
            DailyEnrichedSnapshotDto baselineDay,
            DailyEnrichedSnapshotDto targetDay,
            double weeklyAvgCpu, double weeklyAvgRam,
            string baselineErrors, string targetErrors)
        {
            return $@"SİSTEM ROLÜ: Sen Kıdemli Altyapı Mimarı ve Kapasite Planlamacısın.
GÖREV: '{app.Name}' için son 30 günlük verileri analiz et ve aylık trend raporu sun.
KURALLAR:
- Cevabına tam olarak şu önekle başla: 📅 AYLIK SİSTEM PERFORMANS VE TREND ANALİZİ
- KESİNLİKLE PROFESYONEL TÜRKÇE KULLAN.
- Başlıkları KESİNLİKLE kalınlaştırma (bold) veya yıldız (**) ile işaretleme. Sadece düz metin kullan.
- Aşağıdaki 3 bölümlü yapıyı eksiksiz uygula.

[AYLIK KARŞILAŞTIRMALI VERİ SETİ]
Referans (30 Gün Önce): CPU %{baselineDay.AvgCpu}, RAM %{baselineDay.AvgRam} | Hatalar: {baselineErrors}
Hedef (Dün): CPU %{targetDay.AvgCpu}, RAM %{targetDay.AvgRam} | Hatalar: {targetErrors}
30 Günlük Genel Trend: CPU %{weeklyAvgCpu}, RAM %{weeklyAvgRam}

PERFORMANS VE DEĞİŞİM ANALİZİ:
(Son 30 gündeki genel gidişatı, iyileşme veya kötüleşme trendlerini tanımla)

GELECEK AY İÇİN RİSK VE KAPASİTE TAHMİNİ:
(Önümüzdeki ay için olası kaynak tükenmesi veya stabilite sorunlarını öngör)

STRATEJİK MİMARİ VE KAYNAK ÖNERİLERİ:
(Gelecek ayın daha verimli geçmesi için kapasite planlaması veya kod iyileştirme tavsiyeleri sun)";
        }

        private object AggregateSnapshots(List<HealthSnapshot> snapshots)
        {
            if (snapshots == null || !snapshots.Any()) return new { };
            return new
            {
                TotalRecords = snapshots.Count,
                AverageAppCpu = snapshots.Average(s => s.AppCpuUsage),
                AverageSystemCpu = snapshots.Average(s => s.SystemCpuUsage),
                AverageAppRam = snapshots.Average(s => s.AppRamUsage),
                AverageSystemRam = snapshots.Average(s => s.SystemRamUsage),
                LowestDiskSpace = snapshots.Min(s => s.FreeDiskGb),
                ErrorCounts = snapshots.Count(s => s.Status == HealthStatus.Unhealthy),
                LatestDependencies = snapshots.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.DependencyDetails
            };
        }
    }
}
