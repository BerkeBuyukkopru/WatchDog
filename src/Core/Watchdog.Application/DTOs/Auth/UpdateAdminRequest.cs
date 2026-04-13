using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Auth
{
    // Admin bilgilerini güncellemek için kullanılan veri taşıma sınıfı.
    public class UpdateAdminRequest
    {
        // Hangi adminin güncelleneceğini belirten benzersiz kimlik.
        public Guid Id { get; set; }

        // Yeni veya mevcut kullanıcı adı.
        public string Username { get; set; } = string.Empty;

        // Eğer şifre değiştirilmek isteniyorsa doldurulur.
        public string? NewPassword { get; set; }
    }
}
