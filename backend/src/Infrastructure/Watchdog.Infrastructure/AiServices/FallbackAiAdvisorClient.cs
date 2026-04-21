using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Infrastructure.AiServices
{
    public class FallbackAiAdvisorClient : IAiAdvisorClient
    {
        private readonly IAiAdvisorClient _primaryClient;
        private readonly IAiAdvisorClient _fallbackClient;
        private readonly ILogger _logger;

        public FallbackAiAdvisorClient(IAiAdvisorClient primary, IAiAdvisorClient fallback, ILogger logger)
        {
            _primaryClient = primary;
            _fallbackClient = fallback;
            _logger = logger;
        }

        public async Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                // Önce asıl seçili olan (Bulut) AI'yı dene
                return await _primaryClient.AnalyzeAsync(prompt, cancellationToken);
            }
            catch (Exception ex)
            {
                // Eğer bulut patlarsa, hatayı logla ve sessizce Ollama'ya geç!
                _logger.LogWarning("WatchDog RESILIENCE: Bulut AI motoru başarısız! Yerel motor (Ollama) devreye alınıyor. Hata: {Error}", ex.Message);

                return await _fallbackClient.AnalyzeAsync(prompt, cancellationToken);
            }
        }
    }
}
