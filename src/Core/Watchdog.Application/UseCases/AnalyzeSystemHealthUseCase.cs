using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;
using Watchdog.Domain.Rules;

namespace Watchdog.Application.UseCases
{
    // Sistem Sağlık Analizi. Worker'dan gelen her ping sonucu bu süzgeçten geçer.
    public class AnalyzeSystemHealthUseCase
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly INotificationSender _notificationSender;
        private readonly IMonitoredAppRepository _appRepository;

        public AnalyzeSystemHealthUseCase(
            ISnapshotRepository snapshotRepository,
            IIncidentRepository incidentRepository,
            INotificationSender notificationSender,
            IMonitoredAppRepository appRepository)
        {
            _snapshotRepository = snapshotRepository;
            _incidentRepository = incidentRepository;
            _notificationSender = notificationSender;
            _appRepository = appRepository;
        }

        public async Task ExecuteAsync(HealthSnapshot latestSnapshot)
        {
            // Hangi uygulamaya bakıyoruz ve zaten açık bir yarası (Incident) var mı?
            var app = await _appRepository.GetByIdAsync(latestSnapshot.AppId);
            if (app == null) return;

            // Bu uygulama için şu an devam eden bir kesinti (Alarm) var mı bakıyoruz
            var activeIncident = await _incidentRepository.GetActiveIncidentAsync(app.Id);
            bool hasActiveIncident = activeIncident != null;

            // Eğer sistem temizse ama son gelen sinyal kötüyse geçmişi sorgula.
            if (!hasActiveIncident && latestSnapshot.Status == HealthStatus.Unhealthy)
            {
                // ISnapshotRepository: Sistemin hafızasına gidip son 3 kaydı çekiyoruz.
                var recentSnapshots = await _snapshotRepository.GetLatestSnapshotsAsync(app.Id, 3);

                // IncidentRules: Domain katmanındaki o saf C# kuralını işletiyoruz.
                if (IncidentRules.ShouldOpenIncident(recentSnapshots, hasActiveIncident))
                {
                    // Çöküş tespit edildi! Yeni kayıt oluştur.
                    var newIncident = new Incident
                    {
                        AppId = app.Id,
                        StartedAt = DateTime.UtcNow,
                        ErrorMessage = string.IsNullOrEmpty(latestSnapshot.DependencyDetails)
                        ? "Sistem üst üste 3 kez yanıt vermedi."
                        : $"Sistem üst üste 3 kez yanıt vermedi: {latestSnapshot.DependencyDetails}"
                    };

                    await _incidentRepository.AddAsync(newIncident);

                    // Yöneticilere acil durum e-postası fırlat
                    await _notificationSender.SendDowntimeAlertAsync(newIncident, app);
                }
            }
            // Eğer sistem zaten "Kırmızı"daysa, düzelip düzelmediğine bak.
            else if (hasActiveIncident)
            {
                // Kural motoruna sor: Bu son log, sistemi düzeltir mi?
                if (IncidentRules.ShouldResolveIncident(latestSnapshot, hasActiveIncident))
                {
                    // Kesinti kaydı zaman damgasıyla kapatılır.
                    activeIncident.ResolvedAt = DateTime.UtcNow;

                    await _incidentRepository.UpdateAsync(activeIncident);

                    // Yöneticilere "Sistem kurtarıldı" e-postası at
                    await _notificationSender.SendRecoveryAlertAsync(activeIncident, app);
                }
            }
        }
    }
}
