using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; // Task yapısı için gerekli
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces
{
    // "class" yerine "interface" yazdık
    public interface ISystemConfigurationRepository
    {
        Task<SystemConfiguration?> GetAsync();
        Task<bool> UpdateAsync(SystemConfiguration config);
    }
}