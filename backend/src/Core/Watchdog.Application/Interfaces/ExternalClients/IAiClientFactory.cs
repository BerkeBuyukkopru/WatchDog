using System;
using System.Threading.Tasks;

namespace Watchdog.Application.Interfaces.ExternalClients
{
    public interface IAiClientFactory
    {
        /// <summary>
        /// Gelen ID'ye göre uygulamaya özel yapay zekayı veya veritabanındaki aktif sağlayıcıya bakar.
        /// </summary>
        /// <param name="specificProviderId">Uygulamaya özel atanmış AI sağlayıcısının ID'si (Varsa)</param>
        Task<IAiAdvisorClient> CreateClientAsync(Guid? specificProviderId = null);
    }
}