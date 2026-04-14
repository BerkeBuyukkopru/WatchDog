using System;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Worker.BackgroundServices
{
    // Worker projelerinde HTTP isteği olmadığı için ismi ve ID'si manuel set edilir.
    public class WorkerCurrentUserService : ICurrentUserService
    {
        // Sistem botu olduğu için geçerli bir kullanıcı ID'si yok.
        public Guid UserId => Guid.Empty;

        // Varsayılan isim 'WorkerService' ancak artık dışarıdan değiştirilebilir.
        public string? Username { get; set; } = "WorkerService";
    }
}