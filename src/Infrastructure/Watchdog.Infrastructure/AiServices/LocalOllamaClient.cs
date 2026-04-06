using Microsoft.Extensions.AI;
using OllamaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Infrastructure.AiServices
{
    // ZERO-TRUST (Sıfır Güven) Mimarisi için tasarlanmış yerel yapay zeka motorudur.
    // Kurum dışarı veri çıkarmak istemediğinde (veya internet koptuğunda) sunucu içindeki Ollama (Phi-3 modeli) ile tamamen kapalı devre çalışır.
    public class LocalOllamaClient : IAiAdvisorClient
    {
        private readonly IChatClient _chatClient;

        public LocalOllamaClient(string? apiUrl)
        {
            // Veritabanında özel bir adres yoksa varsayılan yerel Ollama portu (11434) kullanılır.
            var endpoint = new Uri(string.IsNullOrWhiteSpace(apiUrl) ? "http://localhost:11434" : apiUrl);

            // C#'ın araya girip bağlantıyı koparmasını KESİNLİKLE engelliyoruz.
            var customHttpClient = new HttpClient
            {
                BaseAddress = endpoint,
                Timeout = Timeout.InfiniteTimeSpan // SINIRSIZ bekle!
            };

            // OllamaSharp, Microsoft'un IChatClient standardını doğrudan destekler. 
            // Model olarak hızlı ve hafif olan "phi3" tercih edilmiştir.
            _chatClient = new OllamaApiClient(customHttpClient, "phi3");
        }

        public async Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                // İşlemci veya RAM'i yormadan yerel modelden asenkron yanıt istiyoruz.
                var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
                return response.Text ?? "Yerel yapay zeka (Ollama) yanıt üretemedi.";
            }
            catch (Exception ex)
            {
                // OLLAMA ÇÖKÜŞ SENARYOSU: Arka planda Ollama servisi kapanırsa sistemi patlatmadan log dönüyoruz.
                return $"Ollama Bağlantı Hatası: Lütfen arkada Ollama'nın çalıştığından emin olun. Detay: {ex.Message}";
            }
        }
    }
}