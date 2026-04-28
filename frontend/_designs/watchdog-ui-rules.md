# WATCHDOG FRONTEND - BİLEŞEN VE İŞ MANTIĞI (BUSINESS LOGIC) KURALLARI

Bu dosya, arayüz (UI) görsellerinde yer alan bileşenlerin arka planda nasıl çalışması gerektiğini, state durumlarını ve veritabanı mimarisine uygun form kurallarını içerir. Tüm bileşen geliştirmelerinde görsellerle birlikte bu kurallar referans alınacaktır.

## 1. LAYAOUT VE MİMARİ STANDARTLAR
- **Routing:** Uygulama `react-router-dom` v6 ile çalışır.
  - `/login` -> Kimlik doğrulama sayfası.
  - `/dashboard` -> Sadece `AdminLayout` (Header) ile sarılı izleme ekranı.
  - `/management/*` -> `SuperAdminLayout` (Header + Sidebar) ile sarılı yönetim sayfaları.
- **Kütüphaneler:** İkonlar için `lucide-react`, formlar için `react-hook-form` + `zod` veya `yup`, bildirimler için `sonner` (toast) kullanılacaktır.

---

## 2. SAYFA 1: LOGIN & AUTHENTICATION (KİMLİK DOĞRULAMA)
Bu sayfa tek bir bileşen (`Login.tsx`) içinde 3 aşamalı bir state makinesi (State Machine) olarak çalışmalıdır.

- **Aşama 1 (Login):** Kullanıcı Adı ve Şifre inputları. "Şifremi Unuttum" linkine tıklanınca Aşama 2'ye geçer.
- **Aşama 2 (Forgot Password):** Sadece Email inputu. "Kodu Gönder" butonuna basılınca (API isteği atılır) Aşama 3'e geçer. Geri dönmek için "Giriş ekranına dön" butonu içerir.
- **Aşama 3 (Reset Password):** 6 haneli kod, Yeni Şifre ve Yeni Şifre (Tekrar) inputları içerir. 
  - *Validasyon:* Şifre ve Tekrar Şifre eşleşmiyorsa input çerçeveleri anında kırmızıya (rose-500) dönmeli ve form submit engellenmelidir.
  - Başarılı şifre değişiminde otomatik olarak Aşama 1'e dönülür.

---

## 3. SAYFA 2: SUPER ADMIN DASHBOARD (CRUD VE YÖNETİM)

### A. Admin Yönetimi (`AdminManagement.tsx`)
- **Sekme (Tab) Yapısı:** Tablo, "Aktif Adminler" ve "Silinen Adminler" (Çöp Kutusu) olarak iki sekmeden oluşmalıdır.
- **İşlemler (Aktif):** Düzenle (Kalem), Şifre Sıfırlama Linki Gönder (Anahtar), Sil (Çöp Kutusu).
  - *Kritik:* Buradaki silme işlemi veritabanından Hard Delete yapmaz, IsDeleted=true yapar (Soft Delete).
- **İşlemler (Silinenler):** Sadece "Geri Getir" (RefreshCw / Restore ikonu) bulunur. Tıklanınca kullanıcı aktif sekmeye döner.
- **Ekle/Düzenle Modalı:** - Standart inputlara ek olarak "Rol Seçimi" (Admin/SuperAdmin) Dropdown'ı olmalıdır.
  - Yeni eklemede "Yetkili Olduğu Uygulamalar" (AllowedAppIds) için çoklu seçim yapılabilen bir Checklist/Grid yapısı bulunmalıdır.

### B. İzlenen Uygulamalar (`MonitoredApps.tsx`)
- **Manuel AI Analizi (Kritik):** Tablodaki "İşlemler" sütununda şık bir "Beyin (Brain)" ikonu olmalıdır. Tıklandığında 2 saniyelik bir loading/spinner efekti dönmeli (Manuel analiz tetikleme simülasyonu).
- **Ekle/Düzenle Modalı:** - "Sorumlu Admin Email" (Tekil) ve "Bildirim Emailleri" (Virgülle ayrılabilen Textarea/Tag input) alanları olmalıdır.
  - "AI Sağlayıcısı" atamak için bir Dropdown olmalıdır (Örn: Ollama, OpenAI).
  - Uygulama taramasını durdurmak için "Aktif/Pasif" toggle şalteri olmalıdır.

### C. AI Sağlayıcıları ve Sistem Ayarları
- **AI Providers Modalı:** API Key inputu kesinlikle `type="password"` (gizli) olarak tasarlanmalıdır.
- **System Settings:** CPU ve RAM eşiği slider'larına ek olarak, 100ms - 5000ms aralığında kaydırılabilen "Kritik Gecikme (Latency) Eşiği" slider'ı olmalıdır.

---

## 4. SAYFA 3: ADMIN DASHBOARD (AIOPS CANLI İZLEME)

Bu sayfa dikey olarak %70 Sol (Veri Akışı) ve %30 Sağ (AI Kulesi) şeklinde tasarlanmıştır.

### A. Metrik Kartları (Kritik Ayrıştırma)
CPU ve RAM değerleri tek bir bar üzerinde gösterilmeyecektir.
- **CPU:** "Sunucu Genel" (Örn: %45) ve "Bu Uygulama" (Örn: %15) olarak iki ayrı satırda iki ayrı progress bar (Gauge) çizilecektir.
- **RAM:** "Sunucu Boş Alan" (Örn: 4GB) ve "Bu Uygulama" (Örn: 160MB) olarak iki ayrı satırda iki ayrı progress bar çizilecektir.
- **Disk:** Uygulamaya özel ölçülmez. Sadece "Sunucu D Diski" başlığıyla tek bir kırmızı progress bar (Örn: %85) çizilecektir.

### B. Canlı Sağlık Tablosu (`HealthTable.tsx`)
- **Tablo Üstü Kontrolleri:** "İzlenen Uygulamayı Seç" dropdown'ına ek olarak; tabloda gösterilecek geçmiş "Log Sayısı Seçici" (Select) ve tabloyu manuel "Yenileme" (Refresh) butonu bulunmalıdır.
- **Bağımlılıklar (Dependencies) Sütunu:** Gelen log verisindeki modüller düz yazı değil; durumuna göre `[SQL: Healthy]` (Yeşil), `[Mongo: Unhealthy]` (Kırmızı) şeklinde dinamik Hap (Badge/Pill) tasarımlarında render edilmelidir.
- **Satır Vurgusu:** Tablo satırındaki "Genel Durum" hatalıysa (Degraded/Unhealthy), o satırın arka planı çok hafif saydam kırmızıyla (rose-500/10) vurgulanmalıdır.

### C. AI Insight Kulesi (`AITower.tsx`)
- AI bildirim kartlarının sol kenarları önem derecesine göre renklendirilmelidir (Critical: Kırmızı, Info: Mavi/Mor).
- **Optimistic UI (Çözüldü İşaretleme):** Kartların üzerine fare ile gelindiğinde (Hover), sağ üst köşede "Çözüldü Olarak İşaretle" (CheckCircle) butonu belirmelidir. Butona tıklandığında kart yumuşak bir animasyonla (fade-out / scale-down) listeden silinmelidir.
- **Footer:** Kule en alt kısmında aktif yapay zekayı gösteren sabit (sticky) bir Dropdown barındırmalıdır.