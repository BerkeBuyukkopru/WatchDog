using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; // Task yapısı için gerekli
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces
{
    // Bu depo (Repository), MonitoredApp'ten farklı olarak 'Singleton' mantığıyla çalışır.
    public interface ISystemConfigurationRepository
    {
        // Mevcut tüm ayarları (AI sağlayıcısı, CPU/RAM eşikleri vb.) getirir. Task: Veritabanından veri gelene kadar UI'ın donmasını engeller. SystemConfiguration?: Veritabanı henüz boşsa (ilk açılışta) null dönebilir.
        Task<SystemConfiguration?> GetAsync();

        // Dashboard üzerinden değiştirilen ayarları veritabanına kalıcı olarak yazar. Task<bool>: Güncelleme başarılıysa 'true', bir hata oluştuysa 'false' fırlatır.
        Task<bool> UpdateAsync(SystemConfiguration config);
    }
}