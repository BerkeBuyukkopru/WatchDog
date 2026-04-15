using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.Monitoring;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Constants; // 🚨 Rol kontrolü için eklendi
using Watchdog.Domain.Entities;

namespace Watchdog.Application.UseCases.HealthMonitoring
{
    public class GetLatestStatusesUseCase : IUseCaseAsync<GetLatestStatusesRequest, IEnumerable<LatestStatusDto>>
    {
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IAuthRepository _authRepository; // YENİ
        private readonly ICurrentUserService _currentUserService; // YENİ

        // Bağımlılıklar içeri alınıyor
        public GetLatestStatusesUseCase(
            ISnapshotRepository snapshotRepository,
            IAuthRepository authRepository,
            ICurrentUserService currentUserService)
        {
            _snapshotRepository = snapshotRepository;
            _authRepository = authRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<LatestStatusDto>> ExecuteAsync(GetLatestStatusesRequest request)
        {
            IEnumerable<HealthSnapshot> snapshots;

            // 1. Veriyi Repository'den (Veritabanından) al
            if (request.AppId.HasValue && request.AppId.Value != System.Guid.Empty)
            {
                snapshots = await _snapshotRepository.GetLatestSnapshotsAsync(request.AppId.Value, request.Count);
            }
            else
            {
                snapshots = await _snapshotRepository.GetLatestGlobalAsync(request.Count);
            }

            // 2. YETKİ KONTROLÜ (GÜVENLİK DUVARI)
            var currentRole = _currentUserService.Role;

            // Eğer SuperAdmin değilse, sadece izinli olduklarını göster
            if (currentRole != RoleConstants.SuperAdmin)
            {
                Guid userId = _currentUserService.UserId;

                if (userId != Guid.Empty)
                {
                    var currentAdmin = await _authRepository.GetByIdAsync(userId);

                    if (currentAdmin != null && currentAdmin.AllowedAppIds != null && currentAdmin.AllowedAppIds.Any())
                    {
                        // 🚨 Sadece yetkisi olan uygulamaları filtrele
                        snapshots = snapshots.Where(x => currentAdmin.AllowedAppIds.Contains(x.AppId));
                    }
                    else
                    {
                        // Adminin yetkisi yoksa veya liste boşsa, güvenlik gereği boş veri dön
                        return new List<LatestStatusDto>();
                    }
                }
                else
                {
                    return new List<LatestStatusDto>();
                }
            }

            // 3. Ham entity'leri, React için temiz DTO'lara dönüştür (Mapping)
            var dtoList = snapshots.Select(x => new LatestStatusDto
            {
                Id = x.Id,
                AppId = x.AppId,
                AppName = x.App?.Name ?? "Bilinmeyen Uygulama",
                Status = x.Status.ToString(),
                TotalDuration = x.TotalDuration,
                Timestamp = x.Timestamp,
                CpuUsage = x.CpuUsage,
                RamUsage = x.RamUsage,
                FreeDiskGb = x.FreeDiskGb
            })
            .OrderBy(x => x.Timestamp)
            .ToList();

            return dtoList;
        }
    }
}