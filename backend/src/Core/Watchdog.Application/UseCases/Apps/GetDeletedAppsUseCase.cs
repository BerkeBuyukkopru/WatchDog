using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Constants;

namespace Watchdog.Application.UseCases.Apps
{
    public class GetDeletedAppsUseCase : IUseCaseAsync<GetDeletedAppsRequest, IEnumerable<AppDto>>
    {
        private readonly IMonitoredAppRepository _repository;
        private readonly ICurrentUserService _currentUserService;

        public GetDeletedAppsUseCase(
            IMonitoredAppRepository repository,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<AppDto>> ExecuteAsync(GetDeletedAppsRequest request)
        {
            var deletedApps = await _repository.GetAllDeletedAsync();

            return deletedApps.Select(a => new AppDto
            {
                Id = a.Id,
                Name = a.Name,
                HealthUrl = a.HealthUrl,
                PollingIntervalSeconds = a.PollingIntervalSeconds,
                CreatedAt = a.CreatedAt,
                ActiveAiProviderId = a.ActiveAiProviderId,
                IsActive = a.IsActive
            });
        }
    }
}
