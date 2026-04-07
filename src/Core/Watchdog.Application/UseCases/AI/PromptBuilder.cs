using System.Text.Json;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.AI
{
    public class PromptBuilder
    {
       // Kriz anında (Event-Driven) Root Cause (Kök Neden) Analizi için [cite: 943, 1169]
        public string BuildRootCausePrompt(List<HealthSnapshot> recentSnapshots, string appName)
        {
            var summary = AggregateSnapshots(recentSnapshots);
            var jsonContext = JsonSerializer.Serialize(summary);

            return $@"Sen kıdemli bir DevOps ve Sistem Yöneticisisin. 
'{appName}' isimli uygulamada anlık bir çöküş (Unhealthy) tespit edildi. 
Aşağıdaki özet metrikleri (CPU, RAM, Disk, Bağımlılıklar) analiz et:
{jsonContext}
Sence bu çöküşün kök nedeni (Root Cause) ne olabilir? Lütfen kısa ve teknik bir açıklama ile çözüm önerisi sun.";
        }

        // Verileri LLM için hafifletme (Aggregation) 
        private object AggregateSnapshots(List<HealthSnapshot> snapshots)
        {
            return new
            {
                TotalRecords = snapshots.Count,
                AverageCpu = snapshots.Average(s => s.CpuUsage),
                AverageRam = snapshots.Average(s => s.RamUsage),
                LowestDiskSpace = snapshots.Min(s => s.FreeDiskGb),
                ErrorCounts = snapshots.Count(s => s.Status == HealthStatus.Unhealthy),
                // Son hata detayını (Bağımlılıklar) ekle
                LatestDependencies = snapshots.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.DependencyDetails
            };
        }
    }
}