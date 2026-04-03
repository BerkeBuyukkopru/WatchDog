using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.Interfaces.Common
{
    // 1. Tip: Geriye bir değer döndürmeyen (Sadece eylem yapan) Use Case'ler için.
    // Örnek: AnalyzeSystemHealthUseCase (Sadece analiz yapar, ekrana veri dönmez)
    public interface IUseCaseAsync<in TRequest>
    {
        Task ExecuteAsync(TRequest request);
    }

    // 2. Tip: Geriye bir değer döndüren (Arayüze veri gönderen) Use Case'ler için.
    // Örnek: GetAiInsightsUseCase (Veritabanından okur, listeyi döner)
    public interface IUseCaseAsync<in TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest request);
    }
}
