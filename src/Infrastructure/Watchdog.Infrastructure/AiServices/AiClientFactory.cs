using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Infrastructure.AiServices
{
    // Yapay Zeka istemcilerini çalışma zamanında (runtime) üreten ve yöneten fabrika sınıfı.
    public class AiClientFactory : IAiClientFactory
    {
        private readonly IAiProviderRepository _providerRepository;
        private readonly ILogger<AiClientFactory> _logger;

        public AiClientFactory(IAiProviderRepository providerRepository, ILogger<AiClientFactory> logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        public async Task<IAiAdvisorClient> CreateClientAsync()
        {
            // 1. Veritabanından şu an 'Aktif' olan yapay zekayı çekiyoruz
            var activeProvider = await _providerRepository.GetActiveProviderAsync();

            // --- HER ZAMAN BİR YEDEK HAZIRLA ---
            // Herhangi bir bulut hatasında sistemin kör kalmaması için yerel Ollama istemcisini bir 'Fallback' seçeneği olarak her zaman hazırlıyoruz.
            var localFallback = new LocalOllamaClient("http://localhost:11434", "phi3:medium");

            // 2. KRİTİK FALLBACK: Veritabanında aktif sağlayıcı yoksa sistemi çökertme
            if (activeProvider == null)
            {
                _logger.LogWarning("WatchDog: Aktif bir AI sağlayıcısı bulunamadı! Varsayılan olarak Yerel Ollama (phi3) başlatılıyor.");
                return localFallback;
            }

            _logger.LogInformation("WatchDog: '{ProviderName}' sağlayıcısı üzerinden bağlantı kuruluyor...", activeProvider.Name);

            string modelName = activeProvider.ModelName;
            string apiUrl = activeProvider.ApiUrl ?? "http://localhost:11434";

            // İsminde "Ollama" geçenleri doğrudan yerel motor olarak kabul et.
            if (activeProvider.Name.Contains("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("WatchDog: Yerel AI motoru ({Model}) üzerinden analiz yapılacak.", modelName);
                return new LocalOllamaClient(apiUrl, modelName);
            }
            else
            {
                // 4. YENİ MANTIK: Ollama dışındaki TÜM sağlayıcıları (OpenAI, Groq, DeepSeek, Anthropic vb.) Bulut API kabul et.
                // Modern API'lerin neredeyse tamamı OpenAI standartlarını desteklediği için OpenAiClient yapısı ile çalışırlar.

                // Güvenlik Kontrolü: API Key girilmiş mi?
                if (string.IsNullOrWhiteSpace(activeProvider.ApiKey))
                {
                    _logger.LogError("WatchDog: {ProviderName} seçili ancak API Anahtarı eksik! Güvenlik gereği yerel modele dönülüyor.", activeProvider.Name);
                    return localFallback;
                }

                _logger.LogInformation("WatchDog: Bulut AI motoru ({ProviderName} - {Model}) için Akıllı İstemci (Fallback Destekli) oluşturuluyor.", activeProvider.Name, modelName);

                // Ana istemci olarak Bulut AI oluşturulur.
                var cloudClient = new OpenAiClient(activeProvider.ApiKey, modelName, apiUrl);

                // cloudClient'ı FallbackAiAdvisorClient ile sarmalıyoruz. Bulut çökerse/limit dolarsa otomatik olarak localFallback'e geçecek.
                return new FallbackAiAdvisorClient(cloudClient, localFallback, _logger);
            }
        }
    }
}