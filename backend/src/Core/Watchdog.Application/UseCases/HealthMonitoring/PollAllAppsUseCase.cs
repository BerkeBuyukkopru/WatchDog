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
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IPromptBuilder _promptBuilder;

        public PollAllAppsUseCase(
            IMonitoredAppRepository appRepository,
            IUseCaseAsync<PollSingleAppRequest, HealthSnapshot?> pollSingleUseCase,
            ISnapshotRepository snapshotRepository,
            IPromptBuilder promptBuilder)
        {
            _appRepository = appRepository;
            _pollSingleUseCase = pollSingleUseCase;
            _snapshotRepository = snapshotRepository;
            _promptBuilder = promptBuilder;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var apps = await _appRepository.GetAllAsync();

            foreach (var app in apps)
            {
                var request = new PollSingleAppRequest
                {
                    AppId = app.Id,
                    CancellationToken = cancellationToken
                };

                await _pollSingleUseCase.ExecuteAsync(request);

                // Haftalık (7 gün) yerine artık Aylık (30 gün) veriyi AI'a paslıyoruz
                var monthlyData = await _snapshotRepository.GetDailyEnrichedSnapshotsAsync(app.Id, 30);
                
                if (monthlyData.Count >= 2)
                {
                    var baseline = monthlyData.Last(); // 30 gün önceki durum
                    var yesterday = monthlyData.First(); // Dünkü durum
                    
                    var avgCpu = monthlyData.Average(x => x.AvgCpu);
                    var avgRam = monthlyData.Average(x => x.AvgRam);
                    
                    var baselineErrors = string.Join(" | ", baseline.TopErrors);
                    var yesterdayErrors = string.Join(" | ", yesterday.TopErrors);

                    var strategicPrompt = _promptBuilder.BuildStrategicPrompt(app, baseline, yesterday, avgCpu, avgRam, baselineErrors, yesterdayErrors);
                }
            }
        }
    }
}