using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.Interfaces.ExternalClients
{
    public interface IAiClientFactory
    {
        /// Veritabanındaki SystemConfiguration ayarlarına (ActiveAiProvider) bakar.
        /// Ayarlara göre arka planda LocalOllamaClient veya OpenAiClient üretip döner.
        Task<IAiAdvisorClient> CreateClientAsync();
    }
}
