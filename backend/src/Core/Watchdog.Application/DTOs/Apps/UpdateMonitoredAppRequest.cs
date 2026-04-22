using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Watchdog.Application.DTOs.Apps
{
    public class UpdateMonitoredAppRequest
    {
        [Required]
        public Guid Id { get; set; } // Güncellenecek uygulama ID'si zorunlu

        [Required(ErrorMessage = "Uygulama adı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "İzlenecek URL zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string HealthUrl { get; set; } = string.Empty;

        [Range(10, 3600, ErrorMessage = "İzleme aralığı 10 saniye ile 1 saat arasında olmalıdır.")]
        public int PollingIntervalSeconds { get; set; }

        public string? NotificationEmails { get; set; } // Virgülle ayrılmış liste

        [Required(ErrorMessage = "Admin e-postası zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string AdminEmail { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public Guid? ActiveAiProviderId { get; set; }
    }
}
