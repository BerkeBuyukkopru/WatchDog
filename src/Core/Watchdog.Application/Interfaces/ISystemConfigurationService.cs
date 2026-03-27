using System.Threading.Tasks;
using Watchdog.Application.DTOs;

namespace Watchdog.Application.Interfaces
{
    public interface ISystemConfigurationService
    {
        // Mevcut ayarları getiren metot
        Task<SystemConfigDto?> GetConfigAsync();

        // Ayarları güncelleyen metot
        Task<bool> UpdateConfigAsync(SystemConfigDto dto);
    }
}