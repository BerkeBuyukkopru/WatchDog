using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.AI
{

    // Dashboard üzerinden gelen AI yapılandırma güncellemelerini işler.
    public class UpdateAiProviderUseCase : IUseCaseAsync<UpdateAiProviderRequest, bool>
    {
        private readonly IAiProviderRepository _repository;

        public UpdateAiProviderUseCase(IAiProviderRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(UpdateAiProviderRequest request)
        {
            // 1. Güncellenmek istenen sağlayıcıyı bul
            var provider = await _repository.GetByIdAsync(request.Id);
            if (provider == null) return false;

            // 2. Sadece değişebilir alanları ez (Name alanı kurumsal kimliktir, değiştirilmez)
            provider.ModelName = request.ModelName;
            provider.ApiUrl = request.ApiUrl;

            // API Key sadece boş değilse güncellenir (Güvenlik: Mevcut anahtarı yanlışlıkla silmemek için)
            if (!string.IsNullOrWhiteSpace(request.ApiKey))
            {
                provider.ApiKey = request.ApiKey;
            }

            // 3. Veritabanına mühürle
            return await _repository.UpdateAsync(provider);
        }
    }
}
