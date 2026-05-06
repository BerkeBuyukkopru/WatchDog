using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Auth
{
    // Bir admini sistemden uzaklaştırma (Soft Delete) senaryosu.
    public class DeleteAdminUseCase : IUseCaseAsync<Guid, bool>
    {
        private readonly IAuthRepository _authRepository;

        public DeleteAdminUseCase(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<bool> ExecuteAsync(Guid adminId)
        {
            // 1. Silinecek kullanıcıyı bul
            var admin = await _authRepository.GetByIdAsync(adminId);
            if (admin == null || admin.IsDeleted) return false;

            // 2. 🛡️ SON SUPERADMIN KORUMASI
            if (admin.Role == Watchdog.Domain.Constants.RoleConstants.SuperAdmin)
            {
                var superAdminCount = await _authRepository.GetActiveSuperAdminCountAsync();
                if (superAdminCount <= 1)
                {
                    throw new InvalidOperationException("İşlem reddedildi: Sistemde en az bir adet SuperAdmin kalmak zorundadır.");
                }
            }

            // 3. Güvenlik testleri geçildiyse silme (dondurma) işlemini uygula
            return await _authRepository.DeleteUserAsync(adminId);
        }
    }
}
