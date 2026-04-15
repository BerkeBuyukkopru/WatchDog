using System;
using System.Collections.Generic;

namespace Watchdog.Application.DTOs.Auth
{
    // Yeni bir admin kaydı için gerekli bilgiler.
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // YENİ EKLENEN: İsteğe bağlı olarak takip edilecek uygulamaların ID'leri.
        public List<Guid>? AllowedAppIds { get; set; }
    }
}