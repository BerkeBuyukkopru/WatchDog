using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Infrastructure.AiServices
{
    // Yapay Zeka istemcilerini (OpenAI, Ollama vb.) çalışma zamanında (runtime) üreten ve yöneten fabrika sınıfı.
    public class AiClientFactory : IAiClientFactory
    {
        private readonly IAiProviderRepository _providerRepository;
        private readonly ILogger<AiClientFactory> _logger; // Kurumsal izleme için Logger eklendi

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
            // Herhangi bir bulut hatasında (401, 429, 500 vb.) sistemin kör kalmaması için yerel Ollama istemcisini bir 'Fallback' seçeneği olarak her zaman hazırlıyoruz.
            var localFallback = new LocalOllamaClient("http://localhost:11434", "phi3:medium");

            // 2. KURAL 1 - KRİTİK FALLBACK: Veritabanında aktif sağlayıcı yoksa sistemi çökertme
            if (activeProvider == null)
            {
                _logger.LogWarning("WatchDog: Aktif bir AI sağlayıcısı bulunamadı! Varsayılan olarak Yerel Ollama (phi3) başlatılıyor.");
                return localFallback;
            }

            _logger.LogInformation("WatchDog: '{ProviderName}' sağlayıcısı üzerinden bağlantı kuruluyor...", activeProvider.Name);

            string modelName = activeProvider.ModelName;
            string apiUrl = activeProvider.ApiUrl ?? "http://localhost:11434";

            // 3. KURAL 2 - BULUT STRATEJİSİ (OpenAI / Groq) + AKILLI YEDEKLEME
            if (activeProvider.Name.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) ||
                activeProvider.Name.Equals("Groq", StringComparison.OrdinalIgnoreCase))
            {
                // Güvenlik Kontrolü: API Key girilmiş mi?
                if (string.IsNullOrWhiteSpace(activeProvider.ApiKey))
                {
                    _logger.LogError("WatchDog: {ProviderName} seçili ancak API Anahtarı eksik! Güvenlik gereği yerel modele dönülüyor.", activeProvider.Name);
                    return localFallback;
                }

                _logger.LogInformation("WatchDog: Bulut AI motoru ({Model}) için Akıllı İstemci (Fallback Destekli) oluşturuluyor.", modelName);

                // Ana istemci olarak Bulut AI (OpenAI/Groq) oluşturulur.
                var cloudClient = new OpenAiClient(activeProvider.ApiKey, modelName, apiUrl);

                // cloudClient'ı FallbackAiAdvisorClient ile sarmalıyoruz. Eğer cloudClient hata fırlatırsa, bu sarmalayıcı otomatik olarak localFallback'e geçecek.
                return new FallbackAiAdvisorClient(cloudClient, localFallback, _logger);
            }

            // 4. KURAL 3 - YEREL MOTOR (Ollama)
            if (activeProvider.Name.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("WatchDog: Yerel AI motoru ({Model}) üzerinden analiz yapılacak.", modelName);
                return new LocalOllamaClient(apiUrl, modelName);
            }

            // 5. Bilinmeyen durumlar için emniyet sibobu
            _logger.LogCritical("WatchDog: Tanımlanamayan AI sağlayıcısı: {ProviderName}. Lokal modele dönülüyor.", activeProvider.Name);
            return localFallback;
        }
    }
}