using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Enums;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.Apps
{
    public class CreateMonitoredAppUseCase : IUseCaseAsync<CreateMonitoredAppRequest, CreateMonitoredAppResponse>
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly ILogger<CreateMonitoredAppUseCase> _logger;

        public CreateMonitoredAppUseCase(
            IMonitoredAppRepository appRepository,
            ILogger<CreateMonitoredAppUseCase> logger)
        {
            _appRepository = appRepository;
            _logger = logger;
        }

        public async Task<CreateMonitoredAppResponse> ExecuteAsync(CreateMonitoredAppRequest request)
        {
            // LOG: Süreci başlatıyoruz
            _logger.LogInformation("Yeni uygulama ekleme işlemi başlatıldı. Hedef URL: {HealthUrl}", request.HealthUrl);

            // 1. KONTROL: İş Kuralı - URL zaten sistemde aktif olarak var mı?
            if (await _appRepository.IsUrlExistAsync(request.HealthUrl))
            {
                _logger.LogWarning("İşlem reddedildi. Bu URL sistemde zaten mevcut: {HealthUrl}", request.HealthUrl);

                return new CreateMonitoredAppResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Bu adres zaten izlenmektedir.",
                    ErrorCode = AppErrorCode.UrlAlreadyExists
                };
            }

            // 2. OLUŞTURMA: Yeni Domain Entity'yi hazırlıyoruz
            var newApp = new MonitoredApp
            {
                Name = request.Name,
                HealthUrl = request.HealthUrl,
                PollingIntervalSeconds = request.PollingIntervalSeconds,

                // YENİ GÜNCELLEME: Mail kısımları (NotificationEmails ve AdminEmail) buradan tamamen silindi!

                // Profesyonel, güvenli ve benzersiz bir API Key üretimi
                ApiKey = "wdg_live_" + Guid.NewGuid().ToString("N").ToLower(),

                // Yeni uygulama sisteme aktif olarak dahil olmalı
                IsActive = true
            };

            // 3. KAYIT: Repository üzerinden veritabanına yazıyoruz
            var success = await _appRepository.AddAsync(newApp);

            if (success)
            {
                _logger.LogInformation("Uygulama başarıyla veritabanına eklendi. Yeni ID: {AppId}", newApp.Id);

                return new CreateMonitoredAppResponse
                {
                    Id = newApp.Id,
                    ApiKey = newApp.ApiKey,
                    IsSuccess = true,
                    ErrorCode = AppErrorCode.None
                };
            }
            else
            {
                _logger.LogError("Veritabanı kaydı sırasında teknik bir hata oluştu. URL: {HealthUrl}", request.HealthUrl);

                return new CreateMonitoredAppResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Veritabanı kaydı sırasında bir hata oluştu.",
                    ErrorCode = AppErrorCode.DatabaseError
                };
            }
        }
    }
}