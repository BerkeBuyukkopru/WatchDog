using System;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    // İzlenen uygulamaların temel bilgilerini ve durumunu tutan ana sınıf.
    public class MonitoredApp : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string HealthUrl { get; set; } = string.Empty;
        public int PollingIntervalSeconds { get; set; }
        public string? NotificationEmails { get; set; }
        public string? ApiKey { get; set; }

        // YENİ EKLENEN: SuperAdmin'in bu uygulama için atadığı merkezi iletişim/şifre sıfırlama maili
        public string AdminEmail { get; set; } = string.Empty;

        // İŞ MANTIĞI: İzleme aktif mi pasif mi? (Silme ile karıştırılmamalı)
        public bool IsActive { get; set; } = true;

        // YENİ EKLENEN 1: Bu uygulamanın kullandığı aktif yapay zekanın ID'si
        public Guid? ActiveAiProviderId { get; set; }

        // YENİ EKLENEN 2: Entity Framework'ün iki tabloyu birbirine bağlaması için
        public virtual AiProvider? ActiveAiProvider { get; set; }
    }
}