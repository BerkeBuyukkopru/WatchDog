using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Auth
{
    // Yöneticinin KENDİ profilini güncellerken kullanacağı DTO
    public class UpdateAdminProfileRequest
    {
        // Yöneticiler kendi kullanıcı adlarını değiştiremezler.
        public string? NewPassword { get; set; }
    }
}
