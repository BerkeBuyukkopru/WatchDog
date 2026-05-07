using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Apps
{
    public class AppDto
    {
        //Uygulamanın benzersiz kimliği. (Guid: Benzersiz tanımlayıcı türü((e6b2...))
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HealthUrl { get; set; } = string.Empty;
        // Tarama sıklığını saniye cinsinden belirten özellik.
        public int PollingIntervalSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public Guid? ActiveAiProviderId { get; set; }
        public bool IsActive { get; set; }
    }
}
