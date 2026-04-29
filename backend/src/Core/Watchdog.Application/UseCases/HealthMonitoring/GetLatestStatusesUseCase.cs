using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration; // YENİ

        // Bağımlılıklar içeri alınıyor
        public GetLatestStatusesUseCase(
            ISnapshotRepository snapshotRepository,
            IAuthRepository authRepository,
            ICurrentUserService currentUserService,
            IConfiguration configuration)
        {
            _snapshotRepository = snapshotRepository;
            _authRepository = authRepository;
            _currentUserService = currentUserService;
            _configuration = configuration;
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

            // Sistem donanım sınırlarını Config'den okuyalım (yoksa fallback)
            double totalRamMb = Convert.ToDouble(_configuration["SystemMetrics:TotalRamMb"] ?? "16384");
            double totalDiskGb = Convert.ToDouble(_configuration["SystemMetrics:TotalDiskGb"] ?? "500");
            double totalCpuPercentage = Convert.ToDouble(_configuration["SystemMetrics:TotalCpuPercentage"] ?? "100");
            int totalCpuCores = Convert.ToInt32(_configuration["SystemMetrics:TotalCpuCores"] ?? "16");

            // 3. Ham entity'leri, React için temiz DTO'lara dönüştür (Mapping)
            var dtoList = snapshots.Select(x => new LatestStatusDto
            {
                Id = x.Id,
                AppId = x.AppId,
                AppName = x.App?.Name ?? "Bilinmeyen Uygulama",
                Status = x.Status.ToString(),
                TotalDuration = x.TotalDuration,
                Timestamp = x.Timestamp,
                AppCpuUsage = x.AppCpuUsage,
                SystemCpuUsage = x.SystemCpuUsage,
                AppRamUsage = x.AppRamUsage,
                SystemRamUsage = x.SystemRamUsage,
                FreeDiskGb = x.FreeDiskGb,
                DependencyDetails = x.DependencyDetails,
                TotalRamMb = totalRamMb,
                TotalCpuPercentage = totalCpuPercentage,
                TotalDiskGb = totalDiskGb,
                TotalCpuCores = totalCpuCores
            })
            .OrderBy(x => x.Timestamp)
            .ToList();

            return dtoList;
        }
    }
}