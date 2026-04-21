using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.AI;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Common
{
    // Bu Interface, sistemin tüm yapay zeka promptlarını tek bir merkezden (PromptBuilder) üretmek için tasarlandı. (SOLID - SRP)
    public interface IPromptBuilder
    {
        // Kriz Anı (Event-Driven) Promptu
        string BuildRootCausePrompt(
            List<HealthSnapshot> recentSnapshots,
            string appName);

        // Saatlik Rutin Kapasite Promptu
        string BuildRoutinePrompt(
            MonitoredApp app,
            double cpuLimit, double ramLimit, double latencyLimit,
            double avgCpu24h, double avgRam24h, double avgLatency24h,
            double avgCpu2h, double avgRam2h, double avgLatency2h,
            double maxCpu2h, double maxRam2h, double maxLatency2h,
            string peakCpuTime, string dependencyContext,
            int outageCount);

        // Haftalık Stratejik Tahmin Promptu
        string BuildStrategicPrompt(
            MonitoredApp app,
            DailyEnrichedSnapshotDto baselineDay,
            DailyEnrichedSnapshotDto targetDay,
            double weeklyAvgCpu, double weeklyAvgRam,
            string baselineErrors, string targetErrors);
    }
}
