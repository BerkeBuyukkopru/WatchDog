using System.Threading.Tasks;
using Watchdog.Application.DTOs;

namespace Watchdog.Application.Interfaces
{
    // Dashboard'daki 'Ayarlar' sayfasının ana motorudur.
    public interface ISystemConfigurationService
    {
        // Veritabanındaki ham 'Entity' verisini alıp, Dashboard'un anlayacağı 'SystemConfigDto'ya çevirerek getirir.
        Task<SystemConfigDto?> GetConfigAsync();

        // Ayarları güncelleyen metot
        Task<bool> UpdateConfigAsync(SystemConfigDto dto);
    }
}