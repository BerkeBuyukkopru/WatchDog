using System;
using System.Collections.Generic;
using System.Linq; // Pencere filtrelemeleri (Where) için eklendi
using System.Threading.Tasks;
using Watchdog.Application.DTOs.AI;
using Watchdog.Application.Interfaces.Common;
using Watchdog.Application.Interfaces.ExternalClients;
using Watchdog.Application.Interfaces.Repositories;
using Watchdog.Domain.Entities;
using Watchdog.Domain.Enums;

namespace Watchdog.Application.UseCases.AI
{
    // Saatlik Rutin Kapasite Analizi Senaryosu.
    // Bu sınıf Worker'dan (veya API'den) tetiklenir, son 24 saatin loglarını toplayıp AI Fabrikasına gönderir ve gelen cevabı AiInsight tablosuna kaydeder. Dış dünya (Ollama/OpenAI) bağımsızdır.
    public class GenerateRoutineInsightUseCase : IUseCaseAsync<GenerateRoutineInsightRequest, AiInsight?>
    {
        private readonly IMonitoredAppRepository _appRepository;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IAiInsightRepository _insightRepository;
        private readonly IAiClientFactory _aiClientFactory;
        private readonly ISystemConfigurationRepository _systemConfigRepository;
        private readonly IPromptBuilder _promptBuilder;

        public GenerateRoutineInsightUseCase(
            IMonitoredAppRepository appRepository,
            ISnapshotRepository snapshotRepository,
            IAiInsightRepository insightRepository,
            IAiClientFactory aiClientFactory,
            ISystemConfigurationRepository systemConfigRepository,
            IPromptBuilder promptBuilder)

        {
            _appRepository = appRepository;
            _snapshotRepository = snapshotRepository;
            _insightRepository = insightRepository;
            _aiClientFactory = aiClientFactory;
            _systemConfigRepository = systemConfigRepository;
            _promptBuilder = promptBuilder;
        }

