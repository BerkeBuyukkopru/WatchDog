using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Apps
{
    public class CreateMonitoredAppRequest
    {
        public string Name { get; set; } = string.Empty;
        public string HealthUrl { get; set; } = string.Empty;
        public int PollingIntervalSeconds { get; set; } = 60;

        // Yeni eklediğimiz çoklu mail alanı
        public string? NotificationEmails { get; set; }
    }
}
