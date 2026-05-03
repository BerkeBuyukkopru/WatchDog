using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Application.UseCases.AI
{
    // AI tarafından üretilen kriz veya tavsiye raporlarını "Okundu/Çözüldü" olarak işaretler.
    public class ResolveInsightUseCase
    {
        private readonly IAiInsightRepository _repository;
        private readonly IStatusBroadcaster _statusBroadcaster;

        public ResolveInsightUseCase(IAiInsightRepository repository, IStatusBroadcaster statusBroadcaster)
        {
            _repository = repository;
            _statusBroadcaster = statusBroadcaster;
        }

        public async Task<bool> ExecuteAsync(Guid insightId)
        {
            var insight = await _repository.GetByIdAsync(insightId);

            if (insight == null) return false;

            // Zaten çözülmüşse veritabanını yormaya gerek yok
            if (insight.IsResolved) return true;

            insight.IsResolved = true;
            insight.ModifiedAt = DateTime.UtcNow;
            
            await _repository.UpdateAsync(insight);

            // 🚨 CANLI BİLDİRİM: Tüm bağlı kullanıcılara bu analizin çözüldüğünü haber ver
            await _statusBroadcaster.BroadcastInsightResolvedAsync(insightId);

            return true;
        }
    }
}
