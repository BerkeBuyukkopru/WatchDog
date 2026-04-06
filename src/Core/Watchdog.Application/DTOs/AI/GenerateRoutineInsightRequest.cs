using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.AI
{
    // Rutin kapasite analizi (Use Case) için gereken parametreleri taşıyan DTO nesnesi.
    public class GenerateRoutineInsightRequest
    {
        // Hangi uygulamanın kapasitesi analiz edilecek?
        public Guid AppId { get; set; }

        // Geriye dönük kaç saatlik veri toplanacak? (Varsayılan: 1 saat)
        public int HoursToAnalyze { get; set; } = 1;
    }
}
