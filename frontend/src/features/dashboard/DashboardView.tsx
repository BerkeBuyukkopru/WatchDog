import React, { useEffect, useState } from 'react';
import { AiTower } from '../ai-tower/components/AiTower';
import Metrics from './components/Metrics';
import Incidents from './components/Incidents';
import HealthTable from './components/HealthTable';
import { dashboardService } from './api/dashboardService';
import type { HealthCheckLogDto, AppDto } from '../../types/dashboard.types';
import { AlertCircle, Loader2, AlertTriangle } from 'lucide-react';
import { useOutletContext } from 'react-router-dom';
import { useSignalR } from '../../context/SignalRContext';

const DashboardView: React.FC = () => {
  const [logs, setLogs] = useState<HealthCheckLogDto[]>([]);
  const [apps, setApps] = useState<AppDto[]>([]);
  const [selectedAppId, setSelectedAppId] = useState<string>('');
  const [logCount, setLogCount] = useState<number>(50);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Worker durumu için stateler
  const [isWorkerDead, setIsWorkerDead] = useState<boolean>(false);
  const [lastUpdateText, setLastUpdateText] = useState<string>('');

  const context = useOutletContext<{ setApiError: (val: boolean) => void } | null>();
  const setApiError = context?.setApiError || (() => {});

  const { connection, isConnected } = useSignalR();

  const fetchData = async (count: number = logCount, appId: string = selectedAppId) => {
    try {
      setLoading(true);
      setError(null);
      setApiError(false);
      
      // İlk yüklemede uygulamaları da çek
      if (apps.length === 0) {
        const appsData = await dashboardService.getApps();
        setApps(appsData);
        if (appsData.length > 0 && !appId) {
          appId = appsData[0].id;
          setSelectedAppId(appId);
        }
      }

      // Seçili uygulama varsa verilerini çek
      if (appId) {
        const data = await dashboardService.getLatestLogs(count, appId);
        // Backend'den gelen verinin kronolojik olduğundan emin olalım (Grafikler için)
        setLogs(data);
      } else {
        setLogs([]);
      }
      
    } catch (err) {
      console.error('Error fetching dashboard data:', err);
      setError('Veri alınamadı. Lütfen tekrar deneyiniz. Watchdog API Projenizi kontrol ediniz.');
      setApiError(true);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData(logCount, selectedAppId);
  }, []);

  // === SIGNALR CANLI VERİ DİNLEYİCİSİ ===
  useEffect(() => {
    if (!connection || !isConnected || !selectedAppId) return;

    const handleNewStatus = (newSnapshot: HealthCheckLogDto) => {
      // Sadece seçili uygulama için gelen veriyi işle
      if (newSnapshot.appId === selectedAppId) {
        setLogs(prev => {
          // Eğer bu log zaten varsa ekleme (Duplikasyon kontrolü)
          if (prev.some(l => l.id === newSnapshot.id)) return prev;
          
          // Yeni logu sona ekle (Kronolojik akış)
          const newLogs = [...prev, newSnapshot];
          
          // RAM şişmesini önlemek için belirlenen sayıdan fazlasını (en eskileri) sil
          if (newLogs.length > logCount) {
            return newLogs.slice(newLogs.length - logCount);
          }
          return newLogs;
        });
      }
    };

    connection.on('ReceiveStatusUpdate', handleNewStatus);

    return () => {
      connection.off('ReceiveStatusUpdate', handleNewStatus);
    };
  }, [connection, isConnected, selectedAppId, logCount]);

  const handleAppChange = (appId: string) => {
    setSelectedAppId(appId);
    fetchData(logCount, appId);
  };

  const handleCountChange = (count: number) => {
    setLogCount(count);
    fetchData(count, selectedAppId);
  };

  // Backend kronolojik olarak gönderiyor (Timeline grafik için), bu yüzden en yeni olanı en sondaki eleman
  const latestLog = logs.length > 0 ? logs[logs.length - 1] : null;

  useEffect(() => {
    if (!latestLog) {
      setLastUpdateText('');
      setIsWorkerDead(false);
      return;
    }
    
    const checkTime = () => {
      const now = Date.now();
      // UTC Fix: Backend'den gelen zaman damgasının UTC olduğunu tarayıcıya zorla belirtiyoruz.
      const ts = latestLog.timestamp.endsWith('Z') ? latestLog.timestamp : latestLog.timestamp + 'Z';
      const logTime = new Date(ts).getTime();
      const diff = now - logTime;
      
      if (diff > 5 * 60 * 1000) { // 5 dakika
        setIsWorkerDead(true);
      } else {
        setIsWorkerDead(false);
      }
      
      const diffSec = Math.floor(diff / 1000);
      if (diffSec < 60) {
        setLastUpdateText(`Son veri: ${diffSec} saniye önce alındı`);
      } else {
        const diffMin = Math.floor(diffSec / 60);
        setLastUpdateText(`Son veri: ${diffMin} dakika önce alındı`);
      }
    };

    checkTime(); // İlk hesaplama
    const interval = setInterval(checkTime, 1000);
    return () => clearInterval(interval);
  }, [latestLog]);

  return (
    <div className="h-full w-full flex flex-col">
      {/* Kritik Uyarı Bandı (Worker çöktüyse) */}
      {isWorkerDead && (
        <div className="mb-6 p-4 rounded-lg bg-rose-500/10 border border-rose-500/50 flex items-start gap-3 shadow-lg shrink-0">
          <AlertTriangle className="text-rose-500 shrink-0 mt-0.5" size={24} />
          <div>
            <h4 className="text-rose-400 font-semibold mb-1">⚠️ Sistem Uyarısı: Arka plan izleme servisi (Worker) durmuş veya veritabanı bağlantısı kopmuş olabilir!</h4>
            <p className="text-rose-500/80 text-sm">Son veriler güncel değildir. {lastUpdateText}</p>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-10 gap-6 h-full flex-1 min-h-0">
        {/* Sol Taraf: Metrikler, Hatalar, Tablo (70%) */}
        <div className="lg:col-span-7 flex flex-col gap-6 h-full">
          
          {loading ? (
            <div className="flex-1 flex flex-col items-center justify-center min-h-[400px] border border-slate-800 rounded-xl bg-background-light">
              <Loader2 className="w-10 h-10 text-emerald-500 animate-spin mb-4" />
              <p className="text-slate-400">Veriler yükleniyor...</p>
            </div>
          ) : error ? (
            <div className="flex-1 flex flex-col items-center justify-center min-h-[400px] border border-rose-500/20 rounded-xl bg-rose-500/5">
              <AlertCircle className="w-12 h-12 text-rose-500 mb-4" />
              <p className="text-rose-400 font-medium">{error}</p>
              <button 
                onClick={() => fetchData()}
                className="mt-4 px-4 py-2 bg-rose-500/10 text-rose-500 border border-rose-500/20 rounded-md hover:bg-rose-500/20 transition-colors"
              >
                Tekrar Dene
              </button>
            </div>
          ) : (
            <>
              {/* 1. Metrics Cards */}
              <Metrics latestLog={latestLog} />
              
              {/* 2. Active Incidents */}
              <Incidents selectedAppId={selectedAppId} />
              
              {/* 3. Health Table */}
              <HealthTable 
                logs={logs} 
                apps={apps}
                selectedAppId={selectedAppId}
                logCount={logCount}
                isWorkerDead={isWorkerDead}
                lastUpdateText={lastUpdateText}
                onAppChange={handleAppChange}
                onCountChange={handleCountChange}
                onRefresh={() => fetchData()} 
              />
            </>
          )}

        </div>

        {/* Sağ Taraf: AI Kulesi (30%) - Geliştirici B (Senin) Tarafın */}
        <div className="lg:col-span-3 border border-slate-800 rounded-xl bg-background-light overflow-hidden shadow-2xl">
          <AiTower />
        </div>
      </div>
    </div>
  );
};

export default DashboardView;
