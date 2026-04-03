using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.ExternalClients
{
    public interface INotificationSender
    {
        // Sistem çöktüğünde (3-Strike gerçekleştiğinde) gönderilecek acil durum e-postası
        Task SendDowntimeAlertAsync(Incident incident, MonitoredApp app);

        // Sistem tekrar ayağa kalktığında gönderilecek "Her şey yolunda" e-postası
        Task SendRecoveryAlertAsync(Incident incident, MonitoredApp app);
    }
}
