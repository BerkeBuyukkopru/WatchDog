using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
    // Yapay Zekanın ürettiği tavsiyelerin (Insight) veritabanına kaydedilmesi ve okunması için gerekli sözleşme.
    public interface IAiInsightRepository
    {
        // AI tarafından üretilen analizi veritabanına kaydeder.
        Task AddAsync(AiInsight insight);

        // Listeleme (appId null gelirse her şeyi, dolu gelirse o uygulamayı getirir)
        Task<IEnumerable<AiInsight>> GetByAppIdAsync(Guid? appId);
    }
}
