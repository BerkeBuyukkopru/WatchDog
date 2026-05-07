using System.Text.Json;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.AI
{
    // Bütün yapay zeka soruları (Promptlar) UseCase'lerden çıkarılıp buraya taşındı. Artık metinlerde bir değişiklik yapacaksak UseCase sınıflarını kirletmeden buradan yapacağız.
    // Tüm promptlar "Birleşik (Unified)" formata geçirildi. Hangi AI motoru (OpenAI veya Ollama)  çalışırsa çalışsın, aynı veri setini alacak ve ÇOK KATI bir şekilde aynı 3 başlıkta cevap vermeye zorlanacak.
    public class PromptBuilder : IPromptBuilder
    {
        // --- 1. EVENT-DRIVEN RCA PROMPT ---
        public string BuildRootCausePrompt(List<HealthSnapshot> recentSnapshots, string appName)
        {
            var summary = AggregateSnapshots(recentSnapshots);
            var jsonContext = JsonSerializer.Serialize(summary);

            return $@"SYSTEM ROLE: You are an expert SRE (Site Reliability Engineer) and Incident Responder.
TASK: Analyze the crash data for '{appName}'. 
RULES:
- Start your response with the exact prefix: [KRİTİK OLAY ANALİZİ]
- YOU MUST RESPOND IN TURKISH.
- Use a professional, technical, and urgent tone.
- Follow the 3-section structure exactly.

[DATA CONTEXT]
Recent Telemetry: {jsonContext}

KÖK NEDEN ANALİZİ:
(Technically explain why the crash happened based on data)

KAPASİTE DURUMU:
(Assess if current resources are sufficient to handle this load)

ACİL EYLEM PLANI:
(Provide 1-2 immediate technical recovery steps)";
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
            return $@"SYSTEM ROLE: You are an AIOps Capacity Analysis Expert.
TASK: Review the 24-hour performance trends for '{app.Name}'.
RULES:
- Start your response with the exact prefix: [RUTİN KAPASİTE RAPORU]
- YOU MUST RESPOND IN TURKISH.
- If CPU/RAM is 0% while outages exist, diagnose it as 'Service Down' not 'Efficient'.
- Follow the 3-section structure exactly.

[TELEMETRY DATA]
Outages: {outageCount} | Latency Avg: {avgLatency24h}ms
CPU Avg/Max: {avgCpu24h}% / {maxCpu2h}%
RAM Avg/Max: {avgRam24h}% / {maxRam2h}%
Dependencies: {dependencyContext}

KÖK NEDEN ANALİZİ:
(Explain performance drops or outages)

KAPASİTE DURUMU:
(Evaluate resource usage vs configured limits)

STRATEJİK TAVSİYE:
(Provide optimization or scaling steps for the next few hours)";
        }

        // --- 3. WEEKLY STRATEGIC PROMPT ---
        public string BuildStrategicPrompt(
      MonitoredApp app,
      DailyEnrichedSnapshotDto baselineDay,
      DailyEnrichedSnapshotDto targetDay,
      double weeklyAvgCpu, double weeklyAvgRam,
      string baselineErrors, string targetErrors)
        {
            return $@"SYSTEM ROLE: You are a Senior Infrastructure Architect and Capacity Planner.
TASK: Compare last week vs yesterday for '{app.Name}' and forecast risks.
RULES:
- Start your response with the exact prefix: [STRATEJİK GELECEK TAHMİNİ]
- YOU MUST RESPOND IN TURKISH.
- Be highly strategic and look for long-term trends.
- Follow the 3-section structure exactly.

[COMPARATIVE DATA]
Baseline (Last Week): CPU {baselineDay.AvgCpu}%, RAM {baselineDay.AvgRam}% | Errors: {baselineErrors}
Target (Yesterday): CPU {targetDay.AvgCpu}%, RAM {targetDay.AvgRam}% | Errors: {targetErrors}
Weekly Trend: CPU {weeklyAvgCpu}%, RAM {weeklyAvgRam}%

KARŞILAŞTIRMALI ANALİZ:
(Identify key differences in behavior between the two periods)

HAFTALIK RİSK TAHMİNİ:
(Forecast potential resource exhaustion or stability issues for next week)

STRATEJİK ÖNERİ:
(Provide architectural or infrastructure improvements)";
        }

        // Kriz anında logları hafifleten özel metot (App ve System ayrımı yapıldı)
        private object AggregateSnapshots(List<HealthSnapshot> snapshots)
        {
            return new
            {
                TotalRecords = snapshots.Count,
                AverageAppCpu = snapshots.Average(s => s.AppCpuUsage),
                AverageSystemCpu = snapshots.Average(s => s.SystemCpuUsage), // AI Sunucuyu da görsün
                AverageAppRam = snapshots.Average(s => s.AppRamUsage),
                AverageSystemRam = snapshots.Average(s => s.SystemRamUsage), // AI Sunucuyu da görsün
                LowestDiskSpace = snapshots.Min(s => s.FreeDiskGb),
                ErrorCounts = snapshots.Count(s => s.Status == HealthStatus.Unhealthy),
                LatestDependencies = snapshots.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.DependencyDetails
            };
        }
    }
}