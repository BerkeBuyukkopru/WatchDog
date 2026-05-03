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
    public class OpenAiClient : IAiAdvisorClient
    {
        private readonly Microsoft.Extensions.AI.IChatClient _chatClient;
        private static readonly SemaphoreSlim _globalSemaphore = new SemaphoreSlim(2, 2);

        public OpenAiClient(string apiKey, string modelName, string? apiUrl = null)
        {
            var options = new OpenAIClientOptions();

            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                // Groq ve diğerleri için URL sonundaki slash'ı temizle ama /v1'e dokunma.
                var cleanUrl = apiUrl.TrimEnd('/');
                
                if (Uri.TryCreate(cleanUrl, UriKind.Absolute, out var endpoint))
                {
                    options.Endpoint = endpoint;
                }
            }

            var openAiChatClient = new ChatClient(modelName, new ApiKeyCredential(apiKey), options);
            _chatClient = openAiChatClient.AsIChatClient();
        }

        public async Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default)
        {
            int jitter = new Random().Next(1000, 3000);
            await Task.Delay(jitter, cancellationToken);

            await _globalSemaphore.WaitAsync(cancellationToken);
            var startTime = DateTime.Now;
            
            try 
            {
                Console.WriteLine($">>>> [AI-REQUEST-START] Bulut AI İsteği Gönderildi: {startTime:HH:mm:ss}");
                
                string cloudPrompt = "You MUST output your final diagnostic report strictly in professional Turkish. Do not use English.\n\n" + prompt;

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(150));

                var response = await _chatClient.GetResponseAsync(cloudPrompt, cancellationToken: cts.Token);

                if (string.IsNullOrWhiteSpace(response.Text))
                    throw new Exception("Bulut AI boş bir yanıt döndü.");

                var duration = DateTime.Now - startTime;
                Console.WriteLine($">>>> [AI-REQUEST-SUCCESS] Bulut AI Cevap Verdi! Süre: {duration.TotalSeconds:N1}sn");

                return response.Text;
            }
            catch (OperationCanceledException)
            {
                var duration = DateTime.Now - startTime;
                throw new Exception($"Bulut AI isteği {duration.TotalSeconds:N1} saniye sonra zaman aşımına uğradı (Timeout 150s).");
            }
            catch (Exception ex)
            {
                throw new Exception($"Bulut AI Erişim Hatası: {ex.Message}", ex);
            }
            finally
            {
                _globalSemaphore.Release();
            }
        }
    }
}