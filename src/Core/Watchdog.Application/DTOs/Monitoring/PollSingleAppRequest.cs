using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Monitoring
{
    public class PollSingleAppRequest
    {
        public Guid AppId { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
