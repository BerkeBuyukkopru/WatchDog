using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.SystemConfig;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.SystemConfig
{
    public class UpdateSystemConfigUseCase : IUseCaseAsync<SystemConfigDto, bool>
    {
        private readonly ISystemConfigurationRepository _repository;

        public UpdateSystemConfigUseCase(ISystemConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(SystemConfigDto request)
        {
            var existingConfig = await _repository.GetAsync();

            if (existingConfig == null)
            {
                existingConfig = new SystemConfiguration { Id = 1 };
            }

            existingConfig.ActiveAiProvider = request.ActiveAiProvider;

            // Gelen model ismini veritabanına kaydediyoruz. Eğer boş gönderilirse varsayılan olarak "phi3" kabul ediyoruz.
            existingConfig.AiModelName = string.IsNullOrWhiteSpace(request.AiModelName) ? "phi3" : request.AiModelName;

            existingConfig.AiApiUrl = request.AiApiUrl;
            existingConfig.AiApiKey = request.AiApiKey;
            existingConfig.CriticalCpuThreshold = request.CriticalCpuThreshold;
            existingConfig.CriticalRamThreshold = request.CriticalRamThreshold;
            existingConfig.LastUpdated = DateTime.UtcNow;

            return await _repository.UpdateAsync(existingConfig);
        }
    }
}
