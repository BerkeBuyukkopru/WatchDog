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
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public CreateMonitoredAppUseCase(IMonitoredAppRepository appRepository)
        {
            _appRepository = appRepository;
        }

        public async Task<CreateMonitoredAppResponse> ExecuteAsync(CreateMonitoredAppRequest request)
        {
            // URL formatı doğru mu? (Servisten buraya taşıdığımız kural)
            if (!Uri.TryCreate(request.HealthUrl, UriKind.Absolute, out _))
            {
                return new CreateMonitoredAppResponse { IsSuccess = false, ErrorMessage = "Lütfen geçerli bir URL giriniz. (örn: https://...)" };
            }

            // 1. İş Kuralı: URL zaten var mı?
            if (await _appRepository.IsUrlExistAsync(request.HealthUrl))
            {
                return new CreateMonitoredAppResponse { IsSuccess = false, ErrorMessage = "Bu URL zaten kayıtlı." };
            }

            // 2. İş Kuralı: Mailler doğru formatta mı? (Profesyonel Validasyon)
            if (!string.IsNullOrWhiteSpace(request.NotificationEmails))
            {
                var emails = request.NotificationEmails.Split(',', ';');
                foreach (var email in emails)
                {
                    if (!EmailRegex.IsMatch(email.Trim()))
                    {
                        return new CreateMonitoredAppResponse { IsSuccess = false, ErrorMessage = $"Geçersiz e-posta formatı: {email}" };
                    }
                }
            }

            // 3. Entity Oluşturma ve API Key Üretimi
            var newApp = new MonitoredApp
            {
                Name = request.Name,
                HealthUrl = request.HealthUrl,
                PollingIntervalSeconds = request.PollingIntervalSeconds,
                NotificationEmails = request.NotificationEmails,
                // Profesyonel, güvenli ve benzersiz bir API Key üretimi
                ApiKey = Guid.NewGuid().ToString("N").ToUpper()
            };

            // 4. Veritabanına Kaydet
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