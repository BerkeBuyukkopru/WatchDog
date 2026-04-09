using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    // Yapay Zekanın ürettiği tavsiyelerin (Insight) veritabanına kaydedilmesi ve okunması için gerekli sözleşme.
    // Yapay Zekanın ürettiği tavsiyelerin (Insight) veritabanına kaydedilmesi ve okunması için gerekli sözleşme.
    public interface IAiInsightRepository
    {
        Task AddAsync(AiInsight insight);
        Task<IEnumerable<AiInsight>> GetByAppIdAsync(Guid? appId);

        Task<AiInsight?> GetLatestInsightAsync(Guid appId);
    }
}
