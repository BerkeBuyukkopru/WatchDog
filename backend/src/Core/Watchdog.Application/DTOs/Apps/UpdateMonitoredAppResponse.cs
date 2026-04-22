using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Enums;

namespace Watchdog.Application.DTOs.Apps
{
    public class UpdateMonitoredAppResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public AppErrorCode ErrorCode { get; set; } // Create ile tam uyum için eklenebilir
    }
}
