using Microsoft.Extensions.AI;
using OllamaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Infrastructure.AiServices
{
    // ZERO-TRUST mimarisi gereği dışarı veri çıkmaması gerektiğinde bu sınıf devreye girer.
    public class LocalOllamaClient : IAiAdvisorClient
    {
        private readonly IChatClient _chatClient;

        // Burada da 'modelName' parametresini ekledik. Böylece yarın sunucuya "llama3" veya "mistral" kurarsak, sadece Dashboard'dan ismini değiştirmemiz yetecek.
        public LocalOllamaClient(string? apiUrl, string modelName)
        {
            var endpoint = new Uri(string.IsNullOrWhiteSpace(apiUrl) ? "http://localhost:11434" : apiUrl);

            // Yerel modelin asenkron yanıt üretmesi uzun sürebileceği için 
            // HttpClient'ın zaman aşımına (Timeout) uğrayıp işlemi yarıda kesmesini engelliyoruz.
            var customHttpClient = new HttpClient
            {
                BaseAddress = endpoint,
                Timeout = Timeout.InfiniteTimeSpan
            };

            // Eğer veritabanından gelen modelName boşsa, güvende kalmak adına varsayılan olarak "phi3" kullanıyoruz.
            string activeModel = string.IsNullOrWhiteSpace(modelName) ? "phi3:medium" : modelName;
            _chatClient = new OllamaApiClient(customHttpClient, activeModel);
        }

        public async Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
                return response.Text ?? "Yerel yapay zeka (Ollama) yanıt üretemedi.";
            }
            catch (Exception ex)
            {
                // Arka planda Docker veya Windows Servisi olarak çalışan Ollama durmuş olabilir.
                // Sistemi kitlemeden kontrollü bir şekilde uyarı veriyoruz.
                return $"Ollama Bağlantı Hatası: Lütfen arkada Ollama'nın çalıştığından emin olun. Detay: {ex.Message}";
            }
        }
    }
}