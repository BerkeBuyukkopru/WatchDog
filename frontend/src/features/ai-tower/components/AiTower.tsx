import React, { useState, useEffect } from 'react';
import { Sparkles, Loader2, BrainCircuit, ChevronUp, AlertCircle } from 'lucide-react';
import { useAiTower } from '../hooks/useAiTower';
import { InsightCard } from './InsightCard';

export const AiTower: React.FC = () => {
  const { 
    insights, 
    loading, 
    resolveInsight, 
    providers, 
    activeProvider, 
    changeActiveProvider 
  } = useAiTower();
  
  const [isProviderMenuOpen, setIsProviderMenuOpen] = useState(false);
  const [showFallbackLabel, setShowFallbackLabel] = useState(false);
  const [selectedInsight, setSelectedInsight] = useState<AiInsight | null>(null);

  // KRİTİK: Seçili motorun Key'i yoksa fallback durumu
  const isFallbackActive = activeProvider && !activeProvider.hasApiKey && !activeProvider.name.includes('Ollama');

  // Kullanıcı seçimi yaptığında eğer fallback ise 10 saniye boyunca ismi göster
  useEffect(() => {
    if (isFallbackActive) {
      setShowFallbackLabel(true);
      const timer = setTimeout(() => {
        setShowFallbackLabel(false);
      }, 10000); // 10 saniye sonra eski ismine dön
      return () => clearTimeout(timer);
    } else {
      setShowFallbackLabel(false);
    }
  }, [activeProvider?.id, isFallbackActive]);

  return (
    <div className="h-full flex flex-col bg-black/20 backdrop-blur-sm border-l border-white/5 w-full relative">
      {/* Header */}
      <div className="p-6 border-b border-white/5 bg-white/5 shrink-0">
        <div className="flex items-center gap-3 mb-1">
          <div className="p-2 bg-indigo-500/20 rounded-xl text-indigo-400">
            <BrainCircuit size={20} />
          </div>
          <h2 className="text-lg font-bold text-white tracking-tight">AI Insights</h2>
        </div>
        <p className="text-xs text-slate-500 font-medium ml-1">
          Gerçek zamanlı sistem analizi ve çözüm önerileri
        </p>
      </div>

      {/* Content Area */}
      <div className="flex-1 overflow-y-auto custom-scrollbar p-4 space-y-4">
        {loading ? (
          <div className="h-40 flex flex-col items-center justify-center gap-3 text-slate-500">
            <Loader2 size={24} className="animate-spin text-indigo-500" />
            <span className="text-xs font-medium animate-pulse">Tavsiyeler analiz ediliyor...</span>
          </div>
        ) : insights.length === 0 ? (
          <div className="h-40 flex flex-col items-center justify-center gap-3 text-center px-4">
            <div className="w-12 h-12 rounded-full bg-emerald-500/10 flex items-center justify-center text-emerald-500 mb-2">
              <Sparkles size={24} />
            </div>
            <h4 className="text-sm font-semibold text-slate-300">Harika Haber!</h4>
            <p className="text-xs text-slate-500">
              Şu anda bekleyen bir öneri bulunmuyor. Sisteminiz stabil durumda.
            </p>
          </div>
        ) : (
          insights.map((insight) => (
            <InsightCard 
              key={insight.id} 
              insight={insight} 
              onResolve={resolveInsight} 
              onViewDetails={() => setSelectedInsight(insight)}
            />
          ))
        )}
      </div>

      {/* Modal / Pop-up */}
      {selectedInsight && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/60 backdrop-blur-md animate-in fade-in duration-300">
          <div 
            className="bg-[#1A1A1E] border border-white/10 rounded-2xl w-full max-w-2xl max-h-[80vh] flex flex-col shadow-2xl animate-in zoom-in-95 duration-300"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="p-6 border-b border-white/5 flex items-center justify-between bg-white/5">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-indigo-500/20 rounded-lg text-indigo-400">
                  <BrainCircuit size={20} />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-white">{selectedInsight.appName} - Analiz Raporu</h3>
                  <p className="text-[10px] text-slate-500 uppercase tracking-widest font-bold">
                    {new Date(selectedInsight.createdAt).toLocaleString('tr-TR')} tarihinde oluşturuldu
                  </p>
                </div>
              </div>
              <button 
                onClick={() => setSelectedInsight(null)}
                className="p-2 hover:bg-white/10 rounded-full text-slate-400 transition-colors"
              >
                <ChevronUp className="rotate-180" size={20} />
              </button>
            </div>
            
            <div className="flex-1 overflow-y-auto p-6 custom-scrollbar text-slate-300 text-sm leading-relaxed whitespace-pre-wrap">
              <div className="p-4 bg-indigo-500/5 border border-indigo-500/10 rounded-xl mb-4 italic text-indigo-300">
                "Sistem hatalarını analiz ettim. İşte kök neden ve çözüm önerilerim:"
              </div>
              {selectedInsight.message}
            </div>

            <div className="p-4 border-t border-white/5 bg-white/5 flex justify-end gap-3">
              <button 
                onClick={() => setSelectedInsight(null)}
                className="px-6 py-2 bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-bold rounded-xl transition-all shadow-lg shadow-indigo-500/20"
              >
                Anladım
              </button>
            </div>
          </div>
          <div className="absolute inset-0 -z-10" onClick={() => setSelectedInsight(null)}></div>
        </div>
      )}

      {/* Fallback Warning (Her zaman görünür ama küçük) */}
      {isFallbackActive && (
        <div className="mx-4 mb-2 p-2 bg-amber-500/10 border border-amber-500/20 rounded-lg flex items-center gap-2">
          <AlertCircle size={14} className="text-amber-500 shrink-0" />
          <span className="text-[10px] text-amber-200 font-medium">
            Key eksik! Analizler <b>Ollama</b> üzerinden yapılıyor.
          </span>
        </div>
      )}

      {/* Footer / Selector */}
      <div className="p-4 bg-white/5 border-t border-white/5 shrink-0 relative z-20">
        <div className="flex items-center justify-between px-2">
          <div className="flex flex-col gap-1">
            <span className="text-[10px] text-slate-500 uppercase tracking-widest font-bold">
              Analiz Durumu
            </span>
            <span className="text-xs text-indigo-400 font-semibold">
              {loading ? '--' : insights.length} Öneri Mevcut
            </span>
          </div>
          
          <div className="flex flex-col items-end gap-1.5 relative">
            <span className="text-xs text-slate-500 uppercase tracking-widest font-black">
              AI Motoru
            </span>
            <button 
              onClick={() => setIsProviderMenuOpen(!isProviderMenuOpen)}
              className={`flex items-center gap-3 px-4 py-2 rounded-xl border transition-all duration-300 group ${
                isFallbackActive 
                ? 'bg-amber-500/10 border-amber-500/40 shadow-[0_0_15px_rgba(245,158,11,0.1)]' 
                : 'bg-white/5 border-white/10 hover:bg-white/10 hover:border-white/20'
              }`}
              disabled={loading}
            >
              <div className={`w-2 h-2 rounded-full animate-pulse shadow-md ${
                isFallbackActive ? 'bg-amber-500 shadow-amber-500/50' : 'bg-emerald-500 shadow-emerald-500/50'
              }`}></div>
              <span className="text-xs text-slate-200 font-black tracking-wide uppercase">
                {loading 
                  ? '...' 
                  : (showFallbackLabel ? 'Ollama (Fallback)' : (activeProvider?.name || 'Seçilmedi'))}
              </span>
              <ChevronUp size={14} className={`text-slate-500 transition-transform duration-300 ${isProviderMenuOpen ? 'rotate-180' : ''}`} />
            </button>

            {/* Provider Menu */}
            {isProviderMenuOpen && (
              <div className="absolute bottom-full right-0 mb-2 w-48 bg-[#1A1A1E] border border-white/10 rounded-xl shadow-2xl overflow-hidden animate-in fade-in slide-in-from-bottom-2 duration-200">
                <div className="p-3 border-b border-white/5 bg-white/10">
                  <span className="text-[11px] font-black text-slate-400 uppercase tracking-widest ml-1">Motoru Değiştir</span>
                </div>
                <div className="p-1.5">
                  {providers.map((p) => {
                    const isSelected = activeProvider?.id === p.id;
                    const hasNoKey = !p.hasApiKey && !p.name.includes('Ollama');

                    return (
                      <button
                        key={p.id}
                        onClick={() => {
                          changeActiveProvider(p.id);
                          setIsProviderMenuOpen(false);
                        }}
                        className={`w-full flex items-center justify-between px-4 py-3 rounded-xl text-xs font-black transition-all duration-200 ${
                          isSelected 
                          ? 'bg-indigo-500/20 text-indigo-300' 
                          : 'text-slate-400 hover:bg-white/10 hover:text-white'
                        }`}
                      >
                        <div className="flex items-center gap-3">
                          <span className="tracking-wide">{p.name}</span>
                          {hasNoKey && <AlertCircle size={14} className="text-amber-500" />}
                        </div>
                        {isSelected && <div className="w-1.5 h-1.5 rounded-full bg-indigo-400 shadow-[0_0_8px_rgba(129,140,248,0.5)]"></div>}
                      </button>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
