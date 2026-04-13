using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.AI
{
    // AI tarafından üretilen kriz veya tavsiye raporlarını "Okundu/Çözüldü" olarak işaretler.
    public class ResolveInsightUseCase : IUseCaseAsync<Guid, bool>
    {
        private readonly IAiInsightRepository _repository;

        public ResolveInsightUseCase(IAiInsightRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ExecuteAsync(Guid insightId)
        {
            var insight = await _repository.GetByIdAsync(insightId);

            if (insight == null) return false;

            // Zaten çözülmüşse veritabanını yormaya gerek yok
            if (insight.IsResolved) return true;

            insight.IsResolved = true;
            await _repository.UpdateAsync(insight);

            return true;
        }
    }
}
