import { useState, useEffect } from 'react';
import type { AiInsight, AiProvider, MonitoredApp } from '../../../types/ai-tower.types';
import { aiTowerService } from '../../../api/aiTowerService';
import { useAuth } from '../../../context/AuthContext';
import { useSignalR } from '../../../context/SignalRContext';

export const useAiTower = () => {
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

      // Eğer adminse sadece ona ait olanları, superadminse listeyi alır
      const app = appsData.length > 0 ? appsData[0] : null;
      setMainApp(app);

      // Ana uygulama varsa veya adminse ve uygulamaları gelmişse analizleri çek
      if (app && app.id) {
        const insightsData = await aiTowerService.getInsights(app.id, 5);
        setInsights(insightsData);

        // Aktif sağlayıcıyı belirle
        if (app.activeAiProviderId) {
          const appSpecific = providersData.find(p => p.id === app.activeAiProviderId);
          setActiveProvider(appSpecific || providersData.find(p => p.isActive) || null);
        } else {
          setActiveProvider(providersData.find(p => p.isActive) || null);
        }
      }
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
      if (mainApp?.id) {
        aiTowerService.getInsights(mainApp.id, 5).then(data => {
          setInsights(prev => {
            // Sadece yeni olanları ekle, eskileri koru
            const existingIds = new Set(prev.map(p => p.id));
            const newOnes = data.filter(d => !existingIds.has(d.id));
            if (newOnes.length === 0) return prev;
            return [...newOnes, ...prev].slice(0, 15);
          });
        }).catch(() => { });
      }
    }, 30000);

    return () => {
      connection.off('ReceiveNewInsight', handleNewInsight);
      connection.off('ReceiveAllInsightsResolved', handleAllInsightsResolved);
      connection.off('ReceiveInsightResolved', handleSingleInsightResolved);
      clearInterval(pollInterval);
    };
  }, [connection, isConnected, mainApp?.id, token]);

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
      if (mainApp) {
        await aiTowerService.setAppProvider(mainApp.id, providerId);
        // Değişiklik sonrası state'i tazele
        const providersData = await aiTowerService.getProviders();
        setProviders(providersData);
        const updatedApp = (await aiTowerService.getApps())[0];
        setMainApp(updatedApp);

        if (updatedApp.activeAiProviderId) {
          setActiveProvider(providersData.find(p => p.id === updatedApp.activeAiProviderId) || null);
        }
      }
    } catch (error) {
      console.error('Sağlayıcı değiştirme hatası:', error);
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
