using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    public class GetLatestStatusesUseCase : IUseCaseAsync<GetLatestStatusesRequest, IEnumerable<LatestStatusDto>>
    {
        private readonly ISnapshotRepository _snapshotRepository;

        public GetLatestStatusesUseCase(ISnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
        }

        public async Task<IEnumerable<LatestStatusDto>> ExecuteAsync(GetLatestStatusesRequest request)
        {
            IEnumerable<HealthSnapshot> snapshots;

            // 1. Veriyi Repository'den (Veritabanından) al
            // Gelen istekte AppId varsa sadece o uygulamayı, yoksa tüm sistemi getir.
            if (request.AppId.HasValue && request.AppId.Value != System.Guid.Empty)
            {
                snapshots = await _snapshotRepository.GetLatestSnapshotsAsync(request.AppId.Value, request.Count);
            }
            else
            {
                snapshots = await _snapshotRepository.GetLatestGlobalAsync(request.Count);
            }

            // 2. Ham entity'leri, React için temiz DTO'lara dönüştür (Mapping)
            var dtoList = snapshots.Select(x => new LatestStatusDto
            {
                Id = x.Id,
                AppId = x.AppId,
                AppName = x.App?.Name ?? "Bilinmeyen Uygulama",
                Status = x.Status.ToString(), // Enum'ı string'e ("Healthy") çevirir
                TotalDuration = x.TotalDuration,
                Timestamp = x.Timestamp,
                CpuUsage = x.CpuUsage,
                RamUsage = x.RamUsage,
                FreeDiskGb = x.FreeDiskGb
            })
            .OrderBy(x => x.Timestamp) // React grafiği için zaman çizgisine göre sırala
            .ToList();

            return dtoList;
        }
    }
}