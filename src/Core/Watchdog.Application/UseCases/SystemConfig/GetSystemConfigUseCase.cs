using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.SystemConfig;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.SystemConfig
{
    public class GetSystemConfigUseCase : IUseCaseAsync<GetSystemConfigRequest, SystemConfigDto?>
    {
        private readonly ISystemConfigurationRepository _repository;

        public GetSystemConfigUseCase(ISystemConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<SystemConfigDto?> ExecuteAsync(GetSystemConfigRequest request)
        {
            var config = await _repository.GetAsync();

            if (config == null) return null;

            return new SystemConfigDto
            {
                // YENİ MİMARİ: Sadece eşik değerleri (Thresholds) taşınıyor.
                CriticalCpuThreshold = config.CriticalCpuThreshold,
                CriticalRamThreshold = config.CriticalRamThreshold,
                CriticalLatencyThreshold = config.CriticalLatencyThreshold
            };
        }
    }
}