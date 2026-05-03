import React from 'react';
import { ChevronDown, RefreshCw } from 'lucide-react';
import type { HealthCheckLogDto, DependencyDetail, AppDto } from '../../../types/dashboard.types';

interface HealthTableProps {
  logs: HealthCheckLogDto[];
  apps: AppDto[];
  selectedAppId: string;
  logCount: number;
  isWorkerDead: boolean;
  lastUpdateText?: string;
  onAppChange: (appId: string) => void;
  onCountChange: (count: number) => void;
  onRefresh: () => void;
  isAppDown?: boolean; // Yeni prop
}

const getStatusColor = (status: any) => {
  const s = String(status || '').toLowerCase();
  if (s.includes('unhealthy') || s === '3') return 'text-rose-400 border-rose-500/30';
  if (s.includes('degraded') || s === '2') return 'text-amber-400 border-amber-500/30';
  if (s.includes('healthy') || s === '1') return 'text-emerald-400 border-emerald-500/30';
  return 'text-slate-400 border-slate-500/30';
};

const getStatusBg = (status: any) => {
  const s = String(status || '').toLowerCase();
  if (s.includes('unhealthy') || s === '3') return 'bg-rose-500 text-white border-transparent';
  if (s.includes('degraded') || s === '2') return 'bg-amber-500 text-white border-transparent';
  if (s.includes('healthy') || s === '1') return 'bg-emerald-500 text-white border-transparent';
  return 'bg-slate-500 text-white border-transparent';
};

const getRowBg = (status: any) => {
  const s = String(status || '').toLowerCase();
  if (s.includes('unhealthy') || s === '3') return 'bg-rose-500/10 hover:bg-rose-500/15';
  if (s.includes('degraded') || s === '2') return 'bg-amber-500/5 hover:bg-amber-500/10';
  return 'hover:bg-slate-800/30';
};

const getDurationColor = (ms: number) => {
  if (ms < 1000) return 'text-slate-200';
  if (ms < 2000) return 'text-amber-400';
  return 'text-rose-500';
};

