using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Monitoring
{
    public class GetLatestStatusesRequest
    {
        public int Count { get; set; } = 50; // Varsayılan değer

        // Null gelirse Global logları, dolu gelirse o uygulamanın loglarını çekeriz.
        public Guid? AppId { get; set; }
    }
}