        public async Task<AiInsight?> ExecuteAsync(GenerateRoutineInsightRequest request)
        {
            // Uygulamanın sistemde olup olmadığını kontrol et
            var app = await _appRepository.GetByIdAsync(request.AppId);
            if (app == null) return null;

            // Veritabanından Dashboard üzerinden ayarlanmış dinamik eşik değerlerini getiriyoruz.
            var config = await _systemConfigRepository.GetAsync();

            // (DUAL-MODE) PromptBuilder'a göndermek için aktif yapay zeka sağlayıcısını çekiyoruz. Boş olma ihtimaline karşı varsayılan olarak "Ollama" gönderiyoruz.
            string activeProvider = config?.ActiveAiProvider ?? "Ollama";

            // FALLBACK MANTIĞI: Eğer veritabanında henüz bir ayar satırı yoksa (ilk kurulum vb.), 
            // sistemin çökmemesi için senin belirlediğin kurumsal varsayılan değerleri kullanıyoruz.
            double cpuLimit = config?.CriticalCpuThreshold ?? 90.0;
            double ramLimit = config?.CriticalRamThreshold ?? 90.0;
            double latencyLimit = config?.CriticalLatencyThreshold ?? 1000.0;

            // İstenen zaman dilimine ait geçmiş sağlık verilerini (logları) çek (Artık 24 saatlik büyük veri geliyor)
            var sinceTime = DateTime.UtcNow.AddHours(-request.HoursToAnalyze);
            var snapshots = await _snapshotRepository.GetSnapshotsSinceAsync(request.AppId, sinceTime);

            // Eğer hiç kayıt yoksa AI'ı yormaya gerek yok
            if (snapshots == null || !snapshots.Any()) return null;


            // Son 24 Saatin Genel Ortalaması (Kapasite Analizi İçin)
            double avgCpu24h = Math.Round((double)snapshots.Average(s => s.CpuUsage), 2);
            double avgRam24h = Math.Round((double)snapshots.Average(s => s.RamUsage), 2);
            double avgLatency24h = Math.Round((double)snapshots.Average(s => s.TotalDuration), 2);

            // Son 2 Saatin Yakın Trendi (Anlık Darboğaz Analizi İçin)
            var twoHoursAgo = DateTime.UtcNow.AddHours(-2);
            var snapshots2h = snapshots.Where(s => s.Timestamp >= twoHoursAgo).ToList();

            // Eğer son 2 saatte hiç kayıt yoksa (kesinti vb. yaşanmışsa), null hatası almamak için 24 saatlik değeri fallback (yedek) olarak kullanıyoruz.
            double avgCpu2h = snapshots2h.Any() ? Math.Round((double)snapshots2h.Average(s => s.CpuUsage), 2) : avgCpu24h;
            double avgRam2h = snapshots2h.Any() ? Math.Round((double)snapshots2h.Average(s => s.RamUsage), 2) : avgRam24h;
            double avgLatency2h = snapshots2h.Any() ? Math.Round((double)snapshots2h.Average(s => s.TotalDuration), 2) : avgLatency24h;

            // --- ZENGİNLEŞTİRİLMİŞ ZİRVE (PEAK) ANALİZİ ---
            // Son 2 saat içindeki anlık patlamaları yakalıyoruz.
            var snapshotsForPeak = snapshots2h.Any() ? snapshots2h : snapshots.Take(20).ToList();

            // CPU Zirvesi ve gerçekleştiği an
            var peakCpuRecord = snapshotsForPeak.OrderByDescending(s => s.CpuUsage).First();
            double maxCpu2h = Math.Round((double)peakCpuRecord.CpuUsage, 2);
            string peakCpuTime = peakCpuRecord.Timestamp.ToLocalTime().ToString("HH:mm");

            // RAM ve Latency Zirveleri (Not: TotalDuration long olduğu için double'a cast şart)
            double maxRam2h = Math.Round((double)snapshotsForPeak.Max(s => s.RamUsage), 2);
            double maxLatency2h = Math.Round((double)snapshotsForPeak.Max(s => s.TotalDuration), 2);

            // Alt Bağımlılık (Dependency) Özetleme Algoritması
            // Token tasarrufu yapmak ve AI'ı doğru yönlendirmek için sadece sorunlu (Unhealthy vb.) olan bağımlılıkları süzüyoruz.
            var dependencyIssues = snapshots
                .Where(s => !string.IsNullOrWhiteSpace(s.DependencyDetails) && s.DependencyDetails.Contains("Unhealthy", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.DependencyDetails)
                .Distinct() // Aynı hataları defalarca AI'a yollamamak için tekilleştir
                .Take(5) // En farklı 5 hatayı al
                .ToList();

            // AKILLI BYPASS (ZERO-COMPUTE) MANTIĞI
            // Kontrolü Dashboard'dan gelen dinamik sınırlarla (cpuLimit, ramLimit vb.) yapıyoruz.
            // Sistem stabil mi kontrolü yapıyoruz. Hem 2 saatlik kısa trend hem de 24 saatlik genel kapasite eşiklerin altındaysa ve bağımlılıklar temizse sistem stabildir.
            // Bypass mantığına "Zirve (Max)" değerleri de ekledik (Conservative Bypass). Zirve değerler eşiğin %15 üzerindeyse AI uyanmalı.
            bool isCpuStable = avgCpu2h < cpuLimit && avgCpu24h < cpuLimit && maxCpu2h < (cpuLimit + 15);
            bool isRamStable = avgRam2h < ramLimit && avgRam24h < ramLimit;
            bool isLatencyStable = avgLatency2h < latencyLimit;
            bool isDependenciesClean = !dependencyIssues.Any();

            if (isCpuStable && isRamStable && isLatencyStable && isDependenciesClean)
            {
                // SİSTEM STABİL! AI'ı HİÇ UYANDIRMA. C# OLARAK KENDİN KAYDET VE ÇIK.
                var stableInsight = new AiInsight
                {
                    AppId = request.AppId,
                    InsightType = InsightType.SystemStable, // Yeni Eklediğimiz Enum (Yeşil Durum)
                    Message = $"STATUS: STABLE. Sistem metrikleri Dashboard üzerinde belirlenen ({cpuLimit}% CPU, {ramLimit}% RAM, {latencyLimit}ms Latency) sınırlarının altındadır. Önemli bir anlık patlama (spike) gözlenmedi.",
                    // Kanıt kısmında artık Zirve (Max) değerleri de gösteriliyor.
                    Evidence = $"CPU Avg/Max: {avgCpu2h}%/{maxCpu2h}% | RAM Avg/Max: {avgRam2h}%/{maxRam2h}% | Latency Peak: {maxLatency2h}ms | Bağımlılıklar: Temiz"
                };

                // Tavsiyeyi (stabil durumunu) veritabanına kaydet
                await _insightRepository.AddAsync(stableInsight);
                return stableInsight;
            }

            // EĞER BURAYA GEÇTİYSE SİSTEMDE BİR SIKINTI VAR DEMEKTİR. AI FABRİKASINI UYANDIR!
            // 1. Bağımlılık metnini net, İngilizce log formatına çeviriyoruz
            string dependencyContext = dependencyIssues.Any()
                            ? "STATUS: DEGRADED | ERRORS: " + string.Join(", ", dependencyIssues)
                            : "STATUS: HEALTHY | ALL SUB-SERVICES OPERATIONAL";

            // Verileri IPromptBuilder'a veriyoruz, o bize AI'a yollanacak en uygun soruyu üretiyor. (Temiz Kod & Tek Sorumluluk)
            string aiPrompt = _promptBuilder.BuildRoutinePrompt(
                activeProvider,
                app, cpuLimit, ramLimit, latencyLimit,
                avgCpu24h, avgRam24h, avgLatency24h,
                avgCpu2h, avgRam2h, avgLatency2h,
                maxCpu2h, maxRam2h, maxLatency2h,
                peakCpuTime, dependencyContext);

            // FACTORY DESENİ KULLANIMI: Sistemde o an aktif olan AI motorunu (OpenAI veya Ollama) iste
            var aiClient = await _aiClientFactory.CreateClientAsync();

            // Seçili motora (Interface üzerinden) soruyu sor ve cevabı bekle
            var aiResponseText = await aiClient.AnalyzeAsync(aiPrompt);

            // Gelen cevabı Domain Entity'sine (AiInsight) çevir
            var insight = new AiInsight
            {
                AppId = request.AppId,
                InsightType = InsightType.ScalingAdvice,
                Message = aiResponseText,
                Evidence = $"CPU Peak: {maxCpu2h}% at {peakCpuTime} | RAM Peak: {maxRam2h}% | Latency Peak: {maxLatency2h}ms | Bağımlılıklar: {(dependencyIssues.Any() ? "Sorunlu" : "Temiz")}"
            };

            // Tavsiyeyi veritabanına kaydet
            await _insightRepository.AddAsync(insight);
            return insight;
        }
    }
}