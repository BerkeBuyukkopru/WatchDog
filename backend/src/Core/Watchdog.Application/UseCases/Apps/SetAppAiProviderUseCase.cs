using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Apps
{
    public class SetAppAiProviderUseCase
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly IAiProviderRepository _aiRepository;

        public SetAppAiProviderUseCase(IMonitoredAppRepository appRepository, IAiProviderRepository aiRepository)
        {
            _appRepository = appRepository;
            _aiRepository = aiRepository;
        }

        public async Task<bool> ExecuteAsync(Guid appId, Guid providerId)
        {
            // 1. İlgili uygulamayı veritabanından bul
            var app = await _appRepository.GetByIdAsync(appId);
            if (app == null) return false;

            // 2. Seçilen sağlayıcıyı doğrula (Varlığı ve Aktifliği)
            var provider = await _aiRepository.GetByIdAsync(providerId);
            if (provider == null || !provider.IsActive)
            {
                // Pasif veya silinmiş bir motoru atayamazsın.
                return false;
            }

            // 3. Uygulamanın yapay zeka beynini yeni seçilen ID ile değiştir
            app.ActiveAiProviderId = providerId;

            // 4. Veritabanını güncelle ve sonucu dön
            return await _appRepository.UpdateAsync(app);
        }
    }
}