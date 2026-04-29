using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Constants; // 🚨 Rol kontrolü için eklendi

namespace Watchdog.Application.UseCases.AI
{
    public class GetAiInsightsUseCase
    {
        private readonly IAiInsightRepository _insightRepository;
        private readonly IAuthRepository _authRepository; // YENİ
        private readonly ICurrentUserService _currentUserService; // YENİ

        // Bağımlılıklar içeri alınıyor
        public GetAiInsightsUseCase(
            IAiInsightRepository insightRepository,
            IAuthRepository authRepository,
            ICurrentUserService currentUserService)
        {
            _insightRepository = insightRepository;
            _authRepository = authRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<AiInsightDto>> ExecuteAsync(Guid? appId, int limit = 5)
        {
            // 1. Repository'den verileri çek 
            var insights = await _insightRepository.GetByAppIdAsync(appId);

            // 2. YETKİ KONTROLÜ (GÜVENLİK DUVARI)
            var currentRole = _currentUserService.Role;

            if (currentRole != RoleConstants.SuperAdmin)
            {
                Guid userId = _currentUserService.UserId;

                if (userId != Guid.Empty)
                {
                    var currentAdmin = await _authRepository.GetByIdAsync(userId);

                    if (currentAdmin != null && currentAdmin.AllowedAppIds != null && currentAdmin.AllowedAppIds.Any())
                    {
                        // 🚨 Sadece adminin yetkili olduğu AppId'ye sahip analizleri getir
                        insights = insights.Where(i => currentAdmin.AllowedAppIds.Contains(i.AppId));
                    }
                    else
                    {
                        Console.WriteLine($">>>> [WARN] GetAiInsights: Admin ({userId}) için yetkili uygulama bulunamadı veya admin kaydı eksik.");
                        return new List<AiInsightDto>();
                    }
                }
                else
                {
                    return new List<AiInsightDto>();
                }
            }

            // 3. Entity listesini DTO listesine "Map" et. (Sadece çözülmemiş olanları getir)
            return insights.Where(i => !i.IsResolved).Select(i => new AiInsightDto
            {
                Id = i.Id,
                AppName = i.App?.Name ?? "Sistem Geneli",
                Message = i.Message,
                Evidence = i.Evidence,
                InsightType = i.InsightType.ToString(),
                IsResolved = i.IsResolved,
                CreatedAt = i.CreatedAt
            }).OrderByDescending(i => i.CreatedAt).Take(limit).ToList(); // Sadece son 5 kaydı dön
        }
    }
}