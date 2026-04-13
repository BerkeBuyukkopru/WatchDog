using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Watchdog.Application.Attributes;

namespace Watchdog.Application.DTOs.Apps
{
    public class CreateMonitoredAppRequest
    {
        [Required(ErrorMessage = "Uygulama adı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Health URL zorunludur.")]
        [Url(ErrorMessage = "Lütfen geçerli bir URL giriniz.")] // TTD Format kuralı!
        public string HealthUrl { get; set; } = string.Empty;

        public int PollingIntervalSeconds { get; set; } = 60;

        // Yeni eklediğimiz çoklu mail alanı
        [CommaSeparatedEmails(ErrorMessage = "Geçersiz e-posta formatı")]
        public string? NotificationEmails { get; set; }
    }
}
