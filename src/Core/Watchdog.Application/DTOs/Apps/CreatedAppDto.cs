using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Apps
{
    public class CreatedAppDto //Yeni kayıt oluştururken arayüzden gelecek verileri tutacak sınıf.
    {
        public string Name { get; set; } = string.Empty;
        public string HealthUrl { get; set; } = string.Empty;
        // Tarama sıklığını saniye cinsinden belirten özellik.
        public int PollingIntervalSeconds { get; set; }
    }
}
