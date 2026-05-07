using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Infrastructure.Notifications
{
    public class MailSettings
    {
        public string DisplayName { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty; // Fallback (Yedek) adres
        
        // SMTP Ayarları (MailKit için)
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1025;
        public bool UseSsl { get; set; } = false;

        // Eski API Ayarları (Opsiyonel olarak kalabilir)
        public string ApiToken { get; set; } = string.Empty;
        public string InboxId { get; set; } = string.Empty;
    }
}
