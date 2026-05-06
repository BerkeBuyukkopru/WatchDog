using System;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Apps
{
    public class ToggleAppStatusUseCase : IUseCaseAsync<ToggleAppStatusRequest, bool>
    {
        private readonly IMonitoredAppRepository _repository;

        public ToggleAppStatusUseCase(IMonitoredAppRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(ToggleAppStatusRequest request)
        {
            var app = await _repository.GetByIdAsync(request.Id);
            if (app == null) return false;

            app.IsActive = !app.IsActive;
            return await _repository.UpdateAsync(app);
        }
    }
}
