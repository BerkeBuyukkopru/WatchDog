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
        public string ApiToken { get; set; } = string.Empty;
        public string InboxId { get; set; } = string.Empty;
    }
}
