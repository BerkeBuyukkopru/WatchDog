import React, { useState } from 'react';
import { Settings, Activity, Clock, RotateCcw, Save, ShieldAlert } from 'lucide-react';

export interface SystemConfig {
  criticalCpuThreshold: number;
  criticalRamThreshold: number;
  criticalLatencyThreshold: number;
  scanTimeoutSeconds: number;
  retryCount: number;
}

interface SystemConfigFormProps {
  initialData?: SystemConfig;
  onSubmit: (data: SystemConfig) => void;
  isLoading?: boolean;
}

const SystemConfigForm: React.FC<SystemConfigFormProps> = ({ 
  initialData, 
  onSubmit,
  isLoading = false 
}) => {
  // Başlangıç değerleri (Varsayılan veya API'den gelen)
  const [config, setConfig] = useState<SystemConfig>(initialData || {
    criticalCpuThreshold: 85,
    criticalRamThreshold: 90,
    criticalLatencyThreshold: 500,
    scanTimeoutSeconds: 20,
    retryCount: 3
  });

  // initialData (API'den) geldiğinde state'i güncelle
  React.useEffect(() => {
    if (initialData) {
      setConfig(initialData);
    }
  }, [initialData]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setConfig(prev => ({
      ...prev,
      [name]: Number(value)
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(config);
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-8 w-full max-w-4xl mx-auto">
      {/* 1. KAYNAK KULLANIM EŞİKLERİ (SLIDERS) */}
      <div className="bg-white/5 border border-white/10 rounded-2xl overflow-hidden shadow-xl">
        <div className="min-h-[52px] px-4 border-b border-white/10 flex items-center gap-3 bg-white/5 py-3">
          <ShieldAlert size={18} className="text-rose-500" />
          <h2 className="text-sm font-black text-slate-100 uppercase tracking-[0.2em]">Kaynak Kullanım Eşikleri</h2>
        </div>
        
        <div className="p-6 space-y-8">
          {/* CPU Slider */}
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <label className="text-xs font-bold text-slate-400 uppercase tracking-widest flex items-center gap-2">
                <Activity size={14} className="text-blue-500" />
                Kritik CPU Eşiği
              </label>
              <div className="flex items-center gap-1.5">
                <input
                  type="number"
                  name="criticalCpuThreshold"
                  min="1"
                  max="100"
                  value={config.criticalCpuThreshold}
                  onChange={handleChange}
                  className="w-16 px-2 py-1 bg-blue-500/10 border border-blue-500/30 text-blue-400 text-xs font-black rounded-md text-center focus:outline-none focus:ring-1 focus:ring-blue-500/50 appearance-none"
                />
                <span className="text-blue-500/70 text-xs font-bold">%</span>
              </div>
            </div>
            <input
              type="range"
              name="criticalCpuThreshold"
              min="1"
              max="100"
              value={config.criticalCpuThreshold}
              onChange={handleChange}
              className="w-full h-1.5 bg-slate-800 rounded-lg appearance-none cursor-pointer accent-blue-500 hover:accent-blue-400 transition-all"
            />
          </div>

          {/* RAM Slider */}
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <label className="text-xs font-bold text-slate-400 uppercase tracking-widest flex items-center gap-2">
                <Activity size={14} className="text-purple-500" />
                Kritik RAM Eşiği
              </label>
              <div className="flex items-center gap-1.5">
                <input
                  type="number"
                  name="criticalRamThreshold"
                  min="1"
                  max="100"
                  value={config.criticalRamThreshold}
                  onChange={handleChange}
                  className="w-16 px-2 py-1 bg-purple-500/10 border border-purple-500/30 text-purple-400 text-xs font-black rounded-md text-center focus:outline-none focus:ring-1 focus:ring-purple-500/50 appearance-none"
                />
                <span className="text-purple-500/70 text-xs font-bold">%</span>
              </div>
            </div>
            <input
              type="range"
              name="criticalRamThreshold"
              min="1"
              max="100"
              value={config.criticalRamThreshold}
              onChange={handleChange}
              className="w-full h-1.5 bg-slate-800 rounded-lg appearance-none cursor-pointer accent-purple-500 hover:accent-purple-400 transition-all"
            />
          </div>

          {/* Latency Slider */}
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <label className="text-xs font-bold text-slate-400 uppercase tracking-widest flex items-center gap-2">
                <Clock size={14} className="text-amber-500" />
                Kritik Gecikme (Latency)
              </label>
              <div className="flex items-center gap-1.5">
                <input
                  type="number"
                  name="criticalLatencyThreshold"
                  min="50"
                  max="5000"
                  step="50"
                  value={config.criticalLatencyThreshold}
                  onChange={handleChange}
                  className="w-20 px-2 py-1 bg-amber-500/10 border border-amber-500/30 text-amber-400 text-xs font-black rounded-md text-center focus:outline-none focus:ring-1 focus:ring-amber-500/50 appearance-none"
                />
                <span className="text-amber-500/70 text-xs font-bold">ms</span>
              </div>
            </div>
            <input
              type="range"
              name="criticalLatencyThreshold"
              min="50"
              max="5000"
              step="50"
              value={config.criticalLatencyThreshold}
              onChange={handleChange}
              className="w-full h-1.5 bg-slate-800 rounded-lg appearance-none cursor-pointer accent-amber-500 hover:accent-amber-400 transition-all"
            />
          </div>
        </div>
      </div>

      {/* 2. TARAMA MOTORU (ENGINE) AYARLARI */}
      <div className="bg-white/5 border border-white/10 rounded-2xl overflow-hidden shadow-xl">
        <div className="min-h-[52px] px-4 border-b border-white/10 flex items-center gap-3 bg-white/5 py-3">
          <Settings size={18} className="text-indigo-500" />
          <h2 className="text-sm font-black text-slate-100 uppercase tracking-[0.2em]">Tarama Motoru (Engine)</h2>
        </div>

        <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-2">
            <label className="text-[10px] font-black text-slate-500 uppercase tracking-[0.1em] ml-1">Timeout (Saniye)</label>
            <div className="relative">
              <Clock size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input
                type="number"
                name="scanTimeoutSeconds"
                value={config.scanTimeoutSeconds}
                onChange={handleChange}
                className="w-full bg-white/5 border border-white/10 rounded-xl py-2.5 pl-10 pr-4 text-sm text-slate-200 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 transition-all"
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-[10px] font-black text-slate-500 uppercase tracking-[0.1em] ml-1">Tekrar Deneme (Retry)</label>
            <div className="relative">
              <RotateCcw size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input
                type="number"
                name="retryCount"
                value={config.retryCount}
                onChange={handleChange}
                className="w-full bg-white/5 border border-white/10 rounded-xl py-2.5 pl-10 pr-4 text-sm text-slate-200 focus:outline-none focus:ring-2 focus:ring-indigo-500/30 transition-all"
              />
            </div>
          </div>
        </div>
      </div>

      {/* 3. SUBMIT BUTTON */}
      <div className="flex justify-end mt-2">
        <button
          type="submit"
          disabled={isLoading}
          className="flex items-center justify-center gap-2 px-8 py-3 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded-xl transition-all shadow-[0_0_20px_rgba(16,185,129,0.2)] hover:shadow-[0_0_25px_rgba(16,185,129,0.4)] active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed w-full sm:w-auto"
        >
          <Save size={18} />
          {isLoading ? 'Kaydediliyor...' : 'Ayarları Kaydet'}
        </button>
      </div>
    </form>
  );
};

export default SystemConfigForm;
