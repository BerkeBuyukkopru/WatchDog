using System;
using System.Collections.Generic;
using System.Text;
using Watchdog.Domain.Entities;

namespace Watchdog.Application.Interfaces
{
    //Infrastructure katmanı bu kurallara uymak zorundadır.
    public interface IMonitoredAppRepository
    {
        //Tüm izlenen uygulamaları getir. IEnumerable: Sadece "okunabilir" bir liste döner
        Task<IEnumerable<MonitoredApp>> GetAllAsync();

        // Belirli bir uygulamayı ID ile getir. MonitoredApp?: Bulamazsa hata fırlatmak yerine "null" döner.
        Task<MonitoredApp?> GetByIdAsync(Guid id);

        //Yeni bir uygulamayı sisteme ekle. Task<bool>: Kayıt başarılıysa true, veritabanı hatası çıkarsa false döner.
        Task<bool> AddAsync(MonitoredApp app);

        Task<bool> DeleteAsync(Guid id);

        //Aynı URL'in iki kez eklenmesini önleme.
        Task<bool> IsUrlExistAsync(string healthUrl);
    }
}
