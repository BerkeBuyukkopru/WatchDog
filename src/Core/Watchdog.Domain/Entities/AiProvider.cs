using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Domain.Entities
{
    // Yapay zeka sağlayıcılarının (OpenAI, Ollama vb.) bilgilerini tutan entity.
    // Kurumsal standart gereği Guid (UUID) kimlik yapısı kullanılmıştır.
    public class AiProvider
    {
        // Kurumsal Güvenlik: Tahmin edilebilir integer ID yerine benzersiz GUID kullanımı.
        public Guid Id { get; set; } = Guid.NewGuid();

        // Örn: "OpenAI", "Ollama", "Groq"
        public string Name { get; set; } = string.Empty;

        // Çalıştırılacak model (gpt-4o, phi3:medium vb.)
        public string ModelName { get; set; } = string.Empty;

        // API Endpoint adresi (Ollama ve Groq gibi servisler için zorunlu)
        public string? ApiUrl { get; set; }

        // Hassas veri: API Key (Dashboard üzerinden güncellenebilir)
        public string? ApiKey { get; set; }

        // Sistemin o an bu sağlayıcıyı kullanıp kullanmadığını belirleyen bayrak
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
