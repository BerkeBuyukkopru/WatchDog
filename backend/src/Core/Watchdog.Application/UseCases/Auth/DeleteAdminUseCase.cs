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
            // İŞ KURALI: Eğer projen tek bir admin ile kalacaksa burada bir kontrol eklenebilir.
            // Şimdilik repository'ye silme emrini gönderiyoruz.
            return await _authRepository.DeleteUserAsync(adminId);
        }
    }
}