const HealthTable: React.FC<HealthTableProps> = ({ 
  logs, 
  apps, 
  selectedAppId, 
  logCount, 
  isWorkerDead,
  lastUpdateText,
  onAppChange, 
  onCountChange, 
  onRefresh,
  isAppDown = false // Varsayılan değer
}) => {
  const handleRefresh = () => {
    onRefresh();
  };

  const handleAppSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    onAppChange(e.target.value);
  };

  const handleCountSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const count = parseInt(e.target.value);
    onCountChange(count);
  };

  // JSON string olarak gelen dependencyDetails alanını parse etme fonksiyonu
  const parseDependencies = (jsonString: string): { [key: string]: DependencyDetail } => {
    if (!jsonString) return {};
    
    const trimmed = jsonString.trim();
    if (!trimmed.startsWith('{')) {
      // Eğer JSON değilse (düz metinse), bunu tek bir hata detayı olarak dön
      return {
        "System": { 
          status: "Unhealthy", 
          description: jsonString 
        }
      };
    }

    try {
      return JSON.parse(trimmed);
    } catch (err) {
      console.error('Failed to parse dependencies', err);
      return {
        "Error": { 
          status: "Unhealthy", 
          description: "Veri okuma hatası: " + jsonString 
        }
      };
    }
  };

  return (
    <div className="bg-background-light border border-slate-800 rounded-xl shadow-lg flex flex-col flex-1 overflow-hidden min-h-[400px]">
      {/* Table Header Controls */}
      <div className="p-4 border-b border-slate-800 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-400">Uygulama:</span>
            <div className="relative">
              <select 
                value={selectedAppId} 
                onChange={handleAppSelect}
                className="appearance-none bg-background border border-slate-700 text-slate-200 text-sm rounded-md pl-3 pr-8 py-1.5 focus:outline-none focus:border-indigo-500 cursor-pointer"
              >
                {apps.length === 0 && <option value="">Uygulama Yok</option>}
                {apps.map(app => (
                  <option key={app.id} value={app.id}>{app.name}</option>
                ))}
              </select>
              <ChevronDown size={14} className="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
            </div>
          </div>
          
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-400">Kayıt Sayısı:</span>
            <div className="relative">
              <select 
                value={logCount}
                onChange={handleCountSelect}
                className="appearance-none bg-background border border-slate-700 text-slate-200 text-sm rounded-md pl-3 pr-8 py-1.5 focus:outline-none focus:border-indigo-500 cursor-pointer"
              >
                <option value={10}>10</option>
                <option value={50}>50</option>
                <option value={100}>100</option>
              </select>
              <ChevronDown size={14} className="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
            </div>
          </div>

          <div className="flex items-center gap-3">
            {lastUpdateText && (
              <span className="text-slate-400 text-xs hidden md:inline-block">
                {lastUpdateText}
              </span>
            )}
            <button 
              onClick={handleRefresh}
              className="flex items-center gap-2 px-3 py-1.5 bg-indigo-500/10 text-indigo-400 hover:bg-indigo-500/20 border border-indigo-500/20 rounded-md transition-colors text-sm font-medium"
            >
              <RefreshCw size={14} />
              <span>Yenile</span>
            </button>
          </div>
        </div>
        
        <div className="flex items-center gap-3">
          <div className={`flex items-center gap-2 ${isWorkerDead ? 'text-rose-500' : 'text-emerald-500'}`}>
            <div className={`w-2 h-2 rounded-full ${isWorkerDead ? 'bg-rose-500' : 'bg-emerald-500 animate-pulse'}`}></div>
            <span className="text-xs font-medium">
              {isWorkerDead ? 'Veri Akışı Durdu' : 'Canlı Veri Akışı'}
            </span>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto max-h-[600px] overflow-y-auto custom-scrollbar">
        <table className="w-full text-left text-sm text-slate-300">
          <thead className="text-xs text-slate-400 bg-slate-800/30 uppercase border-b border-slate-800">
            <tr>
              <th className="px-6 py-4 font-medium">Zaman</th>
              <th className="px-6 py-4 font-medium">Süre</th>
              <th className="px-6 py-4 font-medium">Bağımlılıklar (Dependencies)</th>
              <th className="px-6 py-4 font-medium text-right">Genel Durum</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/50">
            {isAppDown ? (
              <tr>
                <td colSpan={4} className="px-6 py-20 text-center">
                  <div className="flex flex-col items-center justify-center text-slate-500">
                    <RefreshCw className="w-8 h-8 mb-3 animate-spin text-slate-700" />
                    <p className="text-lg font-medium text-slate-400">Uygulama Yanıt Vermiyor</p>
                    <p className="text-sm">Log verileri şu an alınamamaktadır. Lütfen üstteki menüden başka bir uygulama seçin.</p>
                  </div>
                </td>
              </tr>
            ) : logs.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-6 py-10 text-center text-slate-500">Kayıt bulunamadı.</td>
              </tr>
            ) : (
              [...logs].reverse().map((log) => {
                const depsMap = parseDependencies(log.dependencyDetails);
                const depKeys = Object.keys(depsMap);
                const duration = log.totalDuration || log.durationMs || 0;

                return (
                  <tr key={log.id} className={`transition-colors ${getRowBg(log.status)}`}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {new Date(log.timestamp).toLocaleString('tr-TR')}
                    </td>
                    <td className={`px-6 py-4 whitespace-nowrap font-medium ${getDurationColor(duration)}`}>
                      {duration}ms
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-wrap gap-2 max-w-3xl">
                        {depKeys.length === 0 ? (
                          <span className="text-xs text-slate-500">Bağımlılık Yok</span>
                        ) : (
                          depKeys.map((key) => {
                            const dep = depsMap[key];
                            // Status'ü obje formatından string formatına çevirme (örn: status: { value: "Healthy" } vs status: "Healthy")
                            // Bazı JSON serializer'lar enum'ları obje veya sayı dönebilir.
                            let statusText = dep.status || 'Unknown';
                            if (typeof dep.status === 'object' && dep.status !== null) {
                              statusText = (dep.status as any).name || (dep.status as any).value || 'Unknown';
                            }
                            
                            // Key temizleme: "Monitor", "_Check", "_Pulse" gibi teknik ekleri arayüzden siliyoruz.
                            const displayKey = key
                              .replace(/\s*Monitor/gi, '')
                              .replace(/_Check/gi, '')
                              .replace(/_Pulse/gi, '')
                              .replace(/\s*Check/gi, '')
                              .replace(/_/g, ' ');

                            return (
                              <span 
                                key={key} 
                                className={`px-2.5 py-1 text-[11px] font-bold border rounded-md whitespace-nowrap min-w-[120px] text-center shadow-sm transition-all hover:scale-105 ${getStatusColor(statusText)}`}
                                title={dep.description || ''}
                              >
                                {displayKey}: {statusText}
                              </span>
                            );
                          })
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <span className={`px-3 py-1 text-xs font-semibold rounded-md border ${getStatusBg(log.status)}`}>
                        {log.status}
                      </span>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default HealthTable;
