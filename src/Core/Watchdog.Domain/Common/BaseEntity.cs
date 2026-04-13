using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Domain.Common
{
    // TId: Tablonun kimlik (ID) tipini temsil eder. 
    public abstract class BaseEntity<TId>
    {
        // 1. Kimlik Bilgisi
        public TId Id { get; set; } = default!;

        // 2. Denetim (Audit) Alanları: Kim, ne zaman yaptı?
        // NOT: İlk değer atamaları (DateTime.UtcNow) model kararlılığı için kaldırıldı.
        // Bu alanlar DbContext.SaveChangesAsync içinde otomatik doldurulacaktır.
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        // 3. Soft Delete (Yumuşak Silme) Alanları: Veri silindi mi?
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

    // Projedeki çoğu varlık Guid kullandığı için bir de kısa yol tanımı yapalım:
    public abstract class BaseEntity : BaseEntity<Guid>
    {
        protected BaseEntity()
        {
            // ID ataması model building sırasında hata vermemesi için boş bırakıldı.
            // EF Core veya manuel üretim (UseCase) tarafından yönetilecektir.
        }
    }
}