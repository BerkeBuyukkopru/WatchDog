using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Infrastructure.AiServices
{
    // Kurumun bulut tabanlı, gelişmiş yapay zeka (OpenAI) kullanmak istediğinde devreye giren motor.
    // IAiAdvisorClient arayüzünü uygulayarak, UseCase'lerin arkada kimin çalıştığını bilmeden bu motorla konuşabilmesini sağlar (Strateji Deseni).
    public class OpenAiClient : IAiAdvisorClient
    {
        private readonly Microsoft.Extensions.AI.IChatClient _chatClient;

        public OpenAiClient(string apiKey, string? apiUrl)
        {
            // .NET 10 Microsoft.Extensions.AI standartlarına uygun olarak, OpenAI'ın resmi ChatClient sınıfını oluşturup, bunu sisteme entegre ediyoruz. Model olarak maliyet/performans oranı en iyi olan "gpt-4o-mini" seçilmiştir.
            var chatClient = new ChatClient("gpt-4o-mini", apiKey);
            _chatClient = chatClient.AsIChatClient();
        }

        public async Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                // Prompt'u LLM'e yollayıp asenkron olarak cevabı bekliyoruz.
                var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
                return response.Text ?? "OpenAI yanıt üretemedi.";
            }
            catch (Exception ex)
            {
                // FALLBACK KURALI: API kotası bitse veya internet kopsa bile sistem çökmemeli (Exception fırlatmamalı).
                // Sadece uyarı mesajı dönerek uygulamanın yaşamaya devam etmesini sağlıyoruz.
                return $"OpenAI API Hatası: Lütfen API anahtarınızı ve bakiyenizi kontrol edin. Detay: {ex.Message}";
            }
        }
    }
}