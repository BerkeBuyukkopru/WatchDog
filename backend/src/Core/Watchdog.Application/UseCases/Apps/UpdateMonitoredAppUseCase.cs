using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Enums;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Apps
{
    public class UpdateMonitoredAppUseCase : IUseCaseAsync<UpdateMonitoredAppRequest, UpdateMonitoredAppResponse>
    {
        private readonly IMonitoredAppRepository _appRepository;

        public UpdateMonitoredAppUseCase(IMonitoredAppRepository appRepository)
        {
            _appRepository = appRepository;
        }

        public async Task<UpdateMonitoredAppResponse> ExecuteAsync(UpdateMonitoredAppRequest request)
        {
            // 1. Uygulama var mı kontrol et?
            var app = await _appRepository.GetByIdAsync(request.Id);

            if (app == null)
            {
                return new UpdateMonitoredAppResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Güncellenecek uygulama bulunamadı.",
                    ErrorCode = AppErrorCode.AppNotFound
                };
            }

            // 2. URL değişmişse, yeni URL'in başka bir uygulamada olup olmadığını kontrol et
            if (app.HealthUrl != request.HealthUrl)
            {
                if (await _appRepository.IsUrlExistAsync(request.HealthUrl))
                {
                    return new UpdateMonitoredAppResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Bu Health URL zaten başka bir uygulama tarafından kullanılıyor.",
                        ErrorCode = AppErrorCode.UrlAlreadyExists
                    };
                }
            }

            // 3. Mapping: Gelen verileri veritabanı nesnesine (Entity) aktarıyoruz
            app.Name = request.Name;
            app.HealthUrl = request.HealthUrl;
            app.PollingIntervalSeconds = request.PollingIntervalSeconds;
            app.NotificationEmails = request.NotificationEmails;
            app.AdminEmail = request.AdminEmail;
            app.IsActive = request.IsActive;
            app.ActiveAiProviderId = request.ActiveAiProviderId;

            // 4. Veritabanında güncelle
            var result = await _appRepository.UpdateAsync(app);

            if (!result)
            {
                return new UpdateMonitoredAppResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Veritabanı güncellemesi sırasında teknik bir hata oluştu.",
                    ErrorCode = AppErrorCode.DatabaseError
                };
            }

            return new UpdateMonitoredAppResponse { IsSuccess = true };
        }
    }
}