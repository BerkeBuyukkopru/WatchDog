import React, { useState } from 'react';
import { ChevronDown, RefreshCw, Download, Loader2 } from 'lucide-react';
import type { HealthCheckLogDto, DependencyDetail, AppDto } from '../../../types/dashboard.types';

interface HealthTableProps {
  logs: HealthCheckLogDto[];
  apps: AppDto[];
  selectedAppId: string;
  logCount: number;
  lastUpdateText?: string;
  onAppChange: (appId: string) => void;
  onCountChange: (count: number) => void;
  onRefresh: () => void;
  onExport: (days: number) => void;
  isExporting: boolean;
  isAppDown?: boolean;
  latencyThreshold?: number;
}

const getStatusColor = (status: unknown) => {
  const s = String(status || '').toLowerCase();
  if (s.includes('unhealthy') || s === '3') return 'text-rose-500 border-rose-500/20 bg-rose-500/5';
  if (s.includes('degraded') || s === '2') return 'text-amber-500 border-amber-500/20 bg-amber-500/5';
  if (s.includes('healthy') || s === '1') return 'text-emerald-500 border-emerald-500/20 bg-emerald-500/5';
  return 'text-slate-500 border-white/10 bg-white/5';
};

const getStatusBg = (status: unknown) => {
  const s = String(status || '').toLowerCase();
  if (s.includes('unhealthy') || s === '3') return 'bg-rose-500/10 text-rose-500 border-rose-500/20';
  if (s.includes('degraded') || s === '2') return 'bg-amber-500/10 text-amber-500 border-amber-500/20';
  if (s.includes('healthy') || s === '1') return 'bg-emerald-500/10 text-emerald-500 border-emerald-500/20';
  return 'bg-white/5 text-slate-500 border-white/10';
};

const getRowBg = (status: unknown) => {
  const s = String(status || '').toLowerCase();
  if (s.includes('unhealthy') || s === '3') return 'bg-rose-500/[0.03] hover:bg-rose-500/[0.05]';
  if (s.includes('degraded') || s === '2') return 'bg-amber-500/[0.02] hover:bg-amber-500/[0.04]';
  return 'hover:bg-white/[0.02]';
};

const getDurationColor = (ms: number, threshold: number = 1000) => {
  if (ms < threshold) return 'text-slate-200';
  if (ms < threshold * 2) return 'text-amber-400';
  return 'text-rose-500';
};

