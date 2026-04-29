using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;

namespace Watchdog.Application.UseCases.Apps
{
    // BU SINIF KALDIRILDI. REHBERLİK AMAÇLI TUTULUYOR.
    public class UpdateAppEmailsUseCase : IUseCaseAsync<UpdateAppEmailsRequest, (bool IsSuccess, string ErrorMessage)>
    {
        public Task<(bool IsSuccess, string ErrorMessage)> ExecuteAsync(UpdateAppEmailsRequest request)
        {
            return Task.FromResult((false, "E-posta özelliği sistemden kaldırıldı."));
        }
    }
}
