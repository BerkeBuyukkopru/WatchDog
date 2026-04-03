using System;
using System.Collections.Generic;
using System.Text;

namespace Watchdog.Application.DTOs.Apps
{
    // Parametresiz istek (GetAll için)
    public record GetAllAppsRequest();

    // ID parametreli istek (Delete için)
    public record DeleteAppRequest(Guid Id);
}
