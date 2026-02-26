using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.Abstractions.Enums
{
    public enum HealthStatus
    {
        Healthy = 1,      // Sistem tıkır tıkır çalışıyor
        Degraded = 2,     // Çalışıyor ama bir şeyler yavaş veya bazı yan servisler sıkıntılı
        Unhealthy = 3     // Sistem yanıt vermiyor veya tamamen çökmüş
    }
}
