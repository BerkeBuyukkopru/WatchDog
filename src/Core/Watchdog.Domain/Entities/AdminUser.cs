using System;
using System.Collections.Generic;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    public class AdminUser : BaseEntity<Guid> // BaseEntity tipinizi projene göre kontrol et (Guid vs int)
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";

        // YENİ EKLENEN: Bu adminin erişimine izin verilen uygulamanın ID'leri.
        // Entity Framework Core, List<Guid> tipini destekler ve veritabanında JSON (veya text) olarak tutar.
        public List<Guid> AllowedAppIds { get; set; } = new List<Guid>();
    }
}