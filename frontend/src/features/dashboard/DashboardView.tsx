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
  const [logCount, setLogCount] = useState<number>(10);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Worker durumu için stateler
  const [isWorkerDead, setIsWorkerDead] = useState<boolean>(false);
  const [lastUpdateText, setLastUpdateText] = useState<string>('');

  const context = useOutletContext<{ setApiError: (val: boolean) => void } | null>();
  const setApiError = context?.setApiError || (() => { });

  const { connection, isConnected } = useSignalR();

  const fetchData = async (count: number = logCount, appId: string = selectedAppId, silent: boolean = false) => {
    try {
      if (!silent) setLoading(true);
      setError(null);
      setApiError(false);

      // İlk yüklemede veya sessiz olmayan yenilemede uygulamaları çek
      if (apps.length === 0 || !silent) {
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
        setLogs(data);
      } else {
        setLogs([]);
      }

    } catch (err) {
      console.error('Error fetching dashboard data:', err);
      if (!silent) setError('Veri alınamadı. Lütfen tekrar deneyiniz. Watchdog API Projenizi kontrol ediniz.');
      setApiError(true);
    } finally {
      if (!silent) setLoading(false);
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
          if (prev.some(l => l.id === newSnapshot.id)) return prev;
          const newLogs = [...prev, newSnapshot];
          if (newLogs.length > logCount) {
            return newLogs.slice(newLogs.length - logCount);
          }
          return newLogs;
        });
      }
    };

    const handleRefresh = () => {
      console.log('SignalR: Global sistem yenileme sinyali alındı.');
      fetchData(logCount, selectedAppId, true);
    };

    const handleIncidentUpdate = () => {
      // Herhangi bir olay değişikliğinde verileri sessizce tazele
      fetchData(logCount, selectedAppId, true);
    };

    connection.on('ReceiveStatusUpdate', handleNewStatus);
    connection.on('ReceiveSystemRefresh', handleRefresh);
    connection.on('ReceiveNewIncident', handleIncidentUpdate);
    connection.on('ReceiveResolvedIncident', handleIncidentUpdate);

    return () => {
      connection.off('ReceiveStatusUpdate', handleNewStatus);
      connection.off('ReceiveSystemRefresh', handleRefresh);
      connection.off('ReceiveNewIncident', handleIncidentUpdate);
      connection.off('ReceiveResolvedIncident', handleIncidentUpdate);
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

  const isAppDown = latestLog?.status === 'Unhealthy' &&
    (latestLog.dependencyDetails?.includes('Network is unreachable') ||
      latestLog.dependencyDetails?.includes('Kritik Ağ Hatası') ||
      latestLog.dependencyDetails?.includes('Connection Error') ||
      latestLog.dependencyDetails?.includes('Timeout:'));

  const isInvalidJson = latestLog?.status === 'Unhealthy' &&
    !isAppDown &&
    !latestLog.dependencyDetails?.trim().startsWith('{');

  return (
    <div className="h-full w-full flex flex-col gap-6 p-6 overflow-y-auto custom-scrollbar">
      {/* Kritik Uyarı Bantları (En Üstte) */}
      <div className="flex flex-col gap-4">
        {isWorkerDead && (
          <div className="p-4 rounded-lg bg-rose-500/10 border border-rose-500/50 flex items-start gap-3 shadow-lg shrink-0">
            <AlertTriangle className="text-rose-500 shrink-0 mt-0.5" size={24} />
            <div>
              <h4 className="text-rose-400 font-semibold mb-1">⚠️ Sistem Uyarısı: Arka plan izleme servisi (Worker) durmuş veya veritabanı bağlantısı kopmuş olabilir!</h4>
              <p className="text-rose-500/80 text-sm">Son veriler güncel değildir. {lastUpdateText}</p>
            </div>
          </div>
        )}

        {!isWorkerDead && (isAppDown || isInvalidJson) && (
          <div className={`p-6 rounded-xl border flex items-center gap-4 shadow-2xl animate-pulse ${isAppDown ? 'bg-gradient-to-r from-rose-600/20 to-rose-900/20 border-rose-500/30' : 'bg-gradient-to-r from-amber-600/20 to-amber-900/20 border-amber-500/30'}`}>
            <div className={`p-3 rounded-full text-white shadow-lg ${isAppDown ? 'bg-rose-500 shadow-rose-500/50' : 'bg-amber-500 shadow-amber-500/50'}`}>
              <AlertCircle size={24} />
            </div>
            <div>
              <h3 className={`text-xl font-bold ${isAppDown ? 'text-rose-100' : 'text-amber-100'}`}>
                {isAppDown ? 'Uygulama çalışmamaktadır!' : 'Sağlık verisi okunamıyor!'}
              </h3>
              <p className={`${isAppDown ? 'text-rose-300' : 'text-amber-300'} text-sm font-medium`}>
                {isAppDown
                  ? 'Seçili uygulamanın sağlık kontrolü (Health Check) adresine ulaşılamıyor. Lütfen uygulama durumunu kontrol edin.'
                  : 'Uygulamadan gelen yanıt beklenen JSON formatında değil. Uygulama bir iç hata (500) veriyor olabilir.'}
              </p>
            </div>
          </div>
        )}
      </div>

      {/* 1. Metrics (Full Width) */}
      <Metrics latestLog={latestLog} />

      {/* 2. Middle Row: Incidents & AI Tower (Responsive Split) */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 lg:h-[550px] shrink-0">
        <div className="lg:col-span-8 h-[550px] lg:h-full flex flex-col overflow-hidden">
          <Incidents selectedAppId={selectedAppId} />
        </div>
        <div className="lg:col-span-4 border border-slate-800 rounded-xl bg-background-light overflow-hidden shadow-2xl h-[550px] lg:h-full">
          {!isAppDown ? (
            <AiTower selectedAppId={selectedAppId} />
          ) : (
            <div className="h-full flex flex-col items-center justify-center p-8 text-center text-slate-500">
              <div className="w-16 h-16 rounded-full bg-rose-500/10 flex items-center justify-center text-rose-500 mb-4">
                <AlertCircle size={32} />
              </div>
              <h3 className="text-lg font-bold text-slate-300 mb-2">AI Analizi Devre Dışı</h3>
              <p className="text-sm">Uygulama çalışmadığı için yapay zeka analizi yapılamamaktadır.</p>
            </div>
          )}
        </div>
      </div>

      {/* 3. Bottom Row: Health Table (Responsive Scroll) */}
      <div className="w-full overflow-x-auto custom-scrollbar border border-slate-800 rounded-xl bg-background-light shadow-2xl">
        <div className="min-w-[1000px] lg:min-w-full">
          {loading ? (
            <div className="flex flex-col items-center justify-center min-h-[400px]">
              <Loader2 className="w-10 h-10 text-emerald-500 animate-spin mb-4" />
              <p className="text-slate-400">Veriler yükleniyor...</p>
            </div>
          ) : error ? (
            <div className="flex flex-col items-center justify-center min-h-[400px] bg-rose-500/5">
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
            <HealthTable
              logs={logs}
              apps={apps}
              selectedAppId={selectedAppId}
              logCount={logCount}
              isWorkerDead={isWorkerDead}
              isAppDown={isAppDown}
              lastUpdateText={lastUpdateText}
              onAppChange={handleAppChange}
              onCountChange={handleCountChange}
              onRefresh={() => fetchData()}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default DashboardView;
