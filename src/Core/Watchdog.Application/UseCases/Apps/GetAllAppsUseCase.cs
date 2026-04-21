using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Constants; // RoleConstants için

namespace Watchdog.Application.UseCases.Apps
{
    public class GetAllAppsUseCase : IUseCaseAsync<GetAllAppsRequest, IEnumerable<AppDto>>
    {
        private readonly IMonitoredAppRepository _repository;
        private readonly IAuthRepository _authRepository;
        private readonly ICurrentUserService _currentUserService;

        // DI Container'dan CurrentUserService ve AuthRepository'yi istiyoruz.
        public GetAllAppsUseCase(
            IMonitoredAppRepository repository,
            IAuthRepository authRepository,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _authRepository = authRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<AppDto>> ExecuteAsync(GetAllAppsRequest request)
        {
            // 1. Tüm uygulamaları veritabanından çek.
            var allApps = await _repository.GetAllAsync();

            // 2. İsteği yapan kullanıcının rolünü al.
            var currentRole = _currentUserService.Role;

            // 3. EĞER kullanıcı SuperAdmin DEĞİLSE filtreleme yap.
            if (currentRole != RoleConstants.SuperAdmin)
            {
                // UserId artık direkt Guid geldiği için Parse etmeye gerek yok!
                Guid userId = _currentUserService.UserId;

                if (userId != Guid.Empty)
                {
                    var currentAdmin = await _authRepository.GetByIdAsync(userId);

                    if (currentAdmin != null && currentAdmin.AllowedAppIds != null && currentAdmin.AllowedAppIds.Any())
                    {
                        // Sadece yetkisi olan uygulamaları filtrele.
                        allApps = allApps.Where(app => currentAdmin.AllowedAppIds.Contains(app.Id)).ToList();
                    }
                    else
                    {
                        // Eğer adminin hiçbir uygulamaya yetkisi yoksa veya bilgisi bulunamadıysa boş liste dön.
                        return new List<AppDto>();
                    }
                }
            }

            // 4. Sonuçları DTO'ya dönüştür. (NotificationEmails silindi!)
            return allApps.Select(a => new AppDto
            {
                Id = a.Id,
                Name = a.Name,
                HealthUrl = a.HealthUrl,
                PollingIntervalSeconds = a.PollingIntervalSeconds,
                CreatedAt = a.CreatedAt
            });
        }
    }
}