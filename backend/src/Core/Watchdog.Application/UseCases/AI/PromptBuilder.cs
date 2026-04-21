using System.Text.Json;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.AI
{
    // Bütün yapay zeka soruları (Promptlar) UseCase'lerden çıkarılıp buraya taşındı. Artık metinlerde bir değişiklik yapacaksak UseCase sınıflarını kirletmeden buradan yapacağız.
    // Tüm promptlar "Birleşik (Unified)" formata geçirildi. Hangi AI motoru (OpenAI veya Ollama)  çalışırsa çalışsın, aynı veri setini alacak ve ÇOK KATI bir şekilde aynı 3 başlıkta cevap vermeye zorlanacak.
    public class PromptBuilder : IPromptBuilder
    {
        // --- 1. KRİZ ANI (EVENT-DRIVEN) PROMPTU ---
        public string BuildRootCausePrompt(List<HealthSnapshot> recentSnapshots, string appName)
        {
            var summary = AggregateSnapshots(recentSnapshots);
            var jsonContext = JsonSerializer.Serialize(summary);

            return $@"SYSTEM ROLE: You are an Expert DevOps and SRE AI.

[CONTEXT - TELEMETRY DATA]
App Name: '{appName}'
Current Status: UNHEALTHY (Crash Detected)
Recent Logs: {jsonContext}

[TASK]
Analyze the data above. Your output MUST EXACTLY MATCH the 3 sections below. Do not add any greetings or conversational text.

ROOT CAUSE ANALYSIS:
(Write your root cause analysis here based on the logs)

CAPACITY STATUS:
(Write the current resource status here)

ACTIONABLE ADVICE:
(Write 1-2 direct technical steps to recover the system)";
        }

        // --- 2. SAATLİK RUTİN (CAPACITY) PROMPTU ---
        public string BuildRoutinePrompt(
      MonitoredApp app,
      double cpuLimit, double ramLimit, double latencyLimit,
      double avgCpu24h, double avgRam24h, double avgLatency24h,
      double avgCpu2h, double avgRam2h, double avgLatency2h,
      double maxCpu2h, double maxRam2h, double maxLatency2h,
      string peakCpuTime, string dependencyContext,
      int outageCount) 
        {
            return $@"SYSTEM ROLE: You are an automated SRE diagnostic engine.

[STRICT INTERPRETATION RULE]:
- If CPU/RAM values are near 0% AND 'Outages Detected' is greater than 0, this means the app was CRASHED/DOWN, not efficient.
- Diagnose these 0% values as 'Service Unavailable' in your report.

[CONFIGURATION & THRESHOLDS]
CPU Limit: {cpuLimit}% | RAM Limit: {ramLimit}% | Latency Limit: {latencyLimit}ms

[TELEMETRY & AVAILABILITY DATA]
App: {app.Name}
Outages Detected (Last 24h): {outageCount} times! << IMPORTANT
CPU Stats: 24h-Avg: {avgCpu24h}%, 2h-PEAK: {maxCpu2h}% at {peakCpuTime}
RAM Stats: 24h-Avg: {avgRam24h}%, 2h-PEAK: {maxRam2h}%
Latency Stats: 24h-Avg: {avgLatency24h}ms, 2h-Avg: {avgLatency2h}ms
Dependencies: {dependencyContext}

[TASK]
Analyze the data. If outages occurred, focus your Root Cause Analysis on why the service was down.
Your output MUST EXACTLY MATCH the 3 sections below.

ROOT CAUSE ANALYSIS:
(Explain if it's high load or a total service outage)

CAPACITY STATUS:
(Evaluate resource usage vs outages)

ACTIONABLE ADVICE:
(Provide recovery or scaling steps)";
        }

        // --- 3. HAFTALIK STRATEJİK (FORECAST) PROMPTU ---
        public string BuildStrategicPrompt(
      MonitoredApp app,
      DailyEnrichedSnapshotDto baselineDay,
      DailyEnrichedSnapshotDto targetDay,
      double weeklyAvgCpu, double weeklyAvgRam,
      string baselineErrors, string targetErrors)
        {
            return $@"SYSTEM ROLE: You are an advanced AIOps and Capacity Planning AI. Your goal is to analyze historical trends, identify anomalies between matching days, and provide capacity forecasts.

STRICT RULES:
- Output ONLY the three requested sections below. No greetings, no markdown blocks around the text.
- Keep it highly professional and technical.

[COMPARATIVE DATA (Day-Over-Day)]
App: {app.Name}
Baseline (Last Week {baselineDay.Date:dddd}): Avg CPU: {baselineDay.AvgCpu}%, Max CPU: {baselineDay.MaxCpu}% | Avg RAM: {baselineDay.AvgRam}%, Max RAM: {baselineDay.MaxRam}% | Top Errors: {baselineErrors}
Target (Yesterday {targetDay.Date:dddd}): Avg CPU: {targetDay.AvgCpu}%, Max CPU: {targetDay.MaxCpu}% (Peak at {targetDay.PeakHour}) | Avg RAM: {targetDay.AvgRam}%, Max RAM: {targetDay.MaxRam}% | Top Errors: {targetErrors}

[WEEKLY TREND]
7-Day Rolling Averages -> CPU: {weeklyAvgCpu}%, RAM: {weeklyAvgRam}%

[TASK]
Analyze historical trends and anomalies between the baseline and target day.
Your output MUST EXACTLY MATCH the 3 sections below.

COMPARATIVE ROOT CAUSE:
(Identify why CPU/RAM changed or why specific errors occurred here)

WEEKLY FORECAST:
(Based on the 7-day trend, forecast the resource risk for next week here)

STRATEGIC RECOMMENDATION:
(Provide 1-2 architectural or scaling recommendations to handle future load here)";
        }

        // Kriz anında logları hafifleten özel metot
        private object AggregateSnapshots(List<HealthSnapshot> snapshots)
        {
            return new
            {
                TotalRecords = snapshots.Count,
                AverageCpu = snapshots.Average(s => s.CpuUsage),
                AverageRam = snapshots.Average(s => s.RamUsage),
                LowestDiskSpace = snapshots.Min(s => s.FreeDiskGb),
                ErrorCounts = snapshots.Count(s => s.Status == HealthStatus.Unhealthy),
                LatestDependencies = snapshots.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.DependencyDetails
            };
        }
    }
}