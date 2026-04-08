using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Common
{
    // Bu Interface, sistemin tüm yapay zeka promptlarını tek bir merkezden (PromptBuilder) üretmek için tasarlandı. (SOLID - SRP)
    public interface IPromptBuilder
    {
        // Kriz anında (çöküşte) Kök Neden tahmini için kullanılan prompt
        string BuildRootCausePrompt(string activeProvider, List<HealthSnapshot> recentSnapshots, string appName);

        // Saatlik kapasite ve zirve (peak) analizi için kullanılan prompt
        string BuildRoutinePrompt(
            string activeProvider,
            MonitoredApp app,
            double cpuLimit, double ramLimit, double latencyLimit,
            double avgCpu24h, double avgRam24h, double avgLatency24h,
            double avgCpu2h, double avgRam2h, double avgLatency2h,
            double maxCpu2h, double maxRam2h, double maxLatency2h,
            string peakCpuTime, string dependencyContext);

        // Günlük/Haftalık karşılaştırmalı (Target vs Baseline) kapasite tahmini için kullanılan prompt
        string BuildStrategicPrompt(
            string activeProvider,
            MonitoredApp app,
            dynamic baselineDay, dynamic targetDay,
            double weeklyAvgCpu, double weeklyAvgRam, 
            string baselineErrors, string targetErrors);
    }
}
