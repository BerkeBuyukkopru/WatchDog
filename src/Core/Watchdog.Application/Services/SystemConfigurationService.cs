using System;
using System.Threading.Tasks;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Services
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly ISystemConfigurationRepository _repository;

        public SystemConfigurationService(ISystemConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<SystemConfigDto?> GetConfigAsync()
        {
            var config = await _repository.GetAsync();

            if (config == null) return null;

            // Entity nesnesini, React'in anlayacağı DTO'ya çeviriyoruz
            return new SystemConfigDto
            {
                ActiveAiProvider = config.ActiveAiProvider,
                AiApiUrl = config.AiApiUrl,
                AiApiKey = config.AiApiKey,
                CriticalCpuThreshold = config.CriticalCpuThreshold,
                CriticalRamThreshold = config.CriticalRamThreshold
            };
        }

        public async Task<bool> UpdateConfigAsync(SystemConfigDto dto)
        {
            var existingConfig = await _repository.GetAsync();

            // Eğer veritabanında hiç ayar yoksa (Seed data çalışmamışsa) yeni oluşturur
            if (existingConfig == null)
            {
                existingConfig = new SystemConfiguration { Id = 1 };
            }

            // DTO'dan gelen yeni değerleri Entity'ye aktarıyoruz
            existingConfig.ActiveAiProvider = dto.ActiveAiProvider;
            existingConfig.AiApiUrl = dto.AiApiUrl;
            existingConfig.AiApiKey = dto.AiApiKey;
            existingConfig.CriticalCpuThreshold = dto.CriticalCpuThreshold;
            existingConfig.CriticalRamThreshold = dto.CriticalRamThreshold;
            existingConfig.LastUpdated = DateTime.UtcNow;

            return await _repository.UpdateAsync(existingConfig);
        }
    }
}