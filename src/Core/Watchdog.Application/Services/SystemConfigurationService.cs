using System;
using System.Threading.Tasks;
using Watchdog.Application.DTOs;
using Watchdog.Application.Interfaces;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Services
{
    // Dashboard'daki 'Ayarlar' sayfasının iş mantığını (Logic) yönetir.
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly ISystemConfigurationRepository _repository;

        // Veritabanı teknik detaylarını (SQL/NoSQL) bilmez, sadece interface ile konuşur.
        public SystemConfigurationService(ISystemConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<SystemConfigDto?> GetConfigAsync()
        {
            // Veritabanındaki tekil (Singleton-like) ayar kaydını getirir.
            var config = await _repository.GetAsync();

            if (config == null) return null;

            // MAPPING: Domain Entity nesnesini DTO'ya çeviriyoruz. Bu sayede veritabanı şeması Dashboard'a sızmaz, sadece DTO'daki alanlar gider
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
            // Mevcut ayarları çekiyoruz.
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