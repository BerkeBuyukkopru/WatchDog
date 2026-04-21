using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.AI
{
    // AI'a kök neden (Root Cause) analizinde ipucu sağlayacak "Zenginleştirilmiş" veri taşıma objesi.
    public class DailyEnrichedSnapshotDto
    {
        public DateTime Date { get; set; }
        public double AvgCpu { get; set; }
        public double AvgRam { get; set; }
        public double AvgLatency { get; set; }

        // Zirve yük noktaları (Darboğazın ne kadar şiddetli olduğunu gösterir)
        public double MaxCpu { get; set; }
        public double MaxRam { get; set; }

        // Darboğazın yaşandığı saat dilimi (Trafik deseni tahmini için)
        public string PeakHour { get; set; } = string.Empty;

        // Kök neden analizi için en sık tekrar eden hatalar
        public List<string> TopErrors { get; set; } = new List<string>();
    }
}
