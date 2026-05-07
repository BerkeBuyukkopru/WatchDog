using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.AI
{
    // Dashboard üzerinden gelen "Yapay Zekayı Aktif Et" isteğini işleyen kural seti.
    // Kurumsal standart gereği kimlik tipi Guid olarak güncellenmiştir.
    // Giriş tipi int yerine Guid, dönüş tipi işlemin başarısını belirten bool.
    public class SetActiveAiProviderUseCase : IUseCaseAsync<Guid, bool>
    {
        private readonly IAiProviderRepository _providerRepository;
        private readonly IMonitoredAppRepository _appRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ICurrentUserService _currentUserService;

        public SetActiveAiProviderUseCase(
            IAiProviderRepository providerRepository,
            IMonitoredAppRepository appRepository,
            IAuthRepository authRepository,
            ICurrentUserService currentUserService)
        {
            _providerRepository = providerRepository;
            _appRepository = appRepository;
            _authRepository = authRepository;
            _currentUserService = currentUserService;
        }

        public async Task<bool> ExecuteAsync(Guid id)
        {
            // 1. Sağlayıcıyı al ve durumunu tersine çevir (Toggle)
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null) return false;

            provider.IsActive = !provider.IsActive;
            await _providerRepository.UpdateAsync(provider);

            // Eğer sağlayıcı kapatıldıysa (IsActive = false), uygulama eşleştirmelerini değiştirmeye gerek yok.
            // (Zaten inaktif olan bir sağlayıcı analiz sırasında Fallback mekanizmasına takılacaktır)
            if (!provider.IsActive) return true;

            // 2. Kullanıcının rolünü ve kimliğini al
            var currentRole = _currentUserService.Role;
            var userId = _currentUserService.UserId;

            IEnumerable<MonitoredApp> targetApps;

            // 3. Hangi uygulamaların güncelleneceğine karar ver
            if (currentRole == Watchdog.Domain.Constants.RoleConstants.SuperAdmin)
            {
                // SuperAdmin her şeyi değiştirir
                targetApps = await _appRepository.GetAllAsync();
            }
            else
            {
                // Normal Admin sadece kendi sorumlu olduğu uygulamaları değiştirir
                var currentAdmin = await _authRepository.GetByIdAsync(userId);
                if (currentAdmin == null || currentAdmin.AllowedAppIds == null || !currentAdmin.AllowedAppIds.Any())
                {
                    return true;
                }

                var allApps = await _appRepository.GetAllAsync();
                targetApps = allApps.Where(app => currentAdmin.AllowedAppIds.Contains(app.Id)).ToList();
            }

            // 4. Hedef uygulamaların AI motorunu güncelle (SADECE aktif hale getirildiyse)
            foreach (var app in targetApps)
            {
                app.ActiveAiProviderId = id;
                await _appRepository.UpdateAsync(app);
            }

            return true;
        }
    }
}
