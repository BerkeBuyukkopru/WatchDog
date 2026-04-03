using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Apps
{
    public class GetAllAppsUseCase : IUseCaseAsync<GetAllAppsRequest, IEnumerable<AppDto>>
    {
        private readonly IMonitoredAppRepository _repository;

        public GetAllAppsUseCase(IMonitoredAppRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AppDto>> ExecuteAsync(GetAllAppsRequest request)
        {
            var apps = await _repository.GetAllAsync();

            return apps.Select(a => new AppDto
            {
                Id = a.Id,
                Name = a.Name,
                HealthUrl = a.HealthUrl,
                PollingIntervalSeconds = a.PollingIntervalSeconds,
                CreatedAt = a.CreatedAt,
                NotificationEmails = a.NotificationEmails ?? string.Empty
            });
        }
    }
}