const HealthTable: React.FC<HealthTableProps> = ({
  logs,
  apps,
  selectedAppId,
  logCount,
  lastUpdateText,
  onAppChange,
  onCountChange,
  onRefresh,
  onExport,
  isExporting,
  isAppDown = false,
  latencyThreshold = 1000
}) => {
  const [exportDays, setExportDays] = useState<string>('30');

  const handleRefresh = () => {
    onRefresh();
  };

  const handleExport = () => {
    const days = parseInt(exportDays);
    const validatedDays = isNaN(days) ? 30 : Math.min(30, Math.max(1, days));
    setExportDays(validatedDays.toString());
    onExport(validatedDays);
  };

  const handleAppSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    onAppChange(e.target.value);
  };

  const handleCountSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const count = parseInt(e.target.value);
    onCountChange(count);
  };

  const parseDependencies = (jsonString: string): { [key: string]: DependencyDetail } => {
    if (!jsonString) return {};
    const trimmed = jsonString.trim();
    if (!trimmed.startsWith('{')) {
      return {
        "System": {
          status: "Unhealthy",
          description: jsonString
        }
      };
    }
    try {
      return JSON.parse(trimmed) as { [key: string]: DependencyDetail };
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
    <div className="flex flex-col flex-1 overflow-hidden min-h-[400px]">
      {/* Table Header Controls */}
      <div className="p-4 border-b border-white/10 flex flex-col lg:flex-row lg:items-center justify-between gap-4 bg-white/5">
        <div className="flex flex-wrap items-center gap-4 sm:gap-6">
          <div className="flex items-center gap-3">
            <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest whitespace-nowrap">Uygulama</span>
            <div className="relative">
              <select
                value={selectedAppId}
                onChange={handleAppSelect}
                className="appearance-none bg-black/20 border border-white/10 text-slate-200 text-xs font-bold rounded-xl pl-4 pr-10 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 cursor-pointer transition-all hover:bg-black/40 w-full sm:w-auto min-w-[140px]"
              >
                {apps.length === 0 && <option value="">Uygulama Yok</option>}
                {apps.map(app => (
                  <option key={app.id} value={app.id}>{app.name}</option>
                ))}
              </select>
              <ChevronDown size={14} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-500 pointer-events-none" />
            </div>
          </div>

          <div className="flex items-center gap-3">
            <span className="text-[10px] font-black text-slate-500 uppercase tracking-widest whitespace-nowrap">Kayıt</span>
            <div className="relative">
              <select
                value={logCount}
                onChange={handleCountSelect}
                className="appearance-none bg-black/20 border border-white/10 text-slate-200 text-xs font-bold rounded-xl pl-4 pr-10 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 cursor-pointer transition-all hover:bg-black/40"
              >
                <option value={10}>10</option>
                <option value={50}>50</option>
                <option value={100}>100</option>
              </select>
              <ChevronDown size={14} className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-500 pointer-events-none" />
            </div>
          </div>
        </div>

        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between sm:justify-end gap-4 w-full lg:w-auto pt-4 lg:pt-0 border-t lg:border-t-0 border-white/5">
          {lastUpdateText && (
            <span className="text-slate-500 text-[10px] font-bold uppercase tracking-widest hidden sm:inline-block">
              {lastUpdateText}
            </span>
          )}

          <div className="flex flex-wrap items-center gap-3 w-full sm:w-auto">
            {/* Dışa Aktarma Bölümü */}
            <div className="flex items-center gap-2 bg-emerald-500/5 p-1 px-2.5 rounded-xl border border-emerald-500/20 shadow-inner">
              <input
                type="number"
                min="1"
                max="30"
                value={exportDays}
                onChange={(e) => setExportDays(e.target.value)}
                className="w-10 bg-black/40 border border-emerald-500/30 text-emerald-400 text-[11px] font-black rounded-lg px-1.5 py-1 focus:outline-none focus:ring-1 focus:ring-emerald-500/50 text-center"
              />
              <span className="text-[9px] font-black text-emerald-500 uppercase tracking-widest">Gün</span>
              <div className="h-4 w-px bg-emerald-500/20 mx-1"></div>
              <button
                onClick={handleExport}
                disabled={isExporting || !selectedAppId}
                className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-500 disabled:opacity-30 text-white text-[9px] font-black uppercase tracking-widest rounded-lg transition-all active:scale-95"
                title="CSV Olarak İndir"
              >
                {isExporting ? <Loader2 size={12} className="animate-spin" /> : <Download size={12} />}
                <span>İndir</span>
              </button>
            </div>

            <button
              onClick={handleRefresh}
              className="flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white text-[10px] font-black uppercase tracking-widest rounded-xl transition-all shadow-lg shadow-indigo-600/20 active:scale-95"
            >
              <RefreshCw size={14} />
              <span>Yenile</span>
            </button>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto max-h-[600px] overflow-y-auto custom-scrollbar">
        <table className="w-full text-left border-collapse">
          <thead className="text-[10px] font-black uppercase tracking-widest text-slate-500 bg-white/5 border-b border-white/10">
            <tr>
              <th className="px-6 py-4">Zaman Damgası</th>
              <th className="px-6 py-4">Süre</th>
              <th className="px-6 py-4">Bağımlılık Durumu</th>
              <th className="px-6 py-4 text-right">Durum</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
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
                    <td className={`px-6 py-4 whitespace-nowrap font-medium ${getDurationColor(duration, latencyThreshold)}`}>
                      {duration}ms
                    </td>
                    <td className="px-6 py-3">
                      <div className="flex flex-wrap gap-1.5 max-w-full">
                        {depKeys.length === 0 ? (
                          <span className="text-xs text-slate-500">Bağımlılık Yok</span>
                        ) : (
                          depKeys.map((key) => {
                            const dep = depsMap[key];
                            const statusText: string | number = typeof dep.status === 'object' && dep.status !== null
                              ? dep.status.name || dep.status.value || 'Unknown'
                              : dep.status || 'Unknown';

                            const displayKey = key
                              .replace(/\s*Monitor/gi, '')
                              .replace(/_Check/gi, '')
                              .replace(/_Pulse/gi, '')
                              .replace(/\s*Check/gi, '')
                              .replace(/_/g, ' ');

                            const isUnhealthy = String(statusText).toLowerCase().includes('unhealthy') || statusText === '3';
                            const isDegraded = String(statusText).toLowerCase().includes('degraded') || statusText === '2';

                            return (
                              <div
                                key={key}
                                className={`flex items-center gap-1.5 px-2 py-0.5 text-[10px] font-bold border rounded-full shadow-sm transition-all hover:bg-white/5 ${getStatusColor(statusText)} bg-slate-900/40`}
                                title={`${displayKey}: ${statusText}${dep.description ? ` (${dep.description})` : ''}`}
                              >
                                <div className={`w-1.5 h-1.5 rounded-full ${isUnhealthy ? 'bg-rose-500 animate-pulse' :
                                  isDegraded ? 'bg-amber-500' :
                                    'bg-emerald-500'
                                  }`} />
                                <span className="tracking-tight">{displayKey}</span>
                              </div>
                            );
                          })
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <span className={`px-3 py-1 text-[10px] font-black uppercase tracking-widest rounded-lg border ${getStatusBg(log.status)}`}>
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
