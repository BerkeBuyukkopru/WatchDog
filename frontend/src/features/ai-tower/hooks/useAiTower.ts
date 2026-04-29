import { useState, useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import type { AiInsight, AiProvider, MonitoredApp } from '../../../types/ai-tower.types';
import { aiTowerService } from '../../../api/aiTowerService';
import { useAuth } from '../../../context/AuthContext';

export const useAiTower = () => {
  const { token } = useAuth();
  const [insights, setInsights] = useState<AiInsight[]>([]);
  const [loading, setLoading] = useState(true);
  const [providers, setProviders] = useState<AiProvider[]>([]);
  const [activeProvider, setActiveProvider] = useState<AiProvider | null>(null);
  const [mainApp, setMainApp] = useState<MonitoredApp | null>(null);
  
  // SignalR bağlantısını saklamak için ref kullanıyoruz
  const connectionRef = useRef<signalR.HubConnection | null>(null);

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

  // 2. SIGNALR BAĞLANTISINI KUR
  useEffect(() => {
    fetchData();

    if (!token) return;

    // SignalR Hub Bağlantısı
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL || 'http://localhost:5226'}/statushub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Yeni Öneri Geldiğinde Tetiklenecek Olay
    connection.on('ReceiveNewInsight', (newInsight: AiInsight) => {
      console.log('Canlı AI Önerisi Geldi:', newInsight);
      
      // Sadece ana uygulamaya aitse veya admin her şeyi görmeye yetkiliyse ekle
      setInsights(prev => {
        // Aynı ID'li kayıt varsa mükerrer ekleme yapma
        if (prev.some(i => i.id === newInsight.id)) return prev;
        
        // Yeni geleni en başa ekle ve listeyi (isteğe bağlı) 10-15 kayıtla sınırla
        const updated = [newInsight, ...prev];
        return updated.slice(0, 15); 
      });
    });

    connection.start()
      .then(() => console.log('SignalR AI Tower Bağlantısı Başarılı.'))
      .catch(err => console.error('SignalR Bağlantı Hatası:', err));

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [token]);

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
