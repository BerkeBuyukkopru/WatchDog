using System;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Worker.BackgroundServices
{
    // Worker projelerinde HTTP isteği olmadığı için ismi ve ID'si manuel set edilir.
    public class WorkerCurrentUserService : ICurrentUserService
    {
        // Sistem botu olduğu için geçerli bir kullanıcı ID'si yok.
        public Guid UserId => Guid.Empty;

        // 🚨 ÇÖZÜM BURADA: { get; set; } sayesinde artık dışarıdan "StrategicAnalyzerWorker" gibi yeni isimler atanabilir.
        public string? Username { get; set; } = "WorkerService";

        // 🚨 ÖNCEKİ ADIMDAN KALAN ZORUNLULUK: Sözleşmenin (Interface) hata vermemesi için Role bilgisini ekliyoruz.
        public string Role => "SuperAdmin";
    }
}