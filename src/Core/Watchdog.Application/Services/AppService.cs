using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Services
{
    //Bussines Logic, Veritabanına veri kaydetme işlemleri. Veritabanı (Repository) ile dış dünya (API) arasındaki köprüdür.
    public class AppService : IAppService
    {
        private readonly IMonitoredAppRepository _repository;

        // Dependency Injection: Repository buraya enjekte ediliyor.
        public AppService(IMonitoredAppRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AppDto>> GetAllAppsAsync()
        {
            //Tüm uygulamaları veritabanından çekiyoruz.
            var apps = await _repository.GetAllAsync();

            //Entity'yi (MonitoredApp) DTO'ya (AppDto) çeviriyoruz.
            return apps.Select(a => new AppDto
            {
                Id = a.Id,
                Name = a.Name,
                HealthUrl = a.HealthUrl,
                PollingIntervalSeconds = a.PollingIntervalSeconds,
                CreatedAt = a.CreatedAt,
                // YENİ EKLENEN: Veritabanındaki virgüllü mail metnini (null ise boş string) DTO'ya aktarıyoruz
                NotificationEmails = a.NotificationEmails ?? string.Empty
            });
        }

        public async Task<(bool IsSuccess, string ErrorMessage, string ErrorCode, string ApiKey, Guid? Id)> AddAppAsync(CreatedAppDto dto)
        {
            // Uri: URL'nin üst formudur. Bir kaynağı tanımlayan her şeye denir.
            // Uri.TryCreate: URL'nin kurallara uygun olup olmadığını sorgular. Eğer geçersizse hata fırlatmak yerine false döner.
            // UriKind.Absolute: Sistemin sadece tam adresleri (içinde http:// veya https:// olanları) kabul etmesini sağlar. Kullanıcı sadece "/api/health" yazarsa, Absolute kuralı sayesinde sistem bunu reddeder.
            if (!Uri.TryCreate(dto.HealthUrl, UriKind.Absolute, out _))
            {
                return (false, "Lütfen geçerli bir URL giriniz.", "INVALID_URL", string.Empty, null);
            }

            bool isExists = await _repository.IsUrlExistAsync(dto.HealthUrl);
            if (isExists)
            {
                return (false, "Bu adres zaten izlenmektedir.", "URL_ALREADY_EXISTS", string.Empty, null);
            }

            var newApp = new MonitoredApp
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                HealthUrl = dto.HealthUrl,
                PollingIntervalSeconds = dto.PollingIntervalSeconds,
                CreatedAt = DateTime.UtcNow,
                // Uygulamaya özel API Key üretiyoruz.
                ApiKey = Guid.NewGuid().ToString("N")
            };

            // Repository üzerinden veritabanına yazıyoruz.
            bool result = await _repository.AddAsync(newApp);

            if (result)
            {
                // Başarılıysa Dashboard'a 'ApiKey' bilgisini tek seferlik gösterilmek üzere dönüyoruz.
                return (true, string.Empty, string.Empty, newApp.ApiKey, newApp.Id);
            }

            return (false, "Kayıt sırasında veritabanı hatası oluştu.", "DB_ERROR", string.Empty, null);
        }

        public async Task<bool> DeleteAppAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
