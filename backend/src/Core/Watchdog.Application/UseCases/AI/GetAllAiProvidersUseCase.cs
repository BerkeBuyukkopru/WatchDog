using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.AI
{
    public class GetAllAiProvidersUseCase : IUseCaseAsync<GetAllAiProvidersRequest, IEnumerable<AiProviderDto>>
    {
        private readonly IAiProviderRepository _repository;

        public GetAllAiProvidersUseCase(IAiProviderRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AiProviderDto>> ExecuteAsync(GetAllAiProvidersRequest request)
        {
            var providers = await _repository.GetAllAsync();

            // Hassas verileri Backend'de saklayıp sadece gerekli alanları UI'a dönüyoruz.
            return providers.Select(p => new AiProviderDto
            {
                Id = p.Id,
                Name = p.Name,
                ModelName = p.ModelName,
                ApiUrl = p.ApiUrl,
                IsActive = p.IsActive,
                HasApiKey = !string.IsNullOrWhiteSpace(p.ApiKey) || p.Name.Contains("Ollama", StringComparison.OrdinalIgnoreCase)
            });
        }
    }
}
