using System;
using System.Collections.Generic;
using Watchdog.Domain.Common;

namespace Watchdog.Domain.Entities
{
    public class AdminUser : BaseEntity<Guid>
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";

        // YENİ EKLENENLER: E-posta ve Şifre Sıfırlama Alanları
        public string Email { get; set; } = string.Empty; // Uygulamadan miras alınacak mail
        public string? PasswordResetCode { get; set; }
        public DateTime? ResetCodeExpiration { get; set; }

        // Bu adminin erişimine izin verilen uygulamanın ID'leri.
        public List<Guid> AllowedAppIds { get; set; } = new List<Guid>();
    }
}