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
      <div className={`bg-white/5 backdrop-blur-md border ${config.border} rounded-2xl p-4 hover:bg-white/10 transition-all duration-300 shadow-xl flex flex-col gap-3`}>
        {/* Severity Indicator Line */}
        <div className={`absolute top-0 left-0 w-1 h-full ${config.color.replace('text', 'bg')}`}></div>
        
        <div className="flex items-start gap-3">
          <div className={`p-2 rounded-lg ${config.bg} ${config.color} shrink-0`}>
            <Icon size={18} />
          </div>
          
          <div className="flex-1 min-w-0">
            <div className="flex items-center justify-between mb-1">
              <span className={`text-[10px] font-bold uppercase tracking-wider ${config.color}`}>
                {config.title}
              </span>
              <span className="text-[10px] text-slate-500 font-medium">
                {new Date(insight.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </span>
            </div>
            
            <h3 className="text-sm font-semibold text-white mb-1 leading-tight truncate">
              {insight.appName}
            </h3>
            
            <p className="text-xs text-slate-400 leading-relaxed line-clamp-2">
              {insight.message}
            </p>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-2 mt-1">
          <button 
            onClick={onViewDetails}
            className={`flex-1 flex items-center justify-center gap-2 py-2 text-[10px] font-black uppercase tracking-wider bg-white/5 hover:bg-white/10 rounded-xl border border-white/5 transition-all ${config.color}`}
          >
            <Maximize2 size={12} />
            Detayları Gör
          </button>
          
          <button 
            onClick={handleResolve}
            className="px-3 py-2 text-slate-400 hover:text-emerald-400 bg-white/5 hover:bg-emerald-500/10 rounded-xl border border-white/5 hover:border-emerald-500/20 transition-all"
            title="Çözüldü olarak işaretle"
          >
            <Check size={14} />
          </button>
        </div>
      </div>
    </div>
  );
};
