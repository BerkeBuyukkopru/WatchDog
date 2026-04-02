using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs
{
    public class UpdateAppEmailsRequest
    {
        // Güvenlik için URL'den (Route) alacağız ama DTO içinde de tutmak CQRS için iyidir.
        public Guid AppId { get; set; }

        // React'ten gelecek olan virgüllü mail listesi
        public string NotificationEmails { get; set; } = string.Empty;
    }
}
