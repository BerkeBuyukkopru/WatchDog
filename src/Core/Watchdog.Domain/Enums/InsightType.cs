using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Domain.Enums
{
    public enum InsightType
    {
        // Sistem çöktüğünde veya anlık darboğaz yaşandığında üretilen acil durum uyarısı (Kırmızı). Kök Neden Analizi (Root Cause Analysis) içerir.
        CrashWarning = 1,

        // Rutin zamanlanmış görevle üretilen kapasite planlama tavsiyesi (Mavi). İleriye dönük ölçekleme (Scaling Advice) önerileri içerir.
        ScalingAdvice = 2
    }
}
