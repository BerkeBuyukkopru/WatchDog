// Dosya: src/Core/Watchdog.Application/Enums/AppErrorCode.cs
namespace Watchdog.Application.Enums
{
    public enum AppErrorCode
    {
        None = 0,               // Hata yok
        UrlAlreadyExists = 1,   // URL zaten var
        DatabaseError = 2,      // Veritabanı hatası
        AppNotFound = 3         // EKSİK OLAN VE HATAYI ÇÖZECEK SATIR

    }
}