using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Enums;

namespace Watchdog.Application.DTOs.Apps
{
    public class CreateMonitoredAppResponse
    {
        public Guid Id { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public AppErrorCode ErrorCode { get; internal set; }
    }
}
