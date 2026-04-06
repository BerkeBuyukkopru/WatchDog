using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.Interfaces.ExternalClients
{
    public interface IAiAdvisorClient
    {
        /// Verilen metni (Prompt) yapay zekaya gönderir ve analiz sonucunu döner.
        /// Arkada Ollama mı, OpenAI mı yoksa Gemini mı çalıştığını Application katmanından gizler.
        Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
