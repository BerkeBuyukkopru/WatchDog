using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Worker.BackgroundServices
{
    // Worker projelerinde HTTP isteği olmadığı için ismi sabit döneriz.
    public class WorkerCurrentUserService : ICurrentUserService
    {
        public string? Username => "WorkerService";
    }
}
