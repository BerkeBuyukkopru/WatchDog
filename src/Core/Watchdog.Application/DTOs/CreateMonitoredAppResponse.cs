using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs
{
    public class CreateMonitoredAppResponse
    {
        public Guid Id { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
