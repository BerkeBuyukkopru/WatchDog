using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.AI
{
    // Yönetim panelinden gelen "Aktif/Pasif" değişimini işler.
    // Sadece AiProvider tablosundaki IsActive alanını değiştirir.
    public class ToggleAiProviderStatusUseCase : IUseCaseAsync<Guid, bool>
    {
        private readonly IAiProviderRepository _repository;

        public ToggleAiProviderStatusUseCase(IAiProviderRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(Guid id)
        {
            var provider = await _repository.GetByIdAsync(id);
            if (provider == null) return false;

            provider.IsActive = !provider.IsActive;
            return await _repository.UpdateAsync(provider);
        }
    }
}
