using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.Repositories;

namespace Watchdog.Application.UseCases.AI
{
    // Dashboard üzerinden gelen "Yapay Zekayı Aktif Et" isteğini işleyen kural seti.
    // Kurumsal standart gereği kimlik tipi Guid olarak güncellenmiştir.
    // Giriş tipi int yerine Guid, dönüş tipi işlemin başarısını belirten bool.
    public class SetActiveAiProviderUseCase : IUseCaseAsync<Guid, bool>
    {
        private readonly IAiProviderRepository _repository;

        public SetActiveAiProviderUseCase(IAiProviderRepository repository)
        {
            _repository = repository;
        }

        // Belirtilen GUID'ye sahip sağlayıcıyı sistemin ana beyni olarak işaretler.
        public async Task<bool> ExecuteAsync(Guid id)
        {
            // Repository katmanındaki Guid bekleyen metoda veri güvenli bir şekilde iletilir.
            return await _repository.SetActiveProviderAsync(id);
        }
    }
}
