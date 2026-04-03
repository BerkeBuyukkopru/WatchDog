using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.Apps;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Apps
{
    public class DeleteAppUseCase : IUseCaseAsync<DeleteAppRequest, bool>
    {
        private readonly IMonitoredAppRepository _repository;

        public DeleteAppUseCase(IMonitoredAppRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(DeleteAppRequest request)
        {
            return await _repository.DeleteAsync(request.Id);
        }
    }
}
