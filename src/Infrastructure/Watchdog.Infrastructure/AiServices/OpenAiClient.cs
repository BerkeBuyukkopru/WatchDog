using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Threading;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.ExternalClients;

namespace Watchdog.Infrastructure.AiServices
{
    // Bu sınıf, Strategy deseninin bulut tarafındaki temsilcisidir.
    // Yapılan son güncelleme ile "Universal Cloud Connector" haline getirilmiştir.
    // Artık sadece api.openai.com değil, OpenAI protokolünü destekleyen (Groq, Azure, Mistral vb.)
    // tüm servislerle dinamik URL üzerinden konuşabilir.
    public class OpenAiClient : IAiAdvisorClient
    {
        private readonly Microsoft.Extensions.AI.IChatClient _chatClient;

        // EKİP NOTU: 'apiUrl' parametresi eklendi. Bu değer Dashboard'daki AiApiUrl alanından gelir.
        // Eğer boş gelirse, kütüphane varsayılan olarak orijinal OpenAI adresine gider.
        public OpenAiClient(string apiKey, string modelName, string? apiUrl = null)
        {
            var options = new OpenAIClientOptions();

            // Eğer veritabanından özel bir Proxy veya alternatif servis (Groq vb.) URL'i gelmişse
            // OpenAI istemcisini o adrese yönlendiriyoruz.
            if (!string.IsNullOrWhiteSpace(apiUrl) && Uri.TryCreate(apiUrl, UriKind.Absolute, out var endpoint))
            {
                options.Endpoint = endpoint;
            }

            // HATA ÇÖZÜMÜ: Ana OpenAIClient yerine, doğrudan OpenAI.Chat.ChatClient kullanıyoruz
            // ve ayarları (options) içerisine enjekte ediyoruz.
            var openAiChatClient = new ChatClient(modelName, new ApiKeyCredential(apiKey), options);

            // Microsoft.Extensions.AI standardına uygun hale getiriliyor
            _chatClient = openAiChatClient.AsIChatClient();
        }

        public async Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default)
        {
            // Builder'dan dilsiz (ham) olarak gelen metnin başına Bulut AI için profesyonel Türkçe konuşma emrini ekliyoruz.
            string cloudPrompt = "You MUST output your final diagnostic report strictly in professional Turkish. Do not use English.\n\n" + prompt;

            try
            {
                // Ham prompt yerine, kurallı 'cloudPrompt'u gönderiyoruz
                var response = await _chatClient.GetResponseAsync(cloudPrompt, cancellationToken: cancellationToken);

                // Eğer bulut boş dönerse bunu bir hata sayalım
                if (string.IsNullOrWhiteSpace(response.Text))
                    throw new Exception("Bulut AI boş bir yanıt döndü.");

                return response.Text;
            }
            catch (Exception ex)
            {
                // KRİTİK: Burada artık 'return string' yapmıyoruz. 
                // Hatayı yukarıya (Factory/Fallback katmanına) fırlatıyoruz.
                throw new Exception($"Bulut AI (Groq/OpenAI) Erişim Hatası: {ex.Message}", ex);
            }
        }
    }
}