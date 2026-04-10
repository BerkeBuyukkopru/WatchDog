using System.Text.RegularExpressions;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.Apps
{
    public class CreateMonitoredAppUseCase : IUseCaseAsync<CreateMonitoredAppRequest, CreateMonitoredAppResponse>
    {
        private readonly IMonitoredAppRepository _appRepository;

        public CreateMonitoredAppUseCase(IMonitoredAppRepository appRepository)
        {
            _appRepository = appRepository;
        }

        public async Task<CreateMonitoredAppResponse> ExecuteAsync(CreateMonitoredAppRequest request)
        {
            // URL formatı doğru mu? (Servisten buraya taşıdığımız kural)
            // İş Kuralı: Mailler doğru formatta mı? (Profesyonel Validasyon)
            // İş Kuralı: URL zaten var mı?
            if (await _appRepository.IsUrlExistAsync(request.HealthUrl))
            {
                return new CreateMonitoredAppResponse { IsSuccess = false, ErrorMessage = "Bu adres zaten izlenmektedir." };
            }

            // Entity Oluşturma ve API Key Üretimi
            var newApp = new MonitoredApp
            {
                Name = request.Name,
                HealthUrl = request.HealthUrl,
                PollingIntervalSeconds = request.PollingIntervalSeconds,
                NotificationEmails = request.NotificationEmails,
                // Profesyonel, güvenli ve benzersiz bir API Key üretimi
                ApiKey = "wdg_live_" + Guid.NewGuid().ToString("N").ToLower()
            };

            // Veritabanına Kaydet
            var success = await _appRepository.AddAsync(newApp);

            return new CreateMonitoredAppResponse
            {
                Id = newApp.Id,
                ApiKey = newApp.ApiKey,
                IsSuccess = success,
                ErrorMessage = success ? null : "Veritabanı kaydı sırasında bir hata oluştu."
            };
        }
    }
}