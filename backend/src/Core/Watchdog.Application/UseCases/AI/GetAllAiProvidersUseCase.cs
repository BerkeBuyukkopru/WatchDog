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

            return providers.Select(p => new AiProviderDto
            {
                Id = p.Id,
                Name = p.Name,
                ModelName = p.ModelName,
                ApiUrl = p.ApiUrl,
                MaskedApiKey = MaskApiKey(p.ApiKey),
                IsActive = p.IsActive,
                HasApiKey = !string.IsNullOrWhiteSpace(p.ApiKey) || p.Name.Contains("Ollama", StringComparison.OrdinalIgnoreCase),
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                ModifiedAt = p.ModifiedAt,
                ModifiedBy = p.ModifiedBy,
                DeletedAt = p.DeletedAt,
                DeletedBy = p.DeletedBy
            });
        }

        private static string? MaskApiKey(string? apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) return null;
            if (apiKey.Length <= 8) return "••••";

            return $"{apiKey[..4]}••••{apiKey[^4..]}";
        }
    }
}
