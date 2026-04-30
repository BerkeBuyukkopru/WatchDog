import { useState, useEffect } from 'react';
import type { AiInsight, AiProvider, MonitoredApp } from '../../../types/ai-tower.types';
import { aiTowerService } from '../../../api/aiTowerService';
import { useAuth } from '../../../context/AuthContext';
import { useSignalR } from '../../../context/SignalRContext';

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
      } else {
        setInsights([]);
        setActiveProvider(providersData.find(p => p.isActive) || null);
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
      // SADECE SEÇİLİ UYGULAMA İÇİN GELENLERİ EKLE
      if (appId && newInsight.appId !== appId) return;

      setInsights(prev => {
        if (prev.some(i => i.id === newInsight.id)) return prev;
        return [newInsight, ...prev];
      });
    };

    const handleInsightsResolved = (resolvedAppId: string) => {
      // Sadece seçili uygulama çözüldüyse listeyi temizle
      if (appId && resolvedAppId === appId) {
        setInsights([]);
      }
    };

    connection.on('ReceiveNewInsight', handleNewInsight);
    connection.on('ReceiveAllInsightsResolved', handleInsightsResolved);

    // 🔄 FALLBACK POLLING: SignalR kaçarsa diye her 30sn'de bir sessizce tazele
    const pollInterval = setInterval(() => {
      const targetAppId = appId || mainApp?.id;
      if (targetAppId) {
        aiTowerService.getInsights(targetAppId, 5).then(data => {
          setInsights(prev => {
            const newOnes = data.filter(d => !prev.some(p => p.id === d.id));
            if (newOnes.length === 0) return prev;
            return [...newOnes, ...prev].slice(0, 15);
          });
        }).catch(() => { });
      }
    }, 30000);

    return () => {
      connection.off('ReceiveNewInsight', handleNewInsight);
      connection.off('ReceiveAllInsightsResolved', handleInsightsResolved);
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
