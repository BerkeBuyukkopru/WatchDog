using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities; // AiProvider sınıfı için eklendi

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

        public async Task<IAiAdvisorClient> CreateClientAsync(Guid? specificProviderId = null)
        {
            AiProvider? targetProvider = null;

            // 1. Eğer uygulamaya özel bir AI seçilmişse onu getir
            if (specificProviderId.HasValue)
            {
                targetProvider = await _providerRepository.GetByIdAsync(specificProviderId.Value);
            }

            // 2. Uygulamaya özel AI seçilmemişse GLOBAL Aktif olanı getir
            if (targetProvider == null)
            {
                targetProvider = await _providerRepository.GetActiveProviderAsync();
            }

            // --- DİNAMİK FALLBACK HAZIRLIĞI ---
            // Veritabanından Ollama ayarlarını çekiyoruz. Eğer bulunamazsa güvenli varsayılanları kullanıyoruz.
            var allProviders = await _providerRepository.GetAllAsync();
            var ollamaProvider = allProviders.FirstOrDefault(p => p.Name.Contains("Ollama", StringComparison.OrdinalIgnoreCase));
            
            string fallbackUrl = ollamaProvider?.ApiUrl ?? "http://localhost:11434";
            string fallbackModel = ollamaProvider?.ModelName ?? "phi3:mini";
            var localFallback = new LocalOllamaClient(fallbackUrl, fallbackModel);

            // 3. KRİTİK KONTROL: Veritabanında hiçbir sağlayıcı yoksa veya seçilen sağlayıcı AKTİF değilse Ollama'ya dön.
            if (targetProvider == null || (!targetProvider.IsActive && !targetProvider.Name.Contains("Ollama", StringComparison.OrdinalIgnoreCase)))
            {
                string reason = targetProvider == null ? "Bulunamadı" : "Pasif Durumda";
                _logger.LogWarning("WatchDog: AI sağlayıcısı ({Reason})! Varsayılan olarak Yerel Ollama ({Model}) başlatılıyor.", reason, fallbackModel);
                return localFallback;
            }

            // 4. API KEY KONTROLÜ: Ollama dışındaki sağlayıcılarda anahtar yoksa Ollama'ya dön.
            bool isOllama = targetProvider.Name.Contains("Ollama", StringComparison.OrdinalIgnoreCase);
            if (!isOllama && string.IsNullOrWhiteSpace(targetProvider.ApiKey))
            {
                _logger.LogWarning("WatchDog: {ProviderName} için API Anahtarı eksik! Otomatik olarak Ollama'ya ({Model}) geçiliyor.", targetProvider.Name, fallbackModel);
                return localFallback;
            }

            _logger.LogInformation("WatchDog: '{ProviderName}' sağlayıcısı üzerinden bağlantı kuruluyor...", targetProvider.Name);

            if (isOllama)
            {
                return new LocalOllamaClient(targetProvider.ApiUrl ?? fallbackUrl, targetProvider.ModelName);
            }
            else
            {
                _logger.LogInformation("WatchDog: Bulut AI motoru ({ProviderName}) için Fallback destekli istemci oluşturuluyor.", targetProvider.Name);

                var cloudClient = new OpenAiClient(targetProvider.ApiKey!, targetProvider.ModelName, targetProvider.ApiUrl);
                
                // KRİTİK: Bulut çökerse veya hata verirse sadece YEREL OLLAMA'ya düşer. 
                // Başka bir aktif bulut sağlayıcısına (Groq vb.) otomatik geçiş yapmaz.
                return new FallbackAiAdvisorClient(cloudClient, localFallback, _logger);
            }
        }
    }
}