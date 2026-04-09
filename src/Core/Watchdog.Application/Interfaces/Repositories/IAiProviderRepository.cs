using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces.Repositories
{
// Kurumsal AI Sağlayıcı Kayıt Defteri için Sözleşme
    public interface IAiProviderRepository
    {
        // Tüm sağlayıcıları getirir
        Task<IEnumerable<AiProvider>> GetAllAsync();

        // Şu an aktif olan sağlayıcıyı getirir
        Task<AiProvider?> GetActiveProviderAsync();

        Task<AiProvider?> GetByIdAsync(Guid id);

        // Bir sağlayıcıyı aktif yapar (Parametre tipi Guid olmalı!)
        Task<bool> SetActiveProviderAsync(Guid id);

        // YENİ: Sağlayıcı bilgilerini (ApiKey vb.) günceller
        Task<bool> UpdateAsync(AiProvider provider);
    }
}
