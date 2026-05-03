import React, { useState } from 'react';
import { Check, Info, Zap, Maximize2, BrainCircuit } from 'lucide-react';
import type { AiInsight } from '../../../types/ai-tower.types';

interface InsightCardProps {
  insight: AiInsight;
  onResolve: (id: string) => void;
  onViewDetails: () => void;
}

const typeConfig: Record<string, { title: string; color: string; icon: any; bg: string; border: string }> = {
  'CrashWarning': { 
    title: 'Hata Analizi', 
    color: 'text-rose-400', 
    icon: Zap, 
    bg: 'bg-rose-500/10', 
    border: 'border-rose-500/20' 
  },
  'StrategicForecast': { 
    title: 'Haftalık Analiz', 
    color: 'text-indigo-400', 
    icon: BrainCircuit, 
    bg: 'bg-indigo-500/10', 
    border: 'border-indigo-500/20' 
  },
  'ScalingAdvice': { 
    title: 'Saatlik Analiz', 
    color: 'text-emerald-400', 
    icon: Info, 
    bg: 'bg-emerald-500/10', 
    border: 'border-emerald-500/20' 
  },
  'SystemStable': { 
    title: 'Saatlik Analiz', 
    color: 'text-emerald-400', 
    icon: Check, 
    bg: 'bg-emerald-500/10', 
    border: 'border-emerald-500/20' 
  }
};

export const InsightCard: React.FC<InsightCardProps> = ({ insight, onResolve, onViewDetails }) => {
  const [isResolving, setIsResolving] = useState(false);
  const config = typeConfig[insight.insightType] || { 
    title: 'Sistem Analizi', 
    color: 'text-slate-400', 
    icon: Info, 
    bg: 'bg-white/5', 
    border: 'border-white/10' 
  };
  const Icon = config.icon;

  const handleResolve = () => {
    setIsResolving(true);
    // Exit animasyonu için bekle
    setTimeout(() => {
      onResolve(insight.id);
    }, 300);
  };

  return (
    <div 
      className={`group relative overflow-hidden transition-all duration-300 transform ${
        isResolving ? 'opacity-0 scale-95 translate-x-10' : 'opacity-100 scale-100'
      }`}
    >
      <div className={`bg-background-light border ${config.border} rounded-2xl p-5 hover:bg-slate-800/20 transition-all duration-300 shadow-xl flex flex-col gap-4 relative overflow-hidden`}>
        <div className="flex items-start gap-4">
          <div className={`p-3 rounded-xl ${config.bg} ${config.color} shrink-0`}>
            <Icon size={20} />
          </div>
          
          <div className="flex-1 min-w-0">
            <div className="flex items-center justify-between mb-2">
              <span className={`text-[10px] font-bold uppercase tracking-widest ${config.color}`}>
                {config.title}
              </span>
              <span className="text-[10px] text-slate-500 font-medium">
                {new Date(insight.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </span>
            </div>
            
            <h3 className="text-sm font-bold text-slate-100 mb-2 leading-tight">
              {insight.appName}
            </h3>
            
            <p className="text-xs text-slate-400 leading-relaxed line-clamp-3">
              {insight.message}
            </p>

            {insight.providerName && (
              <div className="mt-3 flex items-center gap-2">
                <span className="flex items-center gap-1 text-[9px] bg-slate-800/50 px-2 py-0.5 rounded-md text-slate-500 font-bold border border-slate-700/50">
                  <BrainCircuit size={10} className="text-indigo-500" />
                  {insight.providerName} / {insight.modelName}
                </span>
              </div>
            )}
          </div>
        </div>

        {/* Action Buttons (Matched with Incidents style) */}
        <div className="flex gap-3 mt-1">
          <button 
            onClick={onViewDetails}
            className={`flex-1 flex items-center justify-center gap-2 py-2 text-xs font-medium ${config.color} bg-white/5 hover:bg-white/10 rounded border ${config.border} hover:border-white/20 transition-all`}
          >
            <Maximize2 size={14} />
            Detayları Gör
          </button>
          
          <button 
            onClick={handleResolve}
            className={`flex-1 flex items-center justify-center gap-2 py-2 text-xs font-medium ${config.color} bg-white/5 hover:bg-white/10 rounded border ${config.border} hover:border-white/20 transition-all`}
          >
            <Check size={14} />
            Anladım
          </button>
        </div>
      </div>
    </div>
  );
};
