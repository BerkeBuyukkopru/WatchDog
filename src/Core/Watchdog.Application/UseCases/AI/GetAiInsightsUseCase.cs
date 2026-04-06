using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.AI
{
    public class GetAiInsightsUseCase : IUseCaseAsync<Guid?, IEnumerable<AiInsightDto>>
    {
        private readonly IAiInsightRepository _insightRepository;

        public GetAiInsightsUseCase(IAiInsightRepository insightRepository)
        {
            _insightRepository = insightRepository;
        }

        public async Task<IEnumerable<AiInsightDto>> ExecuteAsync(Guid? appId)
        {
            // 1. Repository'den verileri çek (Virtual App property'sinin dolu geldiğinden emin olacağız)
            var insights = await _insightRepository.GetByAppIdAsync(appId);

            // 2. Entity listesini DTO listesine "Map" et
            return insights.Select(i => new AiInsightDto
            {
                Id = i.Id,
                AppName = i.App?.Name ?? "Sistem Geneli",
                Message = i.Message,
                Evidence = i.Evidence,
                InsightType = i.InsightType.ToString(), // UI tarafı için string dönüşümü
                IsResolved = i.IsResolved,
                CreatedAt = i.CreatedAt
            });
        }
    }
}
