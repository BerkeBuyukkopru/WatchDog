using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Infrastructure.AiServices
{
    // Ekip Arkadaşıma Not: Bu sınıf (Factory Deseni), UseCase'lerin arkasında hangi yapay zekanın (OpenAI/Ollama) 
    // çalışacağına çalışma zamanında (runtime) dinamik olarak karar veren "orkestra şefi"dir.
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

            // Veritabanı konfigürasyonu. Boş gelmesi durumunda sistem "phi3:medium" ile güvenli bölgede kalır.
            string modelName = config?.AiModelName ?? "phi3:medium";
            string apiUrl = config?.AiApiUrl ?? "http://localhost:11434";

            // KURAL 1 - FALLBACK: Yapılandırma hiç yoksa risk alma, doğrudan yerel Ollama motorunu ver.
            if (config == null || string.IsNullOrWhiteSpace(config.ActiveAiProvider))
            {
                return new LocalOllamaClient(apiUrl, modelName);
            }

            // KURAL 2 - DİNAMİK BULUT STRATEJİSİ:
            if (config.ActiveAiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(config.AiApiKey))
                {
                    // (Zero-Trust Fallback) OpenAI seçilmiş ama anahtar yoksa sistemi patlatma,
                    // hemen yereldeki orta sıklet şampiyonuna (phi3:medium) geri dön.
                    return new LocalOllamaClient(apiUrl, "phi3:medium");
                }

                // ARTIK DİNAMİK: OpenAiClient'a sadece anahtar ve model değil, 
                // veritabanındaki apiUrl değerini de geçiyoruz. Bu sayede 'Universal Connector' çalışıyor.
                return new OpenAiClient(config.AiApiKey, modelName, apiUrl);
            }

            // KURAL 3: Kullanıcı açıkça Ollama istediyse...
            if (config.ActiveAiProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                return new LocalOllamaClient(apiUrl, modelName);
            }

            // KURAL 4: Tanımlanamayan bir durumda her zaman lokalde kal (Güvenlik Önlemi).
            return new LocalOllamaClient(apiUrl, modelName);
        }
    }
}