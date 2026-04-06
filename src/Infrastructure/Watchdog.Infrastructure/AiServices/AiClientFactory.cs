using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Infrastructure.AiServices
{
    // FACTORY (Fabrika) DESENİ: Sistemin beynidir. UseCase'ler "Bana bir yapay zeka ver" dediğinde, veritabanındaki (SystemConfiguration) aktif ayarlara bakarak duruma en uygun nesneyi (Ollama veya OpenAI) anında üretir.
    public class AiClientFactory : IAiClientFactory
    {
        private readonly ISystemConfigurationRepository _configRepository;

        public AiClientFactory(ISystemConfigurationRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public async Task<IAiAdvisorClient> CreateClientAsync()
        {
            var config = await _configRepository.GetAsync();

            // KURAL 1 - FALLBACK: Eğer veritabanında ayar yoksa risk alma, en güvenli seçenek olan yerel Ollama motorunu ver.
            if (config == null || string.IsNullOrWhiteSpace(config.ActiveAiProvider))
            {
                return new LocalOllamaClient("http://localhost:11434");
            }

            // KURAL 2 - BULUT STRATEJİSİ: Kullanıcı arayüzden OpenAI'ı seçtiyse...
            if (config.ActiveAiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(config.AiApiKey))
                {
                    // Şifre (API Key) girilmemişse, hata fırlatmak (sistemi çökertmek) yerine sessizce güvenli limana (Ollama'ya) geçiş yap (Zero-Trust Fallback).
                    return new LocalOllamaClient(config.AiApiUrl);
                }
                return new OpenAiClient(config.AiApiKey, config.AiApiUrl);
            }

            // KURAL 3 - YEREL STRATEJİ: Kullanıcı açıkça Ollama istediyse...
            if (config.ActiveAiProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                return new LocalOllamaClient(config.AiApiUrl);
            }

            // KURAL 4: Bilinmeyen veya hatalı bir sağlayıcı adı yazıldıysa yine lokalde kal.
            return new LocalOllamaClient(config.AiApiUrl);
        }
    }
}