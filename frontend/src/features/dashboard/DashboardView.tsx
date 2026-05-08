import React, { useEffect, useState } from 'react';
import { AiTower } from '../ai-tower/components/AiTower';
import Metrics from './components/Metrics';
import Incidents from './components/Incidents';
import HealthTable from './components/HealthTable';
import { dashboardService } from '../../api/dashboardService';
import type { HealthCheckLogDto, AppDto } from '../../types/dashboard.types';
import { AlertCircle, Loader2, AlertTriangle, Clock } from 'lucide-react';
import { useOutletContext } from 'react-router-dom';
import { useSignalR } from '../../context/SignalRContext';
import { systemConfigService, type SystemConfigDto } from '../../api/systemConfigService';

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
  const [config, setConfig] = useState<SystemConfigDto | null>(null);
  const [isExporting, setIsExporting] = useState<boolean>(false);

  const context = useOutletContext<{ 
    setApiError: (val: boolean) => void,
    setSelectedAppName: (val: string) => void 
  } | null>();
  const setApiError = context?.setApiError || (() => { });
  const setSelectedAppName = context?.setSelectedAppName || (() => { });

  const { connection, isConnected } = useSignalR();

  const fetchData = async (count: number = logCount, appId: string = selectedAppId, silent: boolean = false) => {
    try {
      if (!silent) setLoading(true);
      setError(null);
      setApiError(false);

      // İlk yüklemede veya sessiz olmayan yenilemede uygulamaları ve config'i çek
      if (apps.length === 0 || !silent) {
        const [appsData, configData] = await Promise.all([
          dashboardService.getApps(),
          systemConfigService.getConfig()
        ]);
        
        setApps(appsData);
        setConfig(configData);

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
  const handleExport = async (days: number) => {
    if (!selectedAppId) return;
    try {
      setIsExporting(true);
      const appName = apps.find(a => a.id === selectedAppId)?.name;
      await dashboardService.exportHistory(selectedAppId, days, appName);
    } catch (err) {
      console.error('Export failed:', err);
      alert('Dışa aktarma sırasında bir hata oluştu.');
    } finally {
      setIsExporting(false);
    }
  };

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

  const selectedApp = apps.find(a => a.id === selectedAppId);
  const isAppPaused = selectedApp && !selectedApp.isActive;

  // Header'daki uygulama ismini güncelle (TS Fix: Declaration sonrası kullanım)
  useEffect(() => {
    if (selectedApp) {
      setSelectedAppName(selectedApp.name);
    }
    return () => setSelectedAppName('');
  }, [selectedApp, setSelectedAppName]);

  const isAppDown = latestLog?.status === 'Unhealthy' &&
    (latestLog.dependencyDetails?.includes('Network is unreachable') ||
      latestLog.dependencyDetails?.includes('Kritik Ağ Hatası') ||
      latestLog.dependencyDetails?.includes('Connection Error') ||
      latestLog.dependencyDetails?.includes('Timeout:'));

  const isInvalidJson = latestLog?.status === 'Unhealthy' &&
    !isAppDown &&
    !latestLog.dependencyDetails?.trim().startsWith('{');

  return (
    <div className="flex flex-col gap-4 sm:gap-6 animate-in fade-in duration-500">
      {/* Kritik Uyarı Bantları */}
      {(isWorkerDead || (!isWorkerDead && !isAppPaused && (isAppDown || isInvalidJson))) && (
        <div className="flex flex-col gap-4 px-1">
          {isWorkerDead && (
            <div className="p-4 rounded-2xl bg-rose-500/5 border border-rose-500/20 flex items-start sm:items-center gap-4 shadow-xl">
              <div className="p-2.5 rounded-xl bg-rose-500/10 text-rose-500 border border-rose-500/20 shrink-0">
                <AlertTriangle size={20} />
              </div>
              <div>
                <h4 className="text-rose-400 font-bold text-xs sm:text-sm tracking-wide uppercase">Sistem Uyarısı</h4>
                <p className="text-rose-500/80 text-[10px] sm:text-xs font-medium mt-1">Arka plan izleme servisi (Worker) durmuş veya veritabanı bağlantısı kopmuş olabilir! {lastUpdateText}</p>
              </div>
            </div>
          )}

          {isAppPaused && (
            <div className="p-4 rounded-2xl bg-indigo-500/5 border border-indigo-500/20 flex items-start sm:items-center gap-4 shadow-xl">
              <div className="p-2.5 rounded-xl bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 shrink-0">
                <Clock size={20} />
              </div>
              <div>
                <h4 className="text-indigo-100 font-bold text-xs sm:text-sm tracking-wide uppercase">İzleme Duraklatıldı</h4>
                <p className="text-indigo-400/80 text-[10px] sm:text-xs font-medium mt-1">Bu uygulamanın sağlık taraması ve AI analizi ayarlar kısmından duraklatılmıştır.</p>
              </div>
            </div>
          )}

          {!isWorkerDead && !isAppPaused && (isAppDown || isInvalidJson) && (
            <div className={`p-4 sm:p-6 rounded-2xl border flex flex-col sm:flex-row sm:items-center gap-4 shadow-2xl animate-pulse ${isAppDown ? 'bg-gradient-to-r from-rose-600/20 to-rose-900/20 border-rose-500/30' : 'bg-gradient-to-r from-amber-600/20 to-amber-900/20 border-amber-500/30'}`}>
              <div className={`p-3 rounded-full text-white shadow-lg shrink-0 w-fit ${isAppDown ? 'bg-rose-500 shadow-rose-500/50' : 'bg-amber-500 shadow-amber-500/50'}`}>
                <AlertCircle size={24} />
              </div>
              <div>
                <h3 className={`text-lg sm:text-xl font-bold ${isAppDown ? 'text-rose-100' : 'text-amber-100'}`}>
                  {isAppDown ? 'Uygulama çalışmamaktadır!' : 'Sağlık verisi okunamıyor!'}
                </h3>
                <p className={`${isAppDown ? 'text-rose-300' : 'text-amber-300'} text-xs sm:text-sm font-medium`}>
                  {isAppDown
                    ? 'Seçili uygulamanın sağlık kontrolü (Health Check) adresine ulaşılamıyor. Lütfen uygulama durumunu kontrol edin.'
                    : 'Uygulamadan gelen yanıt beklenen JSON formatında değil. Uygulama bir iç hata (500) veriyor olabilir.'}
                </p>
              </div>
            </div>
          )}
        </div>
      )}

      {/* 1. Metrics (Full Width) */}
      <Metrics 
        latestLog={latestLog} 
      />

      {/* 2. Middle Row: Incidents & AI Tower (Responsive Split) */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 lg:h-[550px] shrink-0">
        <div className="lg:col-span-8 h-[500px] sm:h-[550px] lg:h-full flex flex-col overflow-hidden">
          <Incidents />
        </div>
        <div className="lg:col-span-4 border border-white/10 rounded-2xl bg-white/5 overflow-hidden shadow-xl h-[500px] sm:h-[550px] lg:h-full">
          <AiTower selectedAppId={selectedAppId} />
        </div>
      </div>

      {/* 3. Bottom Row: Health Table (Responsive Scroll) */}
      <div className="w-full overflow-hidden border border-white/10 rounded-2xl bg-white/5 shadow-xl">
        <div className="overflow-x-auto custom-scrollbar">
          <div className="min-w-[800px] sm:min-w-[1000px] lg:min-w-full">
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
                isAppDown={isAppDown}
                lastUpdateText={lastUpdateText}
                latencyThreshold={config?.criticalLatencyThreshold}
                onAppChange={handleAppChange}
                onCountChange={handleCountChange}
                onRefresh={() => fetchData()}
                onExport={handleExport}
                isExporting={isExporting}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardView;
