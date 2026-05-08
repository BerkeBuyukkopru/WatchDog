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
        public string BuildRootCausePrompt(List<HealthSnapshot> recentSnapshots, string appName, bool isLocal = false)
        {
            var summary = AggregateSnapshots(recentSnapshots);
            var jsonContext = JsonSerializer.Serialize(summary);

            if (isLocal)
            {
                return $@"SYSTEM ROLE: You are an expert SRE (Site Reliability Engineer) and Incident Response Specialist.
TASK: Analyze the outage for the application '{appName}'. 
RULES:
- Start your response exactly with this prefix: 🚨 CRITICAL INCIDENT AND ROOT CAUSE ANALYSIS
- Respond in ENGLISH (Technical and professional).
- DO NOT bold or use asterisks (**) for headings. Use plain text only.
- Strictly follow the 3-section structure below.

[DATA CONTEXT]
Recent Telemetry Data: {jsonContext}

ROOT CAUSE ANALYSIS:
(Based on the data, explain the technical cause of the outage in detail)

CAPACITY STATUS:
(Evaluate if current resources are sufficient to handle this load)

STRATEGIC ADVICE:
(Suggest 1-2 immediate technical steps to restore the system)";
            }

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
            int outageCount,
            bool isLocal = false)
        {
            if (isLocal)
            {
                return $@"SYSTEM ROLE: You are an AIOps Capacity Analysis Expert.
TASK: Review the performance trends for the application '{app.Name}' over the last 24 hours.
RULES:
- Start your response exactly with this prefix: 📊 DAILY SYSTEM PERFORMANCE AND STATUS ANALYSIS
- Use professional ENGLISH.
- DO NOT bold or use asterisks (**) for headings. Use plain text only.
- Strictly follow the 3-section structure below.

[DAILY TELEMETRY SUMMARY]
Outage Count: {outageCount} | Avg Latency: {avgLatency24h}ms
CPU Avg/Max: %{avgCpu24h} / %{maxCpu2h}
RAM Avg/Max: %{avgRam24h} / %{maxRam2h}
Dependencies: {dependencyContext}

ROOT CAUSE ANALYSIS:
(Technically explain the performance changes and outages, if any, in the last 24 hours)

CAPACITY STATUS:
(Analyze the current load and resource consumption by comparing them with defined thresholds)

STRATEGIC ADVICE:
(Suggest technical steps to improve system stability within the next few hours)";
            }

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
            string baselineErrors, string targetErrors,
            bool isLocal = false)
        {
            if (isLocal)
            {
                return $@"SYSTEM ROLE: You are a Senior Infrastructure Architect and Capacity Planner.
TASK: Analyze the data for the last 30 days for '{app.Name}' and provide a monthly trend report.
RULES:
- Start your response exactly with this prefix: 📅 MONTHLY SYSTEM PERFORMANCE AND TREND ANALYSIS
- Use professional ENGLISH.
- DO NOT bold or use asterisks (**) for headings. Use plain text only.
- Strictly follow the 3-section structure below.

[MONTHLY COMPARATIVE DATA SET]
Baseline (30 Days Ago): CPU %{baselineDay.AvgCpu}, RAM %{baselineDay.AvgRam} | Errors: {baselineErrors}
Target (Yesterday): CPU %{targetDay.AvgCpu}, RAM %{targetDay.AvgRam} | Errors: {targetErrors}
30-Day General Trend: CPU %{weeklyAvgCpu}, RAM %{weeklyAvgRam}

PERFORMANCE AND CHANGE ANALYSIS:
(Identify the overall trend over the last 30 days, including improvement or worsening trends)

FUTURE RISK AND CAPACITY FORECAST:
(Predict potential resource depletion or stability issues for the coming month)

STRATEGIC ARCHITECTURE AND RESOURCE RECOMMENDATIONS:
(Provide capacity planning or code improvement recommendations for a more efficient month ahead)";
            }

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
