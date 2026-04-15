using System;
using System.Threading.Tasks;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.Apps
{
    public class SetAppAiProviderUseCase
    {
        private readonly IMonitoredAppRepository _appRepository;

        public SetAppAiProviderUseCase(IMonitoredAppRepository appRepository)
        {
            _appRepository = appRepository;
        }

        public async Task<bool> ExecuteAsync(Guid appId, Guid providerId)
        {
            // 1. İlgili uygulamayı veritabanından bul
            var app = await _appRepository.GetByIdAsync(appId);

            if (app == null)
            {
                return false; // Uygulama bulunamadıysa işlemi iptal et
            }

            // 2. Uygulamanın yapay zeka beynini yeni seçilen ID ile değiştir
            app.ActiveAiProviderId = providerId;

            // 3. Veritabanını güncelle ve sonucu dön
            return await _appRepository.UpdateAsync(app);
        }
    }
}