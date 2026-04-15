using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Auth
{
    // Silinmiş (Soft Delete olmuş) bir admini tekrar sisteme dahil etme senaryosu.
    public class RestoreAdminUseCase : IUseCaseAsync<Guid, bool>
    {
        private readonly IAuthRepository _authRepository;

        public RestoreAdminUseCase(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<bool> ExecuteAsync(Guid adminId)
        {
            // İşi doğrudan repository'ye devrediyoruz.
            return await _authRepository.RestoreUserAsync(adminId);
        }
    }
}
