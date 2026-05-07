using System;

namespace Watchdog.Domain.Common
{
    /// <summary>
    /// Sadece oluşturulma bilgisini tutan, güncelleme veya silme gerektirmeyen log/telemetri verileri için temel sınıf.
    /// Veri tabanında yer tasarrufu sağlar.
    /// </summary>
    public abstract class SimpleBaseEntity<TId>
    {
        public TId Id { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public abstract class SimpleBaseEntity : SimpleBaseEntity<Guid>
    {
    }
}
