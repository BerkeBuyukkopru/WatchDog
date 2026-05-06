using System;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Apps
{
    public class RestoreAppUseCase : IUseCaseAsync<RestoreAppRequest, bool>
    {
        private readonly IMonitoredAppRepository _repository;

        public RestoreAppUseCase(IMonitoredAppRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(RestoreAppRequest request)
        {
            return await _repository.RestoreAsync(request.Id);
        }
    }
}
