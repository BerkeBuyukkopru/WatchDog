using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Application.DTOs;

namespace Watchdog.Application.Interfaces
{
    // UI (React) ile Veritabanı (Repository) arasındaki tüm mantıksal köprüyü kurar.
    public interface IAppService
    {
        //İzlenen tüm uygulamaları listeler.
        Task<IEnumerable<AppDto>> GetAllAppsAsync();

        Task<(bool IsSuccess, string ErrorMessage, string ErrorCode, string ApiKey, Guid? Id)> AddAppAsync(CreatedAppDto dto);
        Task<bool> DeleteAppAsync(Guid id);
    }
}
