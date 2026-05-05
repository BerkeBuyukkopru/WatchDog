import { useState, useEffect } from 'react';
import type { AiInsight, AiProvider, MonitoredApp } from '../../../types/ai-tower.types';
import { aiTowerService } from '../../../api/aiTowerService';
import { useAuth } from '../../../context/AuthContext';
import { useSignalR } from '../../../context/SignalRContext';
import { toast } from 'sonner';

export const useAiTower = (appId?: string) => {
  const { token } = useAuth();
  const [insights, setInsights] = useState<AiInsight[]>([]);
  const [loading, setLoading] = useState(true);
  const [providers, setProviders] = useState<AiProvider[]>([]);
  const [activeProvider, setActiveProvider] = useState<AiProvider | null>(null);
  const [mainApp, setMainApp] = useState<MonitoredApp | null>(null);

  const { connection, isConnected } = useSignalR();

  // 1. BAŞLANGIÇ VERİLERİNİ ÇEK (İlk 5 Kayıt)
  const fetchData = async () => {
    setLoading(true);
    try {
      // Önce uygulamaları ve sağlayıcıları çek
      const [providersData, appsData] = await Promise.all([
        aiTowerService.getProviders(),
        aiTowerService.getApps()
      ]);

      setProviders(providersData);

      // Seçili uygulama varsa onu kullan, yoksa ilkini al
      let app: MonitoredApp | null = null;
      if (appId) {
        app = appsData.find(a => a.id === appId) || null;
      } else if (appsData.length > 0) {
        app = appsData[0];
      }
        
      setMainApp(app);

      // 2. ANALİZLERİ ÇEK (Global - appId filtresi kaldırıldı)
      const insightsData = await aiTowerService.getInsights(undefined, 15);
      setInsights(insightsData);

      // 3. AKTİF SAĞLAYICIYI BELİRLE (Uygulamaların o an kullandığı ID'ye bak)
      const currentProviderId = appsData[0]?.activeAiProviderId;
      const active = providersData.find(p => p.id === currentProviderId) || providersData.find(p => p.isActive);
      setActiveProvider(active || providersData[0] || null);

    } catch (error) {
      console.error('AI Tower başlangıç verisi çekme hatası:', error);
    } finally {
      setLoading(false);
    }
  };

  // 2. MERKEZİ SIGNALR BAĞLANTISINI DİNLE
  useEffect(() => {
    fetchData();

    if (!connection || !isConnected) return;

    // Yeni Öneri Geldiğinde Tetiklenecek Olay
    const handleNewInsight = (newInsight: AiInsight) => {
      // GLOBAL GÖRÜNÜM: appId filtresi kaldırıldı
      setInsights(prev => {
        if (prev.some(i => i.id === newInsight.id)) return prev;
        return [newInsight, ...prev].slice(0, 15);
      });
    };

    const handleAllInsightsResolved = (resolvedAppId: string) => {
      // Belirtilen uygulamaya ait TÜM analizleri listeden temizle
      setInsights(prev => prev.filter(i => i.appId !== resolvedAppId));
    };

    const handleSingleInsightResolved = (insightId: string) => {
      // Sadece belirtilen analizi listeden temizle
      setInsights(prev => prev.filter(i => i.id !== insightId));
    };

    connection.on('ReceiveNewInsight', handleNewInsight);
    connection.on('ReceiveAllInsightsResolved', handleAllInsightsResolved);
    connection.on('ReceiveInsightResolved', handleSingleInsightResolved);

    // 🔄 FALLBACK POLLING: SignalR kaçarsa diye her 30sn'de bir sessizce tazele
    const pollInterval = setInterval(() => {
      // GLOBAL GÖRÜNÜM: Polling de artık uygulama bağımsız tüm verileri (son 15) çekmeli
      aiTowerService.getInsights(undefined, 15).then(data => {
        setInsights(prev => {
          // Sadece yeni olanları ekle, eskileri koru
          const existingIds = new Set(prev.map(p => p.id));
          const newOnes = data.filter(d => !existingIds.has(d.id));
          if (newOnes.length === 0) return prev;
          return [...newOnes, ...prev].slice(0, 15);
        });
      }).catch(() => { });
    }, 30000);

    return () => {
      connection.off('ReceiveNewInsight', handleNewInsight);
      connection.off('ReceiveAllInsightsResolved', handleAllInsightsResolved);
      connection.off('ReceiveInsightResolved', handleSingleInsightResolved);
      clearInterval(pollInterval);
    };
  }, [connection, isConnected, appId, mainApp?.id, token]);

  const resolveInsight = async (id: string) => {
    try {
      await aiTowerService.resolveInsight(id);
      setInsights((prev) => prev.filter((item) => item.id !== id));
    } catch (error) {
      console.error('Insight çözümleme hatası:', error);
    }
  };

  const changeActiveProvider = async (providerId: string) => {
    try {
      // GLOBAL DEĞİŞİM: Sadece bir uygulama için değil, tüm sistem için aktif sağlayıcıyı değiştiriyoruz.
      // Backend tarafında toggleProvider (set-active) artık tüm uygulamaları güncelliyor.
      await aiTowerService.toggleProvider(providerId);
      
      // Değişiklik sonrası listeyi tazele
      const providersData = await aiTowerService.getProviders();
      setProviders(providersData);
      
      const active = providersData.find(p => p.id === providerId);
      setActiveProvider(active || null);

      toast.success('AI Motoru Güncellendi', {
        description: `Tüm uygulamalar artık ${active?.name}: ${active?.modelName} ile analiz edilecek.`
      });
    } catch (error) {
      console.error('Sağlayıcı değiştirme hatası:', error);
      toast.error('AI Motoru değiştirilemedi.');
    }
  };

  return {
    insights,
    loading,
    resolveInsight,
    providers,
    activeProvider,
    changeActiveProvider,
    refreshInsights: fetchData
  };
};
