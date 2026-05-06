import React from 'react';
import { ExternalLink, Activity } from 'lucide-react';

export interface AppStatusData {
  id: string;
  name: string;
  url: string;
  status: 'healthy' | 'unhealthy' | 'degraded' | 'inactive';
  lastCheck: string;
}

interface AppStatusCardProps {
  app: AppStatusData;
  onAnalyze: (id: string) => void;
}

const AppStatusCard: React.FC<AppStatusCardProps> = ({ app, onAnalyze }) => {
  const isHealthy = app.status === 'healthy';
  const isInactive = app.status === 'inactive';
  const isCritical = app.status === 'unhealthy' || app.status === 'degraded';
  
  return (
    <div className={`bg-background-light border rounded-2xl p-4 flex flex-col gap-5 shadow-lg transition-all group relative overflow-hidden ${
      isCritical 
        ? 'border-rose-500 shadow-rose-500/10' 
        : 'border-slate-800 hover:border-indigo-500/30'
    }`}>
      {/* Background Subtle Glow */}
      <div className={`absolute -right-4 -top-4 w-24 h-24 blur-3xl opacity-10 transition-opacity group-hover:opacity-20 ${
        isInactive ? 'bg-slate-500' : (isHealthy ? 'bg-emerald-500' : 'bg-rose-500')
      }`}></div>

      {/* Header: Name and Status */}
      <div className="flex justify-between items-start">
        <div className="flex flex-col gap-0.5">
          <h4 className="text-slate-100 font-black text-sm uppercase tracking-wide group-hover:text-indigo-400 transition-colors">
            {app.name}
          </h4>
          <div className="flex items-center gap-1.5 text-slate-500">
            <ExternalLink size={10} />
            <span className="text-[10px] font-medium break-all">{app.url}</span>
          </div>
        </div>

        {/* Status Indicator */}
        <div className="flex items-center gap-2">
          <div className={`w-2 h-2 rounded-full shadow-[0_0_8px] ${
            isInactive 
              ? 'bg-slate-600 shadow-slate-600/50'
              : (isHealthy 
                  ? 'bg-emerald-500 shadow-emerald-500/50 animate-pulse' 
                  : 'bg-rose-500 shadow-rose-500/50')
          }`}></div>
          {isInactive && <span className="text-[10px] font-black text-slate-500 uppercase tracking-tighter">Kapalı</span>}
        </div>
      </div>

      {/* Manual Analysis Button */}
      <div className="flex flex-col gap-3">
        <div className="h-[1px] w-full bg-slate-800/50"></div>
        <button
          onClick={() => onAnalyze(app.id)}
          className={`w-full py-2.5 bg-slate-900 border border-slate-800 rounded-xl flex items-center justify-center text-[11px] font-black uppercase tracking-[0.1em] transition-all active:scale-95 ${
            isInactive 
            ? 'text-slate-600 cursor-not-allowed' 
            : 'text-slate-400 hover:bg-indigo-500/10 hover:border-indigo-500/40 hover:text-indigo-400'
          }`}
          disabled={isInactive}
        >
          {isInactive ? 'İzleme Devre Dışı' : 'Durumu İncele'}
        </button>
      </div>

      {/* Footer Info */}
      <div className="flex items-center justify-between mt-auto pt-1">
        <div className="flex items-center gap-1 text-[9px] font-bold text-slate-600 uppercase">
          <Activity size={10} />
          Canlı İzleme
        </div>
        <span className="text-[9px] text-slate-600 font-medium">Son Tarama: {app.lastCheck}</span>
      </div>
    </div>
  );
};

export default AppStatusCard;
