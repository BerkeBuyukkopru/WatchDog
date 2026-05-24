import React, { useEffect, useState } from 'react';
import SummaryCards from './components/SummaryCards';
import AppStatusCard, { type AppStatusData } from './components/AppStatusCard';
import GlobalAppDetailsModal from './components/GlobalAppDetailsModal';
import { LayoutGrid, RefreshCw, Loader2 } from 'lucide-react';
import { dashboardService } from '../../api/dashboardService';
import type { AppDto, HealthCheckLogDto } from '../../types/dashboard.types';
import { useSignalR } from '../../context/SignalRContext';
import { toast } from 'sonner';

const GlobalDashboard: React.FC = () => {
  const [apps, setApps] = useState<AppDto[]>([]);
  const [statusMap, setStatusMap] = useState<Record<string, HealthCheckLogDto>>({});
  const [loading, setLoading] = useState(true);
  const [lastUpdate, setLastUpdate] = useState<number>(Date.now()); // For forcing re-renders to update "time ago"
  const [selectedApp, setSelectedApp] = useState<{ id: string, name: string } | null>(null);

  const { connection, isConnected } = useSignalR();

  const fetchData = async (silent = false) => {
    try {
      if (!silent) setLoading(true);
      
      const [appsData, logsData] = await Promise.all([
        dashboardService.getApps(),
        dashboardService.getLatestLogs(150) // Fetch enough logs to hopefully get the latest for all apps
      ]);

      setApps(appsData);

      // Build the status map (latest log per app)
      const newStatusMap: Record<string, HealthCheckLogDto> = {};
      
      // Since logs are ordered descending by time from backend, the first one we encounter for an app is its latest.
      // Wait, StatusController.cs says: "React'e gönderirken tekrar eskiden-yeniye doğru sıralıyoruz" -> ascending.
      // If it's ascending, the LAST one we encounter is the newest.
      logsData.forEach(log => {
        if (log.appId) {
          newStatusMap[log.appId] = log; // Overwrites older ones, keeping the latest
        }
      });

      setStatusMap(newStatusMap);
    } catch (error) {
      console.error('Veriler çekilirken hata oluştu:', error);
      toast.error('Global durum verileri alınamadı.');
    } finally {
      if (!silent) setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Update "time ago" texts every minute
  useEffect(() => {
    const interval = setInterval(() => {
      setLastUpdate(Date.now());
    }, 60000);
    return () => clearInterval(interval);
  }, []);

  // SignalR Listeners
  useEffect(() => {
    if (!connection || !isConnected) return;

    const handleNewStatus = (newSnapshot: HealthCheckLogDto) => {
      if (newSnapshot.appId) {
        setStatusMap(prev => ({
          ...prev,
          [newSnapshot.appId!]: newSnapshot
        }));
      }
    };

    const handleRefresh = () => {
      fetchData(true);
    };

    connection.on('ReceiveStatusUpdate', handleNewStatus);
    connection.on('ReceiveSystemRefresh', handleRefresh);
    connection.on('ReceiveNewIncident', handleRefresh);
    connection.on('ReceiveResolvedIncident', handleRefresh);

    return () => {
      connection.off('ReceiveStatusUpdate', handleNewStatus);
      connection.off('ReceiveSystemRefresh', handleRefresh);
      connection.off('ReceiveNewIncident', handleRefresh);
      connection.off('ReceiveResolvedIncident', handleRefresh);
    };
  }, [connection, isConnected]);

  const getTimeAgo = (timestamp?: string, now = Date.now()) => {
    if (!timestamp) return 'Bilinmiyor';
    const ts = timestamp.endsWith('Z') ? timestamp : timestamp + 'Z';
    const logTime = new Date(ts).getTime();
    const diff = Math.floor((now - logTime) / 1000);
    
    if (diff < 60) return `${diff} sn önce`;
    const diffMin = Math.floor(diff / 60);
    if (diffMin < 60) return `${diffMin} dk önce`;
    const diffHour = Math.floor(diffMin / 60);
    return `${diffHour} saat önce`;
  };

  // Convert to AppStatusData for the cards
  const displayApps: AppStatusData[] = apps.map(app => {
    const log = statusMap[app.id];
    const status: 'healthy' | 'unhealthy' | 'degraded' | 'inactive' = !app.isActive
      ? 'inactive'
      : log?.status === 'Healthy'
        ? 'healthy'
        : log?.status === 'Degraded'
          ? 'degraded'
          : log
            ? 'unhealthy'
            : 'degraded';

    return {
      id: app.id,
      name: app.name,
      url: app.healthUrl || 'Bilinmeyen URL',
      status,
      lastCheck: getTimeAgo(log?.timestamp, lastUpdate)
    };
  });

  const handleOpenDetails = (id: string) => {
    const app = apps.find(a => a.id === id);
    if (app) {
      setSelectedApp({ id: app.id, name: app.name });
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px]">
        <Loader2 className="w-10 h-10 text-indigo-500 animate-spin mb-4" />
        <p className="text-slate-400">Sistem durumu yükleniyor...</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6 p-1">
      {/* 1. Üst Özet Alanı */}
      <SummaryCards 
        totalApps={displayApps.length} 
        healthyApps={displayApps.filter(a => a.status === 'healthy').length}
        degradedApps={displayApps.filter(a => a.status === 'degraded').length}
        unhealthyApps={displayApps.filter(a => a.status === 'unhealthy').length}
      />

      {/* 2. İzlenen Uygulamalar Başlığı */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between border-b border-white/5 pb-6 mb-2 gap-4 sm:gap-0">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-indigo-500/10 flex items-center justify-center text-indigo-400 border border-indigo-500/20 shadow-lg shadow-indigo-500/5 shrink-0">
            <LayoutGrid size={24} />
          </div>
          <div>
            <h1 className="text-xl font-black text-white uppercase tracking-[0.2em] leading-tight">Global Durum İzleme</h1>
            <p className="text-xs text-slate-500 font-medium mt-1">Sistemdeki tüm uygulamaların canlı sağlık durumunu takip edin.</p>
          </div>
        </div>
        <button 
          onClick={() => fetchData(true)}
          className="flex items-center justify-center gap-2 px-5 py-2.5 bg-slate-800/40 hover:bg-slate-800 text-slate-400 text-[10px] font-black uppercase tracking-widest rounded-xl border border-slate-700/50 transition-all active:scale-95 shadow-lg w-full sm:w-auto"
        >
          <RefreshCw size={14} />
          Canlı Yenile
        </button>
      </div>

      {/* 3. Uygulamalar Grid Yapısı */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        {displayApps.map((app) => (
          <AppStatusCard 
            key={app.id} 
            app={app} 
            onAnalyze={handleOpenDetails} 
          />
        ))}
      </div>

      {/* 4. Uygulama Detay Modalı */}
      {selectedApp && (
        <GlobalAppDetailsModal 
          appId={selectedApp.id}
          appName={selectedApp.name}
          onClose={() => setSelectedApp(null)}
        />
      )}
    </div>
  );
};

export default GlobalDashboard;
