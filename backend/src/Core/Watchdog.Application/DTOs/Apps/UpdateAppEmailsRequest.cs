using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Attributes;

namespace Watchdog.Application.DTOs.Apps
{
    public class UpdateAppEmailsRequest
    {
        // Güvenlik için URL'den (Route) alacağız ama DTO içinde de tutmak CQRS için iyidir.
        public Guid AppId { get; set; }

        // React'ten gelecek olan virgüllü mail listesi

        [CommaSeparatedEmails(ErrorMessage = "Geçersiz e-posta formatı tespit edildi")]
        public string NotificationEmails { get; set; } = string.Empty;
    }
}
