using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    public class PollAllAppsUseCase
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?> _pollSingleUseCase;

        public PollAllAppsUseCase(
            IMonitoredAppRepository appRepository,
            IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?> pollSingleUseCase)
        {
            _appRepository = appRepository;
            _pollSingleUseCase = pollSingleUseCase;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var apps = await _appRepository.GetAllAsync();

            foreach (var app in apps)
            {
                // Her uygulama için tekli tarama isteği oluşturuluyor
                var request = new PollSingleAppRequest
                {
                    AppId = app.Id,
                    CancellationToken = cancellationToken
                };

                // Arka planda beklemeden (fire-and-forget) veya sıralı çalıştırılabilir
                await _pollSingleUseCase.ExecuteAsync(request);
            }
        }
    }
}