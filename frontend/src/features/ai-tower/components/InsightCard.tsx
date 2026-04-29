import React, { useState } from 'react';
import { Check, AlertCircle, AlertTriangle, Info, Zap } from 'lucide-react';
import type { AiInsight, InsightSeverity } from '../../../types/ai-tower.types';

interface InsightCardProps {
  insight: AiInsight;
  onResolve: (id: string) => void;
}

const severityConfig: Record<InsightSeverity, { color: string; icon: any; bg: string }> = {
  critical: { color: 'text-rose-500', icon: Zap, bg: 'bg-rose-500/10' },
  high: { color: 'text-orange-500', icon: AlertCircle, bg: 'bg-orange-500/10' },
  medium: { color: 'text-amber-500', icon: AlertTriangle, bg: 'bg-amber-500/10' },
  low: { color: 'text-emerald-500', icon: Info, bg: 'bg-emerald-500/10' },
};

// Backend'den gelen InsightType string değerini UI severity tipine eşle
const getSeverityFromType = (type: string): InsightSeverity => {
  switch (type) {
    case 'CrashWarning': return 'critical';
    case 'ScalingAdvice': return 'medium';
    case 'StrategicForecast': return 'low';
    case 'SystemStable': return 'low';
    default: return 'low';
  }
};

export const InsightCard: React.FC<InsightCardProps> = ({ insight, onResolve }) => {
  const [isResolving, setIsResolving] = useState(false);
  const severity = getSeverityFromType(insight.insightType);
  const config = severityConfig[severity];
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
      <div className="bg-white/5 backdrop-blur-md border border-white/10 rounded-2xl p-4 hover:bg-white/10 transition-all duration-300 shadow-xl">
        {/* Severity Indicator Line */}
        <div className={`absolute top-0 left-0 w-1 h-full ${config.color.replace('text', 'bg')}`}></div>
        
        <div className="flex items-start gap-3">
          <div className={`p-2 rounded-lg ${config.bg} ${config.color} shrink-0`}>
            <Icon size={18} />
          </div>
          
          <div className="flex-1 min-w-0">
            <div className="flex items-center justify-between mb-1">
              <span className={`text-[10px] font-bold uppercase tracking-wider ${config.color}`}>
                {insight.insightType}
              </span>
              <span className="text-[10px] text-slate-500 font-medium">
                {new Date(insight.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </span>
            </div>
            
            <h3 className="text-sm font-semibold text-white mb-1 leading-tight truncate">
              {insight.appName}
            </h3>
            
            <p className="text-xs text-slate-400 leading-relaxed mb-2 line-clamp-2">
              {insight.message}
            </p>
          </div>
        </div>

        {/* Action Button - Visible on Hover */}
        <button
          onClick={handleResolve}
          className="absolute top-2 right-2 p-2 bg-emerald-500 text-white rounded-lg opacity-0 translate-y-2 group-hover:opacity-100 group-hover:translate-y-0 transition-all duration-300 shadow-lg shadow-emerald-500/20 hover:bg-emerald-600 active:scale-95 flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-wider"
        >
          <Check size={12} strokeWidth={3} />
          <span>Çözüldü</span>
        </button>
      </div>
    </div>
  );
};
