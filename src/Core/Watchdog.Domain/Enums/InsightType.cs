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
        ScalingAdvice = 2,

        // SİSTEM STABİL: AI'ın uyandırılmadığı, her şeyin yolunda olduğunu gösteren rutin kayıt (Yeşil).
        SystemStable = 3,

        // Günlük/Haftalık karşılaştırmalar ve kapasite tahminleri (Sarı renkli vizyoner rapor).
        StrategicForecast = 4
    }
}